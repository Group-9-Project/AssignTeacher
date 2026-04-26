using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SchoolTimetable.Models;
using SchoolTimetable.ViewModels; // Updated Namespace

namespace SchoolTimetable.Controllers
{
    public class StudentsController : BaseController
    {
        // GET: /Students
        public ActionResult Index(int? grade, bool? blocked)
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                // Accessing AppUsers via the new shared context
                var query = db.AppUsers.Where(u => u.Role == UserRole.Student).AsQueryable();

                // Note: If you removed 'Grade' from AppUser to keep it in the 'Student' entity, 
                // you would join the Students table here.
                if (grade.HasValue) query = query.Where(u => u.Grade == grade);
                if (blocked.HasValue) query = query.Where(u => u.IsBlocked == blocked.Value);

                ViewBag.Grade = grade;
                ViewBag.Blocked = blocked;
                return View(query.OrderBy(u => u.FullName).ToList());
            }
        }

        // GET: /Students/Details/5
        public ActionResult Details(int id)
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                var student = db.AppUsers.Find(id);
                if (student == null) return HttpNotFound();

                // Corrected StudentId to AppUserId to match our Borrowing model
                var borrowings = db.Borrowings.Include(b => b.Book)
                    .Where(b => b.AppUserId == id)
                    .OrderByDescending(b => b.BorrowedDate)
                    .ToList();

                var vm = new StudentProfileViewModel
                {
                    Student = student,
                    Borrowings = borrowings,
                    TotalFines = borrowings.Where(b => !b.FinePaid).Sum(b => b.FineAmount),
                    ActiveCount = borrowings.Count(b => b.Status == BorrowStatus.Active),
                    OverdueCount = borrowings.Count(b => b.Status == BorrowStatus.Overdue)
                };
                return View(vm);
            }
        }

        // POST: /Students/Block
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Block(BlockStudentViewModel vm)
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            if (!ModelState.IsValid) return RedirectToAction("Details", new { id = vm.StudentId });

            using (var db = new ApplicationDbContext())
            {
                var student = db.AppUsers.Find(vm.StudentId);
                if (student == null) return HttpNotFound();

                student.IsBlocked = true;
                student.BlockReason = vm.BlockReason;
                db.SaveChanges();
                TempData["Success"] = student.FullName + " has been blocked.";
            }
            return RedirectToAction("Details", new { id = vm.StudentId });
        }

        // POST: /Students/Unblock/5
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Unblock(int id)
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                var student = db.AppUsers.Find(id);
                if (student == null) return HttpNotFound();

                // Logic check: Ensure no unpaid fines exist before unblocking
                var hasUnpaidFines = db.Borrowings.Any(b =>
                    b.AppUserId == id && !b.FinePaid && b.FineAmount > 0 &&
                    (b.Status == BorrowStatus.Overdue ||
                     (b.Status == BorrowStatus.Returned && b.FineAmount > 0)));

                if (hasUnpaidFines)
                {
                    TempData["Error"] = "Cannot unblock: student has unpaid fines. Clear fines first.";
                }
                else
                {
                    student.IsBlocked = false;
                    student.BlockReason = null;
                    db.SaveChanges();
                    TempData["Success"] = student.FullName + " has been unblocked.";
                }
            }
            return RedirectToAction("Details", new { id });
        }

        // POST: /Students/ClearFine
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ClearFine(int borrowingId, int studentId)
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                var b = db.Borrowings.Find(borrowingId);
                if (b != null)
                {
                    b.FinePaid = true;
                    db.SaveChanges();
                }
                TempData["Success"] = "Fine marked as paid.";
            }
            return RedirectToAction("Details", new { id = studentId });
        }
    }
}