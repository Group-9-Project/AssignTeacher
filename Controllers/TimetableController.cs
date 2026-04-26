using Newtonsoft.Json;
using SchoolTimetable.Models;
using SchoolTimetable.Services;
using SchoolTimetable.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace SchoolTimetable.Controllers
{
    public class TimetableController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private readonly EmailService emailService = new EmailService();

        public ActionResult Index()
        {
            var teachers = db.Teachers
                .Include("TimetableSlots")
                .Where(t => t.IsActive)
                .OrderBy(t => t.LastName)
                .ToList();
            return View(teachers);
        }

        public ActionResult TeacherView(int teacherId)
        {
            var teacher = db.Teachers.Find(teacherId);
            if (teacher == null) return HttpNotFound();

            var slots = db.TimetableSlots
                .Include("Subject").Include("Class")
                .Where(s => s.TeacherId == teacherId && s.IsActive)
                .ToList();

            var vm = new TimetableViewModel { Teacher = teacher, Slots = slots };
            return View(vm);
        }

        // GET: Timetable/Create
        // This method handles the initial page load when you click "Assign"
        public ActionResult Create(int? teacherId, int? day, int? period)
        {
            var vm = new TimetableSlotFormViewModel
            {
                // Pre-fill fields if they were passed in the URL (from your "Assign" link)
                TeacherId = teacherId ?? 0,
                DayOfWeek = day ?? 1,
                Period = period ?? 1,

                // Populate dropdown lists for the view
                Teachers = db.Teachers.Where(t => t.IsActive).OrderBy(t => t.LastName).ToList(),
                Subjects = db.Subjects.Where(s => s.IsActive).OrderBy(s => s.Name).ToList(),
                Classes = db.Classes.Where(c => c.IsActive).OrderBy(c => c.Name).ToList()
            };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(TimetableSlotFormViewModel vm)
        {
            // Check if the slot is already taken by this teacher OR this class
            bool conflict = db.TimetableSlots.Any(s =>
                s.IsActive &&
                s.DayOfWeek == vm.DayOfWeek &&
                s.Period == vm.Period &&
                (s.TeacherId == vm.TeacherId || s.ClassId == vm.ClassId));

            if (conflict)
            {
                ModelState.AddModelError("", "This time slot is already occupied for this teacher or class.");
            }

            if (ModelState.IsValid)
            {
                var slot = new TimetableSlot
                {
                    TeacherId = vm.TeacherId,
                    SubjectId = vm.SubjectId,
                    ClassId = vm.ClassId,
                    DayOfWeek = vm.DayOfWeek,
                    Period = vm.Period,
                    Room = vm.Room, // Make sure Room is being passed from your Form
                    Notes = vm.Notes,
                    IsActive = true // IMPORTANT: Ensure this is set to true
                };

                db.TimetableSlots.Add(slot);
                db.SaveChanges();

                TempData["Success"] = "Timetable slot created successfully.";
                return RedirectToAction("TeacherView", new { teacherId = vm.TeacherId });
            }

            // If we got this far, something failed, reload the lists for the dropdowns
            vm.Teachers = db.Teachers.Where(t => t.IsActive).OrderBy(t => t.LastName).ToList();
            vm.Subjects = db.Subjects.Where(s => s.IsActive).OrderBy(s => s.Name).ToList();
            vm.Classes = db.Classes.Where(c => c.IsActive).OrderBy(c => c.Name).ToList();

            return View(vm);
        }

        public ActionResult Edit(int id)
        {
            var slot = db.TimetableSlots
                .Include("Teacher").Include("Subject").Include("Class")
                .FirstOrDefault(s => s.Id == id);
            if (slot == null) return HttpNotFound();

            var vm = new TimetableSlotFormViewModel
            {
                Id = slot.Id,
                TeacherId = slot.TeacherId,
                SubjectId = slot.SubjectId,
                ClassId = slot.ClassId,
                DayOfWeek = slot.DayOfWeek,
                Period = slot.Period,
                Room = slot.Room,
                Notes = slot.Notes,
                Teachers = db.Teachers.Where(t => t.IsActive).OrderBy(t => t.LastName).ToList(),
                Subjects = db.Subjects.Where(s => s.IsActive).OrderBy(s => s.Name).ToList(),
                Classes = db.Classes.Where(c => c.IsActive).OrderBy(c => c.Name).ToList()
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(TimetableSlotFormViewModel vm)
        {
            bool conflict = db.TimetableSlots.Any(s =>
                s.IsActive && s.Id != vm.Id && s.DayOfWeek == vm.DayOfWeek && s.Period == vm.Period &&
                (s.TeacherId == vm.TeacherId || s.ClassId == vm.ClassId));

            if (conflict)
                ModelState.AddModelError("", "Conflict: the teacher or class already has a slot at this day and period.");

            if (!ModelState.IsValid)
            {
                vm.Teachers = db.Teachers.Where(t => t.IsActive).ToList();
                vm.Subjects = db.Subjects.Where(s => s.IsActive).ToList();
                vm.Classes = db.Classes.Where(c => c.IsActive).ToList();
                return View(vm);
            }

            var slot = db.TimetableSlots.Find(vm.Id);
            if (slot == null) return HttpNotFound();

            slot.TeacherId = vm.TeacherId;
            slot.SubjectId = vm.SubjectId;
            slot.ClassId = vm.ClassId;
            slot.DayOfWeek = vm.DayOfWeek;
            slot.Period = vm.Period;
            slot.Room = vm.Room;
            slot.Notes = vm.Notes;
            db.SaveChanges();

            TempData["Success"] = "Timetable slot updated.";
            return RedirectToAction("TeacherView", new { teacherId = vm.TeacherId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var slot = db.TimetableSlots.Find(id);
            if (slot != null)
            {
                int teacherId = slot.TeacherId;
                slot.IsActive = false;
                db.SaveChanges();
                TempData["Success"] = "Slot removed.";
                return RedirectToAction("TeacherView", new { teacherId });
            }
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SendTimetable(int teacherId)
        {
            var teacher = db.Teachers.Find(teacherId);
            if (teacher == null) return HttpNotFound();

          
            if (string.IsNullOrEmpty(teacher.TemporaryPassword))
            {
                teacher.TemporaryPassword = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                db.Entry(teacher).State = EntityState.Modified; 
            }

            var slots = db.TimetableSlots
                .Include("Subject").Include("Class")
                .Where(s => s.TeacherId == teacherId && s.IsActive)
                .ToList();

            // Fix: Added teacher.TemporaryPassword as the 4th argument
            bool sent = emailService.SendTimetableEmail(teacher, slots, teacher.Email, teacher.TemporaryPassword);

            db.NotificationLogs.Add(new NotificationLog
            {
                TeacherId = teacherId,
                Subject = "Weekly Timetable & Login Info",
                Body = slots.Count + " slots and credentials sent.",
                Sent = sent,
                Timestamp = DateTime.Now // Updated to match your model property name
            });
            db.SaveChanges();

            TempData[sent ? "Success" : "Warning"] = sent
                ? "Timetable and credentials emailed to " + teacher.Email
                : "Could not send email — check SMTP settings.";

            
            return RedirectToAction("TeacherView", new { teacherId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SendAllTimetables()
        {
            var teachers = db.Teachers
                .Include("TimetableSlots").Include("TimetableSlots.Subject").Include("TimetableSlots.Class")
                .Where(t => t.IsActive)
                .ToList()
                .Where(t => t.TimetableSlots.Any(s => s.IsActive))
                .ToList();

            int sent = 0;
            foreach (var t in teachers)
            {
                if (string.IsNullOrEmpty(t.TemporaryPassword))
                {
                    t.TemporaryPassword = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                }

                var slots = t.TimetableSlots.Where(s => s.IsActive).ToList();
                // Updated to pass 4 arguments
                if (emailService.SendTimetableEmail(t, slots, t.Email, t.TemporaryPassword)) sent++;
            }
            db.SaveChanges();

            TempData["Success"] = string.Format("Timetables sent to {0}/{1} teachers.", sent, teachers.Count);
            return RedirectToAction("Index");
        }

        public ActionResult GetTeacherSubjects(int teacherId)
        {
            var subjects = db.TeacherSubjects
                .Include("Subject")
                .Where(ts => ts.TeacherId == teacherId)
                .Select(ts => new { id = ts.SubjectId, name = ts.Subject.Name, code = ts.Subject.Code })
                .ToList();
            return Json(subjects, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}