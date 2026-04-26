using SchoolTimetable.Models;
using SchoolTimetable.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SchoolTimetable.Controllers
{
    public class TeachersController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: Teachers (Admin Only)
        public ActionResult Index()
        {
            if (Session["UserRole"]?.ToString() != "Admin") return RedirectToAction("Login");

            var teachers = db.Teachers
                .Include("TeacherSubjects.Subject")
                .Include("TimetableSlots")
                .OrderBy(t => t.LastName)
                .ToList();
            return View(teachers);
        }
        [AllowAnonymous]
        public ActionResult Login() => View();

        [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Please enter both identifier and password.";
                return View();
            }

            var inputId = email.Trim();
            var inputPass = password.Trim();

            // 1. HARDCODED ADMIN CHECK
            if (inputId == "admin@dgss.edu.za" && inputPass == "Admin@2026")
            {
                Session["UserId"] = 0;
                Session["UserName"] = "System Admin";
                Session["UserRole"] = "Admin";
                return RedirectToAction("Dashboard", "Admin");
            }

            // 2. HARDCODED LIBRARIAN CHECK
            if (inputId.ToLower() == "librarian@dgss.com" && inputPass == "librarian@123")
            {
                Session["UserId"] = -1;
                Session["UserName"] = "School Librarian";
                Session["UserRole"] = "Librarian";
                return RedirectToAction("LibrarianDashboard", "Librarian");
            }
            // Change this line (Line 62)
            var studentAccount = db.StudentAccounts.FirstOrDefault(s => s.StudentNumber == inputId);


            if (studentAccount != null && studentAccount.IsActive)
            {
                bool passwordMatch =
                    studentAccount.Password == inputPass ||
                    studentAccount.TemporaryPassword == inputPass;

                if (!passwordMatch)
                {
                    ViewBag.Error = "Invalid student number or temporary password.";
                    return View();
                }

                var appUser = db.AppUsers.Find(studentAccount.Id);
                if (appUser == null)
                {
                    ViewBag.Error = "Student account not linked.";
                    return View();
                }

                Session["UserId"] = appUser.Id;
                Session["UserName"] = studentAccount.FullName;
                Session["UserRole"] = "Student";
                Session["StudentNumber"] = studentAccount.StudentNumber;

                return RedirectToAction("Index", "StudentAccount");
            }
            // 4. TEACHER / STAFF CHECK
            var teacher = db.Teachers.FirstOrDefault(t => t.Email == inputId &&
                (t.Password == inputPass || t.TemporaryPassword == inputPass));

            if (teacher != null && teacher.IsActive)
            {
                Session["UserId"] = teacher.Id;
                Session["UserName"] = teacher.FullName;
                Session["UserRole"] = teacher.IsAdmin ? "Admin" : "Teacher";

                return teacher.IsAdmin
                    ? RedirectToAction("Dashboard", "Admin")
                    : RedirectToAction("Dashboard", "Teachers");
            }

            // 5. PARENT CHECK
            var parent = db.Parents.FirstOrDefault(p => p.Email == inputId && p.Password == inputPass);
            if (parent != null)
            {
                Session["UserId"] = parent.Id;
                Session["UserName"] = parent.FullName;
                Session["UserRole"] = "Parent";
                return RedirectToAction("Index", "Parents");
            }

            // 6. FAILURE
            ViewBag.Error = "Invalid Login Details. Please check your credentials.";
            return View();
        }

        // Changed to GET for easier access from Logout links
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }

        // --- PROFILE & DASHBOARD ---

        public ActionResult Dashboard()
        {
            if (Session["UserRole"]?.ToString() != "Teacher") return RedirectToAction("Login");

            int teacherId = (int)(Session["UserId"] ?? 0);
            var timetable = db.TimetableSlots
                .Include("Subject").Include("Class")
                .Where(s => s.TeacherId == teacherId && s.IsActive)
                .OrderBy(s => s.DayOfWeek).ThenBy(s => s.Period)
                .ToList();

            return View(timetable);
        }

        public ActionResult Details(int? id)
        {
            int targetId = id ?? (int)(Session["UserId"] ?? 0);
            if (targetId == 0) return RedirectToAction("Login");

            var teacher = db.Teachers
                .Include("TeacherSubjects.Subject")
                .Include("TimetableSlots.Subject")
                .Include("TimetableSlots.Class")
                .FirstOrDefault(t => t.Id == targetId);

            if (teacher == null) return HttpNotFound();
            return View(teacher);
        }

        public ActionResult Edit(int id)
        {
            var userRole = Session["UserRole"]?.ToString();
            if (userRole == "Teacher" && (int)(Session["UserId"] ?? 0) != id)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);

            if (userRole == null) return RedirectToAction("Login");

            var teacher = db.Teachers.Find(id);
            if (teacher == null) return HttpNotFound();
            return View(teacher);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Edit(Teacher teacher)
        {
            if (!ModelState.IsValid) return View(teacher);

            var current = db.Teachers.AsNoTracking().FirstOrDefault(t => t.Id == teacher.Id);
            if (current == null) return HttpNotFound();

            if (string.IsNullOrWhiteSpace(teacher.Password))
                teacher.Password = current.Password;

            teacher.CreatedAt = current.CreatedAt;
            teacher.IsAdmin = current.IsAdmin;

            db.Entry(teacher).State = EntityState.Modified;
            db.SaveChanges();

            return Session["UserRole"]?.ToString() == "Admin"
                ? RedirectToAction("Index")
                : RedirectToAction("Details", new { id = teacher.Id });
        }

        // --- ANNOUNCEMENTS ---

        [HttpGet]
        public ActionResult CreateAnnouncement()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login");

            int teacherId = (int)Session["UserId"];

            var myClasses = db.TimetableSlots
                .Where(s => s.TeacherId == teacherId && s.IsActive)
                .Select(s => s.Class)
                .Distinct()
                .ToList();

            // FIX: Using "Name" as the display property to avoid binding errors
            ViewBag.ClassId = new SelectList(myClasses, "Id", "Name");

            var announcements = db.Announcements
                .Where(a => a.TeacherId == teacherId)
                .OrderByDescending(a => a.DatePosted)
                .ToList();

            return View(announcements);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateAnnouncement(Announcement announcement)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login");

            int teacherId = (int)Session["UserId"];

            if (ModelState.IsValid)
            {
                announcement.TeacherId = teacherId;
                announcement.DatePosted = DateTime.Now;
                db.Announcements.Add(announcement);
                db.SaveChanges();
                return RedirectToAction("CreateAnnouncement");
            }

            // REFILL ViewBag if validation fails to prevent DropDownList crash
            var myClasses = db.TimetableSlots
                .Where(s => s.TeacherId == teacherId && s.IsActive)
                .Select(s => s.Class)
                .Distinct()
                .ToList();

            ViewBag.ClassId = new SelectList(myClasses, "Id", "Name");

            var announcements = db.Announcements
                .Where(a => a.TeacherId == teacherId)
                .OrderByDescending(a => a.DatePosted)
                .ToList();

            return View(announcements);
        }

        // --- LEARNING MATERIALS ---

        [HttpGet]
        public ActionResult UploadMaterial()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login");

            int teacherId = (int)Session["UserId"];

            var myClasses = db.TimetableSlots
                .Where(s => s.TeacherId == teacherId && s.IsActive)
                .Select(s => s.Class)
                .Distinct()
                .ToList();

            // Ensure this matches the ViewBag.ClassId used in the View
            ViewBag.ClassId = new SelectList(myClasses, "Id", "Name");

            var materials = db.LearningMaterials
                .Include("Class")
                .Where(m => m.TeacherId == teacherId)
                .ToList();

            return View(materials);
        }

        [HttpPost]
        public ActionResult UploadMaterial(HttpPostedFileBase file, string title, string description, int SchoolClassId)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login");
            int teacherId = (int)Session["UserId"];

            if (file != null && file.ContentLength > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                var folderPath = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var path = Path.Combine(folderPath, fileName);
                file.SaveAs(path);

                db.LearningMaterials.Add(new LearningMaterial
                {
                    Title = title,
                    Description = description,
                    FileName = fileName,
                    FilePath = "/Uploads/" + fileName,
                    FileType = file.ContentType,
                    UploadDate = DateTime.Now,
                    TeacherId = teacherId,
                    SchoolClassId = SchoolClassId
                });
                db.SaveChanges();
                return RedirectToAction("UploadMaterial");
            }

            // If file upload failed, we must still return the view with the list
            return RedirectToAction("UploadMaterial");
        }

        // --- ADMIN MANAGEMENT ---

        public ActionResult Create() => View(new Teacher());

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(Teacher teacher)
        {
            if (!ModelState.IsValid) return View(teacher);
            teacher.CreatedAt = DateTime.Now;
            teacher.IsActive = true;
            db.Teachers.Add(teacher);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult AssignSubjects(int id)
        {
            var teacher = db.Teachers.Find(id);
            if (teacher == null) return HttpNotFound();

            return View(new TeacherAssignmentViewModel
            {
                Teacher = teacher,
                AllSubjects = db.Subjects.Where(s => s.IsActive).ToList(),
                AssignedSubjectIds = db.TeacherSubjects.Where(ts => ts.TeacherId == id).Select(ts => ts.SubjectId).ToList()
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AssignSubjects(int TeacherId, int[] subjectIds)
        {
            var current = db.TeacherSubjects.Where(ts => ts.TeacherId == TeacherId).ToList();
            db.TeacherSubjects.RemoveRange(current);

            if (subjectIds != null)
            {
                foreach (var sId in subjectIds)
                    db.TeacherSubjects.Add(new TeacherSubject { TeacherId = TeacherId, SubjectId = sId });
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}