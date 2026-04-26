using System.Linq;
using System.Web.Mvc;
using SchoolTimetable.Models;
using SchoolTimetable.ViewModels;

namespace SchoolTimetable.Controllers
{
    public class SubjectsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var subjects = db.Subjects
                .Include("TeacherSubjects").Include("TeacherSubjects.Teacher")
                .OrderBy(s => s.Name).ToList();
            return View(subjects);
        }

        public ActionResult Create() { return View(new Subject { Color = "#4A90D9" }); }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Subject subject)
        {
            if (!ModelState.IsValid) return View(subject);
            db.Subjects.Add(subject);
            db.SaveChanges();
            TempData["Success"] = "Subject '" + subject.Name + "' added.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var s = db.Subjects.Find(id);
            if (s == null) return HttpNotFound();
            return View(s);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Subject subject)
        {
            if (!ModelState.IsValid) return View(subject);
            db.Entry(subject).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            TempData["Success"] = "Subject updated.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var s = db.Subjects.Find(id);
            if (s != null) { s.IsActive = false; db.SaveChanges(); }
            TempData["Success"] = "Subject deactivated.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }

    public class ClassesController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            var classes = db.Classes.Include("TimetableSlots").OrderBy(c => c.Name).ToList();
            return View(classes);
        }

        public ActionResult Create() { return View(new SchoolClass()); }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(SchoolClass schoolClass)
        {
            if (!ModelState.IsValid) return View(schoolClass);
            db.Classes.Add(schoolClass);
            db.SaveChanges();
            TempData["Success"] = "Class '" + schoolClass.Name + "' added.";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            var c = db.Classes.Find(id);
            if (c == null) return HttpNotFound();
            return View(c);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(SchoolClass schoolClass)
        {
            if (!ModelState.IsValid) return View(schoolClass);
            db.Entry(schoolClass).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            TempData["Success"] = "Class updated.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var c = db.Classes.Find(id);
            if (c != null) { c.IsActive = false; db.SaveChanges(); }
            TempData["Success"] = "Class deactivated.";
            return RedirectToAction("Index");
        }

        public ActionResult Timetable(int id)
        {
            var cls = db.Classes.Find(id);
            if (cls == null) return HttpNotFound();

            var slots = db.TimetableSlots
                .Include("Teacher").Include("Subject").Include("Class")
                .Where(s => s.ClassId == id && s.IsActive)
                .ToList();

            var vm = new TimetableViewModel { Class = cls, Slots = slots };
            return View(vm);
        }

        protected override void Dispose(bool disposing) { if (disposing) db.Dispose(); base.Dispose(disposing); }
    }
}