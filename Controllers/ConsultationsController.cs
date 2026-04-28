using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SchoolTimetable.Models;
using System.Data.Entity;

namespace SchoolTimetable.Controllers
{
    public class ConsultationController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private int? GetCurrentUserId() => Session["UserId"] != null ? (int)Session["UserId"] : (int?)null;
        private bool IsTeacherUser() => Session["UserRole"]?.ToString() == "Teacher";
        private ActionResult RedirectToLogin() => RedirectToAction("Login", "Teacher");
        public ActionResult RequestConsultation()
        {
            var user = GetCurrentUserId();
            if (user == null) return RedirectToLogin();

            ViewBag.TeacherSubjectId = new SelectList(db.TeacherSubjects
                .Select(ts => new {
                    Id = ts.Id,
                    Display = ts.Teacher.FirstName + " " + ts.Teacher.LastName + " (" + ts.Subject.Name + ")"
                }), "Id", "Display");

            ViewBag.ClassId = new SelectList(db.Classes.Where(c => c.IsActive), "Id", "Name");

            return View();
        }

   
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RequestConsultation(Consultation consultation, int TeacherSubjectId, int ClassId)
        {
            if (ModelState.IsValid)
            {
                consultation.status = conStatus.Pending;
                consultation.TeacherSubject = db.TeacherSubjects.Find(TeacherSubjectId);
                consultation.Class = db.Classes.Find(ClassId);
                consultation.ParentId = GetCurrentUserId();
                db.consultations.Add(consultation);
                db.SaveChanges();

                TempData["Success"] = "Consultation requested successfully. Waiting for teacher approval.";
                return RedirectToAction("MyRequests");
            }

            return View(consultation);
        }

        public ActionResult MyRequests()
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToLogin();
            var list = db.consultations
          .Include(c => c.TeacherSubject.Teacher) 
          .Include(c => c.TeacherSubject.Subject) 
          .Include(c => c.Class)                  
          .Where(c => c.ParentId == userId)
          .OrderByDescending(c => c.date)
          .ToList();
            return View(list);
        }

        public ActionResult Manage()
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToLogin();
            var list = db.consultations
            .Include(c => c.TeacherSubject.Subject)
            .Include(c => c.Class)
            .Where(c => c.TeacherSubject.TeacherId == userId)
            .OrderByDescending(c => c.date)
            .ToList();

            return View(list);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Respond(int id, string decision, DateTime? confirmedDate, DateTime? confirmedTime)
        {
            var consultation = db.consultations.Find(id);
            if (consultation == null) return HttpNotFound();

            if (decision == "Reject")
            {
                if (!confirmedDate.HasValue || !confirmedTime.HasValue)
                {
                    TempData["Error"] = "Please provide a date and time for the accepted consultation.";
                    return RedirectToAction("Manage");
                }

                consultation.status = conStatus.Rejected;
                consultation.date = confirmedDate.Value;
                consultation.time = confirmedTime.Value;
            }
            else
            {
                consultation.status = conStatus.Accepted;
            }

            db.Entry(consultation).State = EntityState.Modified;
            db.SaveChanges();

            TempData["Success"] = $"Consultation has been {consultation.status}.";
            return RedirectToAction("Manage");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}