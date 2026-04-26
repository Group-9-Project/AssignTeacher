using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SchoolTimetable.Models;
using SchoolTimetable.Services;
using SchoolTimetable.ViewModels;
using Newtonsoft.Json;

namespace SchoolTimetable.Controllers
{
    public class OnlineClassController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

  
        private int? GetCurrentUserId() => Session["UserId"] != null ? (int)Session["UserId"] : (int?)null;
        private bool IsTeacherUser() => Session["UserRole"]?.ToString() == "Teacher";
        private ActionResult RedirectToLogin() => RedirectToAction("Login", "Teacher");

     
        public ActionResult Index()
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToLogin();

            var now = DateTime.Now;
            List<OnlineClass> classes;

            if (IsTeacherUser())
            {
                classes = db.OnlineClasses
                    .Include(c => c.Teacher)
                    .Include(c => c.Attendances)
                    .Where(c => c.TeacherId == userId.Value)
                    .OrderByDescending(c => c.ScheduledAt)
                    .ToList();
            }
            else
            {
                classes = db.OnlineClasses
                    .Include(c => c.Teacher)
                    .Include(c => c.Attendances)
                    .Where(c => !c.IsCompleted)
                    .OrderBy(c => c.ScheduledAt)
                    .ToList();
            }

            var vm = new OnlineClassDashboardViewModel
            {
                IsTeacher = IsTeacherUser(),
                CurrentUser = db.AppUsers.Find(userId.Value),
                LiveClasses = classes.Where(c => c.IsActive && !c.IsCompleted).ToList(),
                UpcomingClasses = classes.Where(c => !c.IsActive && !c.IsCompleted && c.ScheduledAt > now).ToList(),
                CompletedClasses = IsTeacherUser()
                    ? classes.Where(c => c.IsCompleted).OrderByDescending(c => c.ScheduledAt).Take(10).ToList()
                    : new List<OnlineClass>(),
                TotalAttendees = IsTeacherUser()
                    ? classes.SelectMany(c => c.Attendances).Select(a => a.StudentId).Distinct().Count()
                    : 0
            };

            return View(vm);
        }

        public ActionResult TeacherDashboardJson()
        {
            var user = GetCurrentUserId();
            if (user == null) return new HttpUnauthorizedResult();

            var classes = db.OnlineClasses
                .Include(c => c.Attendances)
                .Where(c => c.TeacherId == user.Value)
                .OrderByDescending(c => c.ScheduledAt)
                .ToList();

            var result = new
            {
                liveCount = classes.Count(c => c.IsActive && !c.IsCompleted),
                upcomingCount = classes.Count(c => !c.IsActive && !c.IsCompleted && c.ScheduledAt > DateTime.Now),
                completedCount = classes.Count(c => c.IsCompleted),
                totalStudents = classes.SelectMany(c => c.Attendances).Select(a => a.StudentId).Distinct().Count(),
                classes = classes.Select(c => new
                {
                    id = c.Id,
                    title = c.Title,
                    subject = c.Subject,
                    scheduledAt = c.ScheduledAt.ToString("ddd, MMM d · h:mm tt"),
                    duration = c.DurationMinutes,
                    attendeeCount = c.Attendances.Count,
                    isLive = c.IsActive && !c.IsCompleted,
                    isCompleted = c.IsCompleted,
                    detailsUrl = Url.Action("Details", "OnlineClass", new { id = c.Id }),
                    startUrl = Url.Action("StartClass", "OnlineClass", new { id = c.Id }),
                    endUrl = Url.Action("EndClass", "OnlineClass", new { id = c.Id }),
                    deleteUrl = Url.Action("Delete", "OnlineClass", new { id = c.Id }),
                    meetingUrl = Url.Action("Meeting", "OnlineClass", new { id = c.Id }),
                    attendanceUrl = Url.Action("Attendance", "OnlineClass", new { id = c.Id })
                }).ToList()
            };

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Browse(string search)
        {
            var user = GetCurrentUserId();
            if (user == null) return RedirectToLogin();

            var query = db.OnlineClasses
                .Include(c => c.Teacher)
                .Include(c => c.Attendances)
                .Where(c => !c.IsCompleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.Title.Contains(search) || c.Subject.Contains(search));

            var classes = query.OrderBy(c => c.ScheduledAt).ToList();
            ViewBag.Search = search;
            return View(classes);
        }

        // ── CLASS LIFECYCLE ──────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StartClass(int id)
        {
            var user = GetCurrentUserId();
            if (user == null) return RedirectToLogin();

            var cls = db.OnlineClasses.Find(id);
            if (cls == null || cls.TeacherId != user.Value)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);

            cls.IsActive = true;
            cls.IsCompleted = false;
            db.SaveChanges();

            return RedirectToAction("Meeting", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EndClass(int id)
        {
            var user = GetCurrentUserId();

           
            if (user == null)
            {
                return RedirectToLogin();
            }

            var cls = db.OnlineClasses.Find(id);

            if (cls == null || cls.TeacherId != user.Value)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden, "You do not have permission to end this class.");
            }

            cls.IsActive = false;
            cls.IsCompleted = true;

            var open = db.ClassAttendances.Where(a => a.ClassId == id && a.LeftAt == null).ToList();
            foreach (var att in open) att.LeftAt = DateTime.Now;

            db.SaveChanges();

            TempData["Success"] = "Class ended successfully.";
            return RedirectToAction("Attendance", new { id });
        }
     
        public ActionResult Meeting(int id)
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToLogin();

            var cls = db.OnlineClasses.Include(c => c.Teacher).FirstOrDefault(c => c.Id == id);
            if (cls == null) return HttpNotFound();

            bool isHost = IsTeacherUser() && cls.TeacherId == userId.Value;

            // Student Access Check
            if (!isHost && Session["UserRole"]?.ToString() == "Student")
            {
                bool hasAttendance = db.ClassAttendances.Any(a => a.ClassId == id && a.StudentId == userId.Value);
                if (!hasAttendance) return RedirectToAction("Index");
            }

            var teacherToolbar = new[] { "microphone", "camera", "desktop", "fullscreen", "fodeviceselection", "hangup", "profile", "chat", "settings", "raisehand", "videoquality", "tileview", "mute-everyone", "security" };
            var studentToolbar = new[] { "microphone", "camera", "fullscreen", "fodeviceselection", "hangup", "profile", "chat", "raisehand", "videoquality", "tileview" };

            var config = new
            {
                domain = "meet.jit.si",
                roomName = cls.MeetingRoomId,
                displayName = Session["UserName"]?.ToString() ?? "User",
                configOverwrite = new
                {
                    startWithAudioMuted = !isHost,
                    startWithVideoMuted = false,
                    enableWelcomePage = false,
                    prejoinPageEnabled = false,
                    disableRemoteMute = !isHost
                },
                interfaceConfigOverwrite = new
                {
                    TOOLBAR_BUTTONS = isHost ? teacherToolbar : studentToolbar,
                    SHOW_JITSI_WATERMARK = false
                }
            };

            ViewBag.JitsiConfig = JsonConvert.SerializeObject(config);
            ViewBag.IsTeacher = isHost;
            return View(cls);
        }
        public ActionResult Details(int id)
        {
            var user = GetCurrentUserId();
            if (user == null) return RedirectToLogin();

            var cls = db.OnlineClasses.Include(c => c.Teacher).Include("Attendances.Student").FirstOrDefault(c => c.Id == id);
            if (cls == null) return HttpNotFound();

            var isTeacher = IsTeacherUser() && cls.TeacherId == user.Value;
            var hasJoined = cls.Attendances.Any(a => a.StudentId == user.Value);

            return View(new ClassDetailsViewModel
            {
                Class = cls,
                Attendances = cls.Attendances.OrderBy(a => a.JoinedAt).ToList(),
                IsTeacher = isTeacher,
                HasJoined = hasJoined,
                CurrentUser = user.Value
            });
        }

        public ActionResult Attendance(int id)
        {
            var user = GetCurrentUserId();
            if (user == null) return RedirectToLogin();
            var cls = db.OnlineClasses.Include(c => c.Teacher).Include("Attendances.Student").FirstOrDefault(c => c.Id == id);
            if (cls == null || cls.TeacherId != user.Value) return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);
            return View(cls);
        }

        public ActionResult AttendanceJson(int id)
        {
            var user = GetCurrentUserId();
            if (user == null) return new HttpUnauthorizedResult();

          
            var rawAttendances = db.ClassAttendances
                .Include(a => a.Student)
                .Where(a => a.ClassId == id)
                .OrderBy(a => a.JoinedAt)
                .ToList(); 
            var attendances = rawAttendances.Select(a => new AttendanceJsonItem
            {
                Name = a.Student.FullName,
                Email = a.Student.Email,
                JoinedAt = a.JoinedAt.ToString("h:mm tt"), 
                LeftAt = a.LeftAt != null ? a.LeftAt.Value.ToString("h:mm tt") : null,
                IsActive = a.LeftAt == null
            }).ToList();

            return Json(attendances, JsonRequestBehavior.AllowGet);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Join(int id, string password)
        {
            int? userId = GetCurrentUserId();
            if (userId == null) return RedirectToLogin();

            var cls = db.OnlineClasses.Find(id);
            if (cls == null) return HttpNotFound();

            if (!string.IsNullOrEmpty(cls.JoinPassword) && cls.JoinPassword != password)
            {
                TempData["Error"] = "Incorrect class password.";
                return RedirectToAction("Index");
            }

            if (Session["UserRole"]?.ToString() == "Student")
            {
                var existing = db.ClassAttendances.FirstOrDefault(a => a.ClassId == id && a.StudentId == userId.Value);
                if (existing == null)
                {
                    db.ClassAttendances.Add(new ClassAttendance { ClassId = id, StudentId = userId.Value, JoinedAt = DateTime.Now, IsPresent = true });
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Meeting", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Leave(int id)
        {
            var user = GetCurrentUserId();
            if (user == null) return RedirectToLogin();

            var att = db.ClassAttendances.FirstOrDefault(a => a.ClassId == id && a.StudentId == user.Value);
            if (att != null && att.LeftAt == null)
            {
                att.LeftAt = DateTime.Now;
                db.SaveChanges();
            }
            TempData["Success"] = "You have left the class.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var user = GetCurrentUserId();
            if (user == null) return RedirectToLogin();

            var cls = db.OnlineClasses.Find(id);
            if (cls == null || cls.TeacherId != user.Value) return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);

            db.OnlineClasses.Remove(cls);
            db.SaveChanges();
            TempData["Success"] = "Class deleted.";
            return RedirectToAction("Index");
        }

        // ── CREATE ──────────────────
        [HttpGet]
        public ActionResult Create()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToLogin();

            var teacherSubjects = db.TeacherSubjects.Where(ts => ts.TeacherId == userId.Value).Select(ts => ts.Subject).Distinct().Select(s => new SelectListItem { Value = s.Name, Text = s.Name }).ToList();
            var teacherGrades = db.TimetableSlots.Where(ts => ts.TeacherId == userId.Value).Select(ts => ts.Class).Distinct().ToList().Select(c => new SelectListItem { Value = c.Id.ToString(), Text = "Grade " + c.Name }).OrderBy(s => s.Text).ToList();

            ViewBag.GradeList = teacherGrades;
            return View(new CreateClassViewModel { SubjectList = teacherSubjects, ScheduledAt = DateTime.Now.AddHours(1), DurationMinutes = 60 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateClassViewModel model)
        {
            int? userId = GetCurrentUserId();
            if (!IsTeacherUser() || userId == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.Forbidden);

            if (!ModelState.IsValid) return Create();

            var cls = new OnlineClass
            {
                Title = model.Title,
                Description = model.Description,
                Subject = model.Subject,
                ScheduledAt = model.ScheduledAt,
                DurationMinutes = model.DurationMinutes,
                MaxStudents = model.MaxStudents,
                JoinPassword = model.JoinPassword ?? string.Empty,
                MeetingRoomId = JitsiService.GenerateRoomId(model.Title, userId.Value),
                TeacherId = userId.Value,
                CreatedAt = DateTime.Now,
                IsActive = false,
                IsCompleted = false
            };

            db.OnlineClasses.Add(cls);
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