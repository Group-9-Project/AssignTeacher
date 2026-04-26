using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using SchoolTimetable.Models;
using SchoolTimetable.Services;
using SchoolTimetable.ViewModels;

namespace SchoolTimetable.Controllers
{
    public class SubstitutionsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private readonly EmailService emailService = new EmailService();

        public ActionResult Index()
        {
            var subs = db.Substitutions
                .Include("OriginalTeacher").Include("SubstituteTeacher")
                .Include("TimetableSlot").Include("TimetableSlot.Subject")
                .Include("TimetableSlot.Class")
                .OrderByDescending(s => s.SubstitutionDate)
                .ToList();
            return View(subs);
        }

        public ActionResult Create(int? slotId = null)
        {
            var vm = new SubstitutionFormViewModel
            {
                TimetableSlotId = slotId ?? 0,
                SubstitutionDate = DateTime.Today
            };

            if (slotId.HasValue)
            {
                vm.Slot = db.TimetableSlots
                    .Include("Teacher").Include("Subject").Include("Class")
                    .FirstOrDefault(s => s.Id == slotId.Value);
            }

       
            int? originalTeacherId = (vm.Slot != null) ? vm.Slot.TeacherId : (int?)null;

            vm.AllSlots = db.TimetableSlots
                .Include("Teacher").Include("Subject").Include("Class")
                .Where(s => s.IsActive)
                .OrderBy(s => s.Teacher.LastName)
                .ToList();

            // Filter out the original teacher from the dropdown
            vm.AvailableTeachers = db.Teachers
                .Where(t => t.IsActive && (originalTeacherId == null || t.Id != originalTeacherId))
                .OrderBy(t => t.LastName)
                .ToList();

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(SubstitutionFormViewModel vm)
        {
            var slot = db.TimetableSlots
                .Include("Teacher").Include("Subject").Include("Class")
                .FirstOrDefault(s => s.Id == vm.TimetableSlotId);

            // 1. Check if the slot exists
            if (slot == null)
            {
                ModelState.AddModelError("TimetableSlotId", "Please select a valid timetable slot.");
            }
            // 2. EXPLICIT ERROR: Check if Substitute is the same as Original
            else if (vm.SubstituteTeacherId == slot.TeacherId)
            {
                ModelState.AddModelError("SubstituteTeacherId", "Error: A teacher cannot substitute for themselves. Please select a different teacher.");
            }
          

            if (!ModelState.IsValid)
            {
                vm.Slot = slot;
                int? originalTeacherId = (slot != null) ? slot.TeacherId : (int?)null;

                vm.AllSlots = db.TimetableSlots.Include("Teacher").Include("Subject").Include("Class")
                    .Where(s => s.IsActive).ToList();

                // Re-filter the list so the original teacher remains excluded
                vm.AvailableTeachers = db.Teachers
                    .Where(t => t.IsActive && (originalTeacherId == null || t.Id != originalTeacherId))
                    .OrderBy(t => t.LastName).ToList();

                return View(vm);
            }
            if (vm.SubstitutionDate < new DateTime(1753, 1, 1))
            {
                vm.SubstitutionDate = DateTime.Today;
            }

            var sub = new Substitution
            {
                TimetableSlotId = vm.TimetableSlotId,
                OriginalTeacherId = slot.TeacherId,
                SubstituteTeacherId = vm.SubstituteTeacherId,
                SubstitutionDate = vm.SubstitutionDate,
                Reason = vm.Reason,
                Status = SubstitutionStatus.Confirmed,
                CreatedAt = DateTime.Now
            };

            db.Substitutions.Add(sub);
            db.SaveChanges();

            // Refresh sub data for email service
            sub = db.Substitutions
                .Include("OriginalTeacher").Include("SubstituteTeacher")
                .Include("TimetableSlot").Include("TimetableSlot.Subject")
                .Include("TimetableSlot.Class")
                .FirstOrDefault(s => s.Id == sub.Id);

            if (sub != null)
            {
                emailService.SendSubstitutionEmail(sub.OriginalTeacher, sub, false);
                emailService.SendSubstitutionEmail(sub.SubstituteTeacher, sub, true);
            }

            TempData["Success"] = "Substitution created successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Confirm(int id)
        {
            var sub = db.Substitutions.Find(id);
            if (sub != null)
            {
                sub.Status = SubstitutionStatus.Confirmed;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            var sub = db.Substitutions.Find(id);
            if (sub != null)
            {
                sub.Status = SubstitutionStatus.Cancelled;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult GetSlotDetails(int slotId)
        {
            var slot = db.TimetableSlots
                .Include("Teacher").Include("Subject").Include("Class")
                .FirstOrDefault(s => s.Id == slotId);

            if (slot == null) return HttpNotFound();

            return Json(new
            {
                teacherName = slot.Teacher != null ? slot.Teacher.FullName : "",
                subjectName = slot.Subject != null ? slot.Subject.Name : "",
                className = slot.Class != null ? slot.Class.Name : "",
                day = slot.DayName,
                period = slot.Period,
                originalTeacherId = slot.TeacherId
            }, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}