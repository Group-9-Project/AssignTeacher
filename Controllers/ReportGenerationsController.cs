using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SchoolTimetable.Models;

namespace SchoolTimetable.Controllers
{
    public class ReportGenerationsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: ReportGenerations
        public ActionResult Index()
        {
            var reportGenerations = db.ReportGenerations.Include(r => r.Teacher);
            return View(reportGenerations.ToList());
        }

        // GET: ReportGenerations/Details/5
        public ActionResult Details(int? id)
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

        // GET: ReportGenerations/Create
        public ActionResult Create()
        {
            // Ensure SelectList uses the actual PK name ("Id") and the display property ("FullName")
            ViewBag.TeacherId = new SelectList(db.Teachers.ToList(), "Id", "FullName");
            return View();
        }

        // POST: ReportGenerations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ReportId,StudentNumber,StudentName,TeacherId,Subject,Grade,assignmentMark,test1Mark,test2Mark,examMark,Descriptor,Percentage,FinalPercentage,GradeAverage,Status")] ReportGeneration reportGeneration)
        {
            if (ModelState.IsValid)
            {
                // Ensure required StudentAccount relationship if applicable
                var account = db.StudentAccounts.FirstOrDefault(sa => sa.StudentNumber == reportGeneration.StudentNumber);
                if (account != null)
                {

                    reportGeneration.Descriptor = reportGeneration.Descriptors();
                    reportGeneration.Percentage = reportGeneration.percenntage();
                    reportGeneration.FinalPercentage = reportGeneration.Final();
                    reportGeneration.GradeAverage = reportGeneration.AVG();
                    reportGeneration.Status = reportGeneration.state();

                    reportGeneration.StudentAccount = account;
                }

                db.ReportGenerations.Add(reportGeneration);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Re-populate the select list when returning the view after validation errors
            ViewBag.TeacherId = new SelectList(db.Teachers.ToList(), "Id", "FullName", reportGeneration?.TeacherId);
            return View(reportGeneration);
        }

        // GET: ReportGenerations/Edit/5
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

        // POST: ReportGenerations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
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
