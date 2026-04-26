using SchoolTimetable.Models;
using SchoolTimetable.ViewModels;
using SchoolTimetable.Services;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace SchoolTimetable.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private readonly EmailService _emailService = new EmailService();

        private bool IsAdmin() => Session["UserRole"]?.ToString() == "Admin";

        public ActionResult Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Teachers");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PublishAndInvite(int id, DateTime testDate, string testTime)
        {
            // 1. Find the test
            var test = db.Entrance_tests.Find(id);
            if (test == null) return HttpNotFound();

            // 2. IMPORTANT: Look for PAID APPLICANTS in the Applications table
            // This matches the "IsPaid" logic used in your ConfirmedPayments method
            var paidApplicants = db.Applications
                .Where(a => a.IsPaid == true && a.Status == ApplicationStatus.Accept)
                .ToList();

            // 3. If no one is found, show the error
            if (!paidApplicants.Any())
            {
                TempData["Error"] = "No accepted applicants with 'Paid' status were found.";
                return RedirectToAction("Dashboard"); // Stay on the same page
            }

            int inviteCount = 0;
            foreach (var app in paidApplicants)
            {
                try
                {
                    // Use 'app.Email' and 'app.FirstName' from the Application model
                    _emailService.SendEntranceTestInvite(
                        app.Email,
                        app.FirstName + " " + app.LastName,
                        test.Title,
                        testDate.ToShortDateString(),
                        testTime,
                        test.DurationMinutes.ToString()
                    );
                    inviteCount++;
                }
                catch (Exception ex)
                {
                    // If one email fails, we catch the error so the others can continue
                    System.Diagnostics.Debug.WriteLine("Email Error: " + ex.Message);
                }
            }

            TempData["Success"] = $"Invitations successfully sent to {inviteCount} parents.";
            return RedirectToAction("Dashboard");
        }

        public ActionResult ManageStudentsDashboard()
        {
            var allApplicants = db.Applications
                         .AsNoTracking() // Optional: Improves performance for read-only views
                         .OrderByDescending(a => a.AverageMark)
                         .ToList();

            // 3. Send the full list to the View
            return View(allApplicants);
        }

        public ActionResult ApplicationDetails(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Teachers");

            var app = db.Applications
                        .Include(a => a.Marks)
                        .FirstOrDefault(a => a.Id == id);

            if (app == null) return HttpNotFound();
            return View(app);
        }
        public ActionResult ConfirmedPayments()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Teachers");

            // Filter for applications that are NOT processed yet but have ISPAID = true
            var paidApplicants = db.Applications
                                   .Where(a => a.IsPaid == true && a.IsProcessed == false)
                                   .OrderByDescending(a => a.AverageMark)
                                   .ToList();

            return View("ManageStudentsDashboard", paidApplicants);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessApplication(int id, string decision, string adminFeedback)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Teachers");

            // 1. Fetch application with marks
            var app = db.Applications.Include(a => a.Marks).FirstOrDefault(a => a.Id == id);
            if (app == null) return HttpNotFound();

            // Store the email locally immediately to ensure it's available for the service
            string targetEmail = app.Email;

            if (Enum.TryParse(decision, out ApplicationStatus statusResult))
            {
                app.Status = statusResult;
            }

            app.IsProcessed = true;

            if (app.Status == ApplicationStatus.Accept)
            {
                // Generate credentials
                string studentNum = DateTime.Now.Year + "STU" + app.Id.ToString("D3");
                string tempPass = Guid.NewGuid().ToString().Substring(0, 8);

                try
                {
                    // A. Create the AppUser (System Identity)
                    var newAppUser = new AppUser
                    {
                        FullName = $"{app.FirstName} {app.LastName}",
                        Email = targetEmail,
                        PasswordHash = tempPass,
                        Role = UserRole.Student,
                        Grade = int.TryParse(app.GradeApplyingFor, out int g) ? g : (int?)null,
                        CreatedAt = DateTime.Now
                    };
                    db.AppUsers.Add(newAppUser);
                    db.SaveChanges(); // Generates newAppUser.Id

                    // B. Create the StudentAccount (Login Profile)
                    var newProfile = new StudentAccount
                    {
                        Id = newAppUser.Id, // Linking FK to AppUser
                        StudentNumber = studentNum,
                        FirstName = app.FirstName,
                        LastName = app.LastName,
                        Email = targetEmail,
                        TemporaryPassword = tempPass,
                        IsActive = true
                    };
                    db.StudentAccounts.Add(newProfile);

                    // C. Create the Student (Financial/Fee Record)
                    var financialRecord = new Student
                    {
                        FullName = $"{app.FirstName} {app.LastName}",
                        StudentNumber = studentNum,
                        Password = tempPass,
                        ParentEmail = targetEmail,
                        ApplicationId = app.Id,
                        RegistrationFeePaid = false,
                        Balance = 1000.00m
                    };
                    db.Students.Add(financialRecord);

                    // Save both profile and financial records
                    db.SaveChanges();

                    // D. SEND EMAIL 
                    // We use the locally stored targetEmail to avoid any "Entity Detached" issues
                    bool emailResult = _emailService.SendApplicationAcceptance(app, studentNum, tempPass);

                    if (emailResult)
                    {
                        TempData["SuccessMessage"] = $"Application for {app.FirstName} accepted and email sent.";
                    }
                    else
                    {
                        TempData["Error"] = "Student created, but the Email Service failed. Please check SMTP logs.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Database Error: " + (ex.InnerException?.Message ?? ex.Message);
                    return RedirectToAction("ManageStudentsDashboard");
                }
            }
            else if (app.Status == ApplicationStatus.Reject)
            {
                try
                {
                    db.SaveChanges();
                    _emailService.SendApplicationRejection(app, adminFeedback);
                    TempData["SuccessMessage"] = $"Application for {app.FirstName} has been rejected and email sent.";
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "Error processing rejection: " + ex.Message;
                }
            }

            return RedirectToAction("ManageStudentsDashboard");
        }

        public ActionResult AllocateStudents()
        {
            if (Session["UserRole"]?.ToString() != "Admin")
                return RedirectToAction("Login", "Teachers");

            var list = (from stu in db.Students
                        join app in db.Applications on stu.ApplicationId equals app.Id
                        join test in db.Entrance_tests on stu.StudentNumber equals test.StudentNumber
                        join cls in db.Classes on app.AssignedClassId equals cls.Id into classJoin
                        from assignedClass in classJoin.DefaultIfEmpty()
                        select new AllocationViewModel
                        {
                            ApplicationId = app.Id,
                            FullName = stu.FullName,
                            Grade = app.GradeApplyingFor,
                            Score = test.TotalScore,
                            CorrectCount = test.counter,
                            StudentNumber = stu.StudentNumber,
                            AllocatedClassName = assignedClass != null ? assignedClass.Name : "",
                            IsAllocated = assignedClass != null
                        }).ToList();

            ViewBag.Classes = db.Classes.Where(c => c.IsActive).ToList();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FinalizeAllocation(int appId, int classId)
        {
            var application = db.Applications.Find(appId);
            if (application != null)
            {
                application.AssignedClassId = classId;
                db.SaveChanges();
                TempData["Success"] = "Student successfully allocated!";
            }

        
            return RedirectToAction("AllocateStudents");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetStudentTest(string studentNumber)
        {
           
            var testRecord = db.Entrance_tests.FirstOrDefault(t => t.StudentNumber == studentNumber);

            if (testRecord != null)
            {
                db.Entrance_tests.Remove(testRecord);
                db.SaveChanges();
                TempData["Success"] = "Test reset successfully. The student can now log in and retake it.";
            }

            return RedirectToAction("AllocateStudents");
        }
        public ActionResult ManageTeacherDashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Teachers");

            int todayDay = (int)DateTime.Today.DayOfWeek;
            int schoolDay = (todayDay >= 1 && todayDay <= 5) ? todayDay : 1;

            var vm = new DashboardViewModel
            {
                TeacherCount = db.Teachers.Count(t => t.IsActive),
                SubjectCount = db.Subjects.Count(s => s.IsActive),
                ClassCount = db.Classes.Count(c => c.IsActive),
                TimetableSlotCount = db.TimetableSlots.Count(s => s.IsActive),
                PendingSubstitutionsCount = db.Substitutions.Count(s => s.Status == SubstitutionStatus.Pending),
                TodaySlots = db.TimetableSlots
                    .Include(s => s.Teacher).Include(s => s.Subject).Include(s => s.Class)
                    .Where(s => s.DayOfWeek == schoolDay && s.IsActive)
                    .OrderBy(s => s.Period).Take(8).ToList(),
                RecentSubstitutions = db.Substitutions
                    .Include(s => s.OriginalTeacher).Include(s => s.SubstituteTeacher)
                    .Include(s => s.TimetableSlot).Include(s => s.TimetableSlot.Subject)
                    .OrderByDescending(s => s.CreatedAt).Take(5).ToList()
            };

            return View(vm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}