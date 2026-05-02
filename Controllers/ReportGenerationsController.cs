using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SchoolTimetable.Models;
using SchoolTimetable.ViewModels;

namespace SchoolTimetable.Controllers
{
    public class ReportGenerationsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // --- ADDED: Session Helper Methods (Like OnlineClassController) ---
        private int? GetCurrentUserId() => Session["UserId"] != null ? (int)Session["UserId"] : (int?)null;
        private bool IsTeacherUser() => Session["UserRole"]?.ToString() == "Teacher";
        private ActionResult RedirectToLogin() => RedirectToAction("Login", "Teacher");
        // -----------------------------------------------------------------

        // GET: ReportGenerations
        public ActionResult Index()
        {
            // --- ADDED: Filter Index by Teacher (Like OnlineClassController) ---
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToLogin();

            var reportGenerations = db.ReportGenerations
                .Include(r => r.Teacher)
                .Where(r => r.TeacherId == userId.Value); // Only show reports this teacher made

            return View(reportGenerations.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // 1. Try to find if this is a Report ID
            var report = db.ReportGenerations.Find(id);

            // 2. If not found, check if the ID passed was actually an Application ID (Child ID)
            if (report == null)
            {
                var app = db.Applications.Find(id);
                if (app != null)
                {
                    // Find the report linked to this child's student number/email
                    report = db.ReportGenerations.FirstOrDefault(r => r.StudentNumber == app.Email);
                }
            }

            if (report == null)
            {
                return HttpNotFound();
            }

            return View(report);
        }

        public ActionResult Create()
        {

            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToLogin();

            ViewBag.StudentList = LoadStudents();

          
            ViewBag.TeacherId = new SelectList(db.Teachers.Where(t => t.Id == userId.Value).ToList(), "Id", "FullName", userId.Value);

          
            ViewBag.StudentNumber = new SelectList(db.StudentAccounts.ToList(), "StudentNumber", "StudentNumber");

            return View();
        }

        private List<AllocationViewModel> LoadStudents()
        {
            var students = db.Students
                .Select(s => new AllocationViewModel
                {
                    StudentNumber = s.StudentNumber,
                    FullName = s.FullName,
                    ApplicationId = s.ApplicationId
                })
                .ToList();

            foreach (var s in students)
            {
                var app = db.Applications.FirstOrDefault(a => a.Id == s.ApplicationId);

                if (app?.AssignedClassId != null)
                {
                    var cls = db.Classes.FirstOrDefault(c => c.Id == app.AssignedClassId);
                    if (cls != null)
                    {
                        s.Grade = cls.Grade;
                    }
                }
            }

            return students;
        }


        [HttpGet]
        public JsonResult GetStudentDetails(string studentNumber)
        {
            var student = db.StudentAccounts.FirstOrDefault(s => s.StudentNumber == studentNumber);
            if (student == null) return Json(null, JsonRequestBehavior.AllowGet);

            var app = db.Applications.FirstOrDefault(a => (a.FirstName + " " + a.LastName) == student.FullName);

            string className = "Not Assigned";
            if (app?.AssignedClassId != null)
            {
                var cls = db.Classes.FirstOrDefault(c => c.Id == app.AssignedClassId);
                if (cls != null) className = cls.Name;
            }

            return Json(new
            {
                FullName = student.FullName,
                ClassName = className
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ReportId,StudentNumber,StudentName,TeacherId,Subject,Grade,assignmentMark,test1Mark,test2Mark,examMark,Descriptor,Percentage,FinalPercentage,GradeAverage,Status")] ReportGeneration reportGeneration)
        {
            // --- ADDED: Security check ---
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToLogin();

            if (ModelState.IsValid)
            {
                var account = db.StudentAccounts.FirstOrDefault(sa => sa.StudentNumber == reportGeneration.StudentNumber);
                if (account != null)
                {
                    reportGeneration.Descriptor = reportGeneration.Descriptors();
                    reportGeneration.Percentage = reportGeneration.percenntage();
                    reportGeneration.FinalPercentage = reportGeneration.Final();
                    reportGeneration.GradeAverage = reportGeneration.AVG();
                    reportGeneration.Status = reportGeneration.state();

                    reportGeneration.StudentAccount = account;

                    // --- ADDED: Force TeacherId to be the logged-in user (Secure Pattern) ---
                    reportGeneration.TeacherId = userId.Value;
                }

                db.ReportGenerations.Add(reportGeneration);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.TeacherId = new SelectList(db.Teachers.ToList(), "Id", "FullName", reportGeneration.TeacherId);
            ViewBag.StudentNumber = new SelectList(db.StudentAccounts.ToList(), "StudentNumber", "StudentNumber", reportGeneration.StudentNumber);
            return View(reportGeneration);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReportGeneration reportGeneration = db.ReportGenerations.Find(id);
            if (reportGeneration == null)
            {
                return HttpNotFound();
            }
            ViewBag.TeacherId = new SelectList(db.Teachers, "Id", "FirstName", reportGeneration.TeacherId);
            return View(reportGeneration);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ReportId,StudentNumber,StudentName,TeacherId,Subject,Grade,assignmentMark,test1Mark,test2Mark,examMark,Descriptor,Percentage,FinalPercentage,GradeAverage,Status")] ReportGeneration reportGeneration)
        {
            if (ModelState.IsValid)
            {
                db.Entry(reportGeneration).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.TeacherId = new SelectList(db.Teachers, "Id", "FirstName", reportGeneration.TeacherId);
            return View(reportGeneration);
        }

        // GET: ReportGenerations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ReportGeneration reportGeneration = db.ReportGenerations.Find(id);
            if (reportGeneration == null)
            {
                return HttpNotFound();
            }
            return View(reportGeneration);
        }

        // POST: ReportGenerations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ReportGeneration reportGeneration = db.ReportGenerations.Find(id);
            db.ReportGenerations.Remove(reportGeneration);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: ReportGenerations/StudentView/5
        public ActionResult StudentView(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var report = db.ReportGenerations
                           .Include(r => r.Teacher)
                           .FirstOrDefault(r => r.ReportId == id.Value);

            if (report == null)
                return HttpNotFound();

            return View(report);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}