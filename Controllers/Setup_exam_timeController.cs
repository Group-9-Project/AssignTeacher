using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SchoolTimetable.Models;
using Setup_Examination_timetable.Models;

namespace AssignTeacher.Controllers
{
    public class Setup_exam_timeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Setup_exam_time
        public ActionResult Index()
        {
            return View(new List<Setup_exam_time>());
        }

        // GET: Setup_exam_time/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Setup_exam_time setup_exam_time = db.Setup_exam_time.Find(id);
            if (setup_exam_time == null)
            {
                return HttpNotFound();
            }
            return View(setup_exam_time);
        }

        // GET: Setup_exam_time/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Setup_exam_time/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Exam_name,venue,Exam_Starttime,Exam_Endtime,Exam_date,Grade")] Setup_exam_time setup_exam_time)
        {
            if (ModelState.IsValid)
            {
                db.Setup_exam_time.Add(setup_exam_time);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(setup_exam_time);
        }

        // GET: Setup_exam_time/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Setup_exam_time setup_exam_time = db.Setup_exam_time.Find(id);
            if (setup_exam_time == null)
            {
                return HttpNotFound();
            }
            return View(setup_exam_time);
        }

        // POST: Setup_exam_time/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Exam_name,venue,Exam_Starttime,Exam_Endtime,Exam_date,Grade")] Setup_exam_time setup_exam_time)
        {
            if (ModelState.IsValid)
            {
                db.Entry(setup_exam_time).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(setup_exam_time);
        }






        [HttpPost]
        public JsonResult SaveTimetable(List<Setup_exam_time> items)
        {
            try
            {
                if (items == null || items.Count == 0)
                {
                    return Json(new { success = false, error = "No timetable items received." });
                }

                var skipped = new List<object>();

                foreach (var it in items)
                {
                    // basic server-side validation: ensure required fields are present
                    if (string.IsNullOrWhiteSpace(it.Exam_name) || string.IsNullOrWhiteSpace(it.venue)
                        || string.IsNullOrWhiteSpace(it.Exam_Starttime) || string.IsNullOrWhiteSpace(it.Exam_Endtime))
                    {
                        skipped.Add(new
                        {
                            item = it,
                            reason = "Missing required field(s): Exam_name, venue, Exam_Starttime and Exam_Endtime are required."
                        });
                        continue;
                    }

                    DateTime dt = DateTime.MinValue;
                    DateTime.TryParse(Convert.ToString(it.Exam_date), out dt);

                    var entry = new Setup_exam_time
                    {
                        Exam_name = it.Exam_name,
                        venue = it.venue,
                        Exam_date = dt == DateTime.MinValue ? DateTime.Now.Date : dt,
                        Exam_Starttime = it.Exam_Starttime,
                        Exam_Endtime = it.Exam_Endtime,
                        Grade = it.Grade
                    };

                    db.Setup_exam_time.Add(entry);
                }

                try
                {
                    db.SaveChanges();
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    // collect validation messages to return to client for debugging
                    var valErrors = dbEx.EntityValidationErrors
                        .SelectMany(e => e.ValidationErrors)
                        .Select(e => e.PropertyName + ": " + e.ErrorMessage)
                        .ToList();

                    return Json(new { success = false, error = "Validation failed for one or more entities.", details = valErrors });
                }

                return Json(new { success = true, skipped = skipped });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        public ActionResult Published()
        {
            var list = db.Setup_exam_time.OrderBy(d => d.Exam_date).ThenBy(s => s.Exam_Starttime).ToList();
            return View(list);
        }

        // Read-only published view (no delete buttons)
        public ActionResult PublishedReadOnly()
        {
            var list = db.Setup_exam_time.OrderBy(d => d.Exam_date).ThenBy(s => s.Exam_Starttime).ToList();
            return View("PublishedReadOnly", list);
        }

        // POST: Home/DeleteSession
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteSession(string date, string start, string end)
        {
            if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
            {
                return new HttpStatusCodeResult(400);
            }

            DateTime parsedDate;
            if (!DateTime.TryParse(date, out parsedDate))
            {
                return new HttpStatusCodeResult(400);
            }

            var items = db.Setup_exam_time.Where(x => x.Exam_date == parsedDate && x.Exam_Starttime == start && x.Exam_Endtime == end).ToList();
            if (!items.Any())
            {
                return RedirectToAction("Published");
            }

            foreach (var it in items)
            {
                db.Setup_exam_time.Remove(it);
            }
            db.SaveChanges();

            return RedirectToAction("Published");
        }

        // GET: Setup_exam_time/Timetable
        public ActionResult Timetable()
        {
            // returns the Timetable view which is a client-side editor and doesn't require a model
            return View("Timetable");
        }


        // GET: Setup_exam_time/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Setup_exam_time setup_exam_time = db.Setup_exam_time.Find(id);
            if (setup_exam_time == null)
            {
                return HttpNotFound();
            }
            return View(setup_exam_time);
        }





        // POST: Setup_exam_time/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Setup_exam_time setup_exam_time = db.Setup_exam_time.Find(id);
            db.Setup_exam_time.Remove(setup_exam_time);
            db.SaveChanges();
            return RedirectToAction("Index");
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
