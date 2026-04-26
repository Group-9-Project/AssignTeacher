using SchoolTimetable.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace SchoolTimetable.Controllers
{
    public class ApplicationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Create()
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Teachers"); // Restored original

            var model = new Application
            {
                ParentId = (int)Session["UserId"],
                Marks = new List<ApplicantMark>(),
                SubmissionDate = DateTime.Now,
                Status = ApplicationStatus.Pending,
                IsPaid = false,
                IsProcessed = false
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Application app, string[] subjectInputs, int[] markInputs)
        {
            if (Session["UserId"] == null)
                return RedirectToAction("Login", "Teachers"); // Restored original

            // 1. MAP DATA MANUALLY
            app.Marks = new List<ApplicantMark>();
            if (subjectInputs != null && markInputs != null)
            {
                for (int i = 0; i < Math.Min(subjectInputs.Length, markInputs.Length); i++)
                {
                    if (!string.IsNullOrWhiteSpace(subjectInputs[i]) && markInputs[i] > 0)
                    {
                        app.Marks.Add(new ApplicantMark
                        {
                            SubjectName = subjectInputs[i].Trim(),
                            Percentage = markInputs[i]
                        });
                    }
                }
            }

            // 2. SYSTEM DEFAULTS
            app.ParentId = (int)Session["UserId"];
            app.SubmissionDate = DateTime.Now;
            app.IsProcessed = false;
            app.IsPaid = false;
            app.Status = ApplicationStatus.Pending;

            if (app.Marks.Any())
            {
                app.AverageMark = Math.Round(app.Marks.Average(m => (double)m.Percentage), 2);
            }

            // 3. CLEANUP VALIDATION
            ModelState.Remove("ParentId");
            ModelState.Remove("SubmissionDate");
            ModelState.Remove("IsProcessed");
            ModelState.Remove("IsPaid");
            ModelState.Remove("Status");
            ModelState.Remove("AverageMark");
            ModelState.Remove("Marks");

            if (ModelState.IsValid)
            {
                try
                {
                    db.Applications.Add(app);
                    db.SaveChanges();
                    TempData["Success"] = "Application submitted successfully!";
                    return RedirectToAction("Index", "Parents"); // Restored original
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Database Error: " + ex.Message);
                }
            }

            ViewBag.StartStep = 2; // Return to marks step if error occurs
            return View(app);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { db.Dispose(); }
            base.Dispose(disposing);
        }
    }
}