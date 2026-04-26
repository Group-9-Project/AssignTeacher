using SchoolTimetable.Models;
using SchoolTimetable.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace SchoolTimetable.Controllers
{
    public class LibrarianController : BaseController
    {
        // GET: Librarian/LibrarianDashboard
        public ActionResult LibrarianDashboard()
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                // Update overdue statuses based on current date
                ApplicationDbContext.UpdateOverdueStatuses(db);

                var vm = new LibrarianDashboardViewModel
                {
                    TotalBooks = db.Books.Count(b => b.IsActive),
                    TotalStudents = db.AppUsers.Count(u => u.Role == UserRole.Student),
                    ActiveBorrowings = db.Borrowings.Count(b => b.Status == BorrowStatus.Active),
                    OverdueBorrowings = db.Borrowings.Count(b => b.Status == BorrowStatus.Overdue),
                    BlockedStudents = db.AppUsers.Count(u => u.IsBlocked),

                    // Uses the virtual 'Student' property which links via AppUserId
                    RecentBorrowings = db.Borrowings
                        .Include(b => b.Book)
                        .Include(b => b.Student)
                        .OrderByDescending(b => b.BorrowedDate)
                        .Take(6).ToList(),

                    OverdueList = db.Borrowings
                        .Include(b => b.Book)
                        .Include(b => b.Student)
                        .Where(b => b.Status == BorrowStatus.Overdue)
                        .OrderBy(b => b.DueDate)
                        .Take(10).ToList()
                };

                return View("LibrarianDashboard", vm);

            }
        }

        // GET: Librarian/ManageBooks
        public ActionResult ManageBooks()
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                var books = db.Books.OrderBy(b => b.Title).ToList();
                return View(books);
            }
        }

        // POST: Librarian/AddBook
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AddBook(Book book)
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            if (ModelState.IsValid)
            {
                using (var db = new ApplicationDbContext())
                {
                    book.AvailableCopies = book.TotalCopies;
                    book.AddedAt = DateTime.Now;
                    book.IsActive = true;
                    db.Books.Add(book);
                    db.SaveChanges();
                    TempData["Success"] = "Book added successfully.";
                }
            }
            return RedirectToAction("ManageBooks");
        }

        public ActionResult Members(int? grade, bool? blocked, string search)
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                // Start with all students
                var query = db.AppUsers.Where(u => u.Role == UserRole.Student).AsQueryable();

                // Apply Filters
                if (grade.HasValue)
                    query = query.Where(u => u.Grade == grade);

                if (blocked.HasValue)
                    query = query.Where(u => u.IsBlocked == blocked.Value);

                if (!string.IsNullOrEmpty(search))
                    query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

                // Persist filter states for the UI
                ViewBag.Grade = grade;
                ViewBag.Blocked = blocked;
                ViewBag.Search = search;

                var students = query.OrderBy(u => u.FullName).ToList();
                return View(students);
            }
        }


        // GET: Librarian/Details/5
        public ActionResult Details(int id)
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                // Using AppUsers to match your project's identity naming convention
                var student = db.AppUsers.Find(id);
                if (student == null) return HttpNotFound();

                var borrowings = db.Borrowings
                    .Include(b => b.Book) // Strong-typed include
                    .Where(b => b.AppUserId == id) // Ensure this matches your FK name
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

        public ActionResult Students(string search)
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                var query = db.AppUsers.Where(u => u.Role == UserRole.Student);

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
                }

                return View(query.OrderBy(u => u.FullName).ToList());
            }
        }

        // POST: Librarian/Block
        // POST: Librarian/Block
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Block(BlockStudentViewModel vm)
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            if (!ModelState.IsValid) return RedirectToAction("Details", new { id = vm.StudentId });

            using (var db = new ApplicationDbContext())
            {
                // Updated to use AppUsers and vm.StudentId
                var student = db.AppUsers.Find(vm.StudentId);
                if (student == null) return HttpNotFound();

                student.IsBlocked = true;
                student.BlockReason = vm.BlockReason;

                db.SaveChanges();
                TempData["Success"] = student.FullName + " has been blocked.";
            }
            return RedirectToAction("Details", new { id = vm.StudentId });
        }

        // POST: Librarian/Unblock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Unblock(int id)
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                // Updated to use AppUsers to match the Block logic
                var student = db.AppUsers.Find(id);
                if (student == null) return HttpNotFound();

                var hasUnpaidFines = db.Borrowings.Any(b =>
                    b.Id == id && !b.FinePaid && b.FineAmount > 0 &&
                    (b.Status == SchoolTimetable.Models.BorrowStatus.Overdue ||
                     (b.Status == SchoolTimetable.Models.BorrowStatus.Returned && b.FineAmount > 0)));

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
    }
}