using SchoolTimetable.Models;
using SchoolTimetable.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace SchoolTimetable.Controllers
{
    public class StudentAccountController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

      
        public ActionResult Index()
        {
            if (Session["UserRole"]?.ToString() != "Student") return RedirectToAction("Login", "Teachers");
            return View();
        }



        public ActionResult StudentDashboard()
        {

            if (Session["UserId"] == null || Session["UserRole"]?.ToString() != "Student")
            {
                return RedirectToAction("Login", "Teachers");
            }

            int sessionUserId = (int)Session["UserId"];

            var studentNumber = Session["StudentNumber"]?.ToString();

            var currentUser = db.AppUsers
                .Include(u => u.StudentAccount)
                .FirstOrDefault(u => u.Id == sessionUserId);


            if (currentUser == null)
            {
                return HttpNotFound("User not found.");
            }

            string sName = currentUser.StudentAccount?.FullName ?? currentUser.FullName;
            string sNum = currentUser.StudentAccount?.StudentNumber;

            bool isBlocked = currentUser.IsBlocked;
            string blockReason = currentUser.BlockReason;

            var studentData = (from stu in db.Students
                               join app in db.Applications on stu.ApplicationId equals app.Id
                               where stu.StudentNumber == studentNumber
                               select new { stu, app }).FirstOrDefault();
            if (currentUser.StudentAccount != null)
            {
                sName = currentUser.StudentAccount.FullName;
                sNum = currentUser.StudentAccount.StudentNumber;
                isBlocked = currentUser.StudentAccount.IsBlocked;
                blockReason = currentUser.StudentAccount.BlockReason;
            }

            var viewModel = new StudentDashboardViewModel
            {
                StudentName = sName,
                StudentNumber = sNum,
                IsBlocked = isBlocked,
                BlockReason = blockReason,
                MyBorrowings = db.Borrowings
                    .Include(b => b.Book)
                    .Where(b => b.AppUserId == sessionUserId)
                    .OrderByDescending(b => b.BorrowedDate)
                    .ToList(),
                RecentBooks = db.Books
                    .Where(b => b.IsActive && b.AvailableCopies > 0)
                    .Take(6).ToList(),
                OutstandingFines = db.Borrowings
                    .Where(b => b.AppUserId == sessionUserId && !b.FinePaid)
                    .Select(b => b.FineAmount)
                    .DefaultIfEmpty(0m)
                    .Sum()
            };

            if (studentData?.app.AssignedClassId != null)
            {
                int classId = studentData.app.AssignedClassId.Value;

                viewModel.ClassTimetable = db.TimetableSlots
                    .Where(s => s.ClassId == classId)
                    .OrderBy(s => s.DayOfWeek)
                    .ThenBy(s => s.Period)
                    .ToList();
            }

            return View(viewModel);
        }

        public ActionResult MyTimetable()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login", "Teachers");

            int userId = (int)Session["UserId"];

            var studentData = (from user in db.AppUsers
                               join account in db.StudentAccounts on user.Id equals account.Id
                               join student in db.Students on account.StudentNumber equals student.StudentNumber
                               join app in db.Applications on student.ApplicationId equals app.Id
                               where user.Id == userId
                               select new
                               {
                                   student,
                                   app
                               }).FirstOrDefault();
            if (studentData?.app?.AssignedClassId == null)
            {
                TempData["Error"] = "You have not been allocated to a class yet.";
                return RedirectToAction("Index");
            }

            int classId = studentData.app.AssignedClassId.Value;

            var slots = db.TimetableSlots
         .Include(s => s.Subject)
         .Include(s => s.Teacher)
         .Where(s => s.ClassId == classId && s.IsActive)
         .OrderBy(s => s.DayOfWeek)
         .ThenBy(s => s.Period)
         .ToList();

            return View(slots);
        }
        public ActionResult WriteQuiz()
        {
            if (Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Teachers");

            int userId = (int)(Session["UserId"] ?? 0);
            var account = db.StudentAccounts.Find(userId);
            var studentRecord = db.Students.Find(userId);
            string sNum = account?.StudentNumber ?? studentRecord?.StudentNumber;

            var testTemplate = db.Entrance_tests.OrderByDescending(t => t.TestId).FirstOrDefault();

            if (testTemplate == null)
            {
                TempData["Error"] = "No quiz found.";
                return RedirectToAction("Index");
            }

            var existingResult = db.Entrance_tests
                                   .FirstOrDefault(t => t.StudentNumber == sNum && t.TotalScore > 0);

            if (existingResult != null)
            {
                TempData["Info"] = "You have already completed the entrance test.";
                return RedirectToAction("TestResult", new { TestId = existingResult.TestId });
            }

            return View(testTemplate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult WriteQuiz(int TestId, string selectedAnswer1, string selectedAnswer2,
                                       string selectedAnswer3, string selectedAnswer4, string selectedAnswer5)
        {
            var template = db.Entrance_tests.Find(TestId);
            if (template == null) return HttpNotFound();

            int userId = (int)(Session["UserId"] ?? 0);
            var account = db.StudentAccounts.Find(userId);
            var studentRecord = db.Students.Find(userId);

            string systemStudentNumber = account?.StudentNumber ?? studentRecord?.StudentNumber;
            string fullName = account != null ? (account.FirstName + " " + account.LastName) : studentRecord?.FullName;

            if (string.IsNullOrEmpty(systemStudentNumber))
            {
                TempData["Error"] = "Unable to verify student identity.";
                return RedirectToAction("Index");
            }

            int totalCorrect = 0;
            totalCorrect += template.CalcScore(selectedAnswer1, template.CorrectAnswer1);
            totalCorrect += template.CalcScore(selectedAnswer2, template.CorrectAnswer2);
            totalCorrect += template.CalcScore(selectedAnswer3, template.CorrectAnswer3);
            totalCorrect += template.CalcScore(selectedAnswer4, template.CorrectAnswer4);
            totalCorrect += template.CalcScore(selectedAnswer5, template.CorrectAnswer5);

            double percent = template.CalcPercent(totalCorrect, 5);

            var studentAttempt = new Entrance_test
            {
                Title = "Entrance Test - " + (fullName ?? systemStudentNumber),
                Date = DateTime.Now,
                StudentNumber = systemStudentNumber,
                counter = totalCorrect,
                TotalScore = percent,
                QuetionOne = template.QuetionOne,
                QuetionTwo = template.QuetionTwo,
                QuetionThree = template.QuetionThree,
                QuetionFour = template.QuetionFour,
                QuetionFive = template.QuetionFive
            };

            db.Entrance_tests.Add(studentAttempt);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Thank you for completing the Entrance Test!";
            return RedirectToAction("TestResult", new { TestId = studentAttempt.TestId });
        }

        public ActionResult TestResult(int? TestId)
        {
            if (TestId == null) return RedirectToAction("Index");

            var test = db.Entrance_tests.Find(TestId);
            if (test == null) return HttpNotFound();

            return View(test);
        }

        public ActionResult EditProfile()
        {
            if (Session["UserRole"]?.ToString() != "Student") return RedirectToAction("Login", "Teachers");

            int userId = (int)(Session["UserId"] ?? 0);
            var student = db.StudentAccounts.Find(userId);

            if (student != null) return View(student);
            return View();
        }

       
        public ActionResult Dashboard()
        {
            if (Session["UserId"] == null || Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Teachers");

            return View();
        }

        public ActionResult Announcement()
        {
            if (Session["UserId"] == null || Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Teachers");

           
            var announcements = db.Announcements
                                  .OrderByDescending(a => a.DatePosted)
                                  .ToList();
            return View(announcements);
        }

        public ActionResult LearningMaterials()
        {
            if (Session["UserId"] == null || Session["UserRole"]?.ToString() != "Student")
                return RedirectToAction("Login", "Teachers");

     
            var materials = db.LearningMaterials
                              .OrderByDescending(m => m.UploadDate)
                              .ToList();
            return View(materials);
        }

        public FileResult DownloadMaterial(int id)
        {
      
            if (Session["UserId"] == null) return null;

            var material = db.LearningMaterials.Find(id);

            if (material == null || string.IsNullOrEmpty(material.FilePath))
            {
                return null;
            }

     
            string physicalPath = Server.MapPath(material.FilePath);

            if (!System.IO.File.Exists(physicalPath))
            {
                return null;
            }

            var fileBytes = System.IO.File.ReadAllBytes(physicalPath);
            return File(fileBytes, material.FileType, material.FileName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}