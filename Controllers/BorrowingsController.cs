using SchoolTimetable.Models;
using SchoolTimetable.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace SchoolTimetable.Controllers
{
    public class BorrowingsController : BaseController
    {
        // GET: /Borrowings
        public ActionResult Index(string status)
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                var query = db.Borrowings
                    .Include(b => b.Book)
                    .Include(b => b.Student) // In your model, Student is the navigation property for AppUser
                    .AsQueryable();


                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(b => b.Status == status);
                }


                ViewBag.Status = status;
                return View(query.OrderByDescending(b => b.BorrowedDate).ToList());
            }
        }

        // GET: /Borrowings/Mine
        public ActionResult Mine()
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                // FIXED: Replaced CurrentUser with Session check
                if (Session["UserId"] == null) return RedirectToAction("Login", "Teachers");

                int uid = Convert.ToInt32(Session["UserId"]);

                var myBorrowings = db.Borrowings
                    .Include(b => b.Book)
                    .Where(b => b.AppUserId == uid)
                    .ToList();

                // Get user details for blocking info
                var user = db.AppUsers.Find(uid);
                ViewBag.IsBlocked = user?.IsBlocked ?? false;
                ViewBag.BlockReason = user?.BlockReason ?? "Please settle outstanding fines to resume borrowing.";

                return View(myBorrowings);
            }
        }

        // GET: /Borrowings/Issue
        public ActionResult Issue(int bookId, int? reservationId = null)
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                var book = db.Books.Find(bookId);
                if (book == null) return Content("<div class='alert alert-danger'>Book not found.</div>");

                var vm = new IssueBookViewModel
                {
                    BookId = bookId,
                    Book = book,
                    ReservationId = reservationId,
                    DueDate = DateTime.Today.AddDays(14),
                    Students = db.AppUsers.Where(u => u.Role == UserRole.Student && !u.IsBlocked)
                                         .OrderBy(u => u.FullName).ToList()
                };

                if (reservationId.HasValue)
                {
                    var res = db.Borrowings.Find(reservationId.Value);
                    if (res != null) vm.StudentId = res.AppUserId;
                }
                else if (book.AvailableCopies < 1)
                {
                    return Content("<div class='alert alert-danger'>No copies available.</div>");
                }

                return PartialView(vm);
            }
        }

        // POST: /Borrowings/Issue
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Issue(IssueBookViewModel model)
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                if (ModelState.IsValid)
                {
                    if (model.ReservationId.HasValue)
                    {
                        var res = db.Borrowings.Find(model.ReservationId.Value);
                        if (res != null)
                        {
                            res.Status = BorrowStatus.Active;
                            res.BorrowedDate = DateTime.Now;
                            res.DueDate = model.DueDate;
                            res.Notes = model.Notes;
                        }
                    }
                    else
                    {
                        var book = db.Books.Find(model.BookId);
                        if (book != null && book.AvailableCopies > 0)
                        {
                            db.Borrowings.Add(new Borrowing
                            {
                                BookId = model.BookId,
                                AppUserId = model.StudentId,
                                BorrowedDate = DateTime.Now,
                                DueDate = model.DueDate,
                                Status = BorrowStatus.Active,
                                Notes = model.Notes
                            });
                            book.AvailableCopies--;
                        }
                    }
                    db.SaveChanges();
                    TempData["Success"] = "Book issued successfully.";
                }
                return RedirectToAction("Index");
            }
        }

        // POST: /Borrowings/Return/5
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Return(int id)
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                var b = db.Borrowings.Include(x => x.Book).FirstOrDefault(x => x.Id == id);
                if (b != null && (b.Status == BorrowStatus.Active || b.Status == BorrowStatus.Overdue))
                {
                    b.ReturnedDate = DateTime.Now;
                    b.Status = BorrowStatus.Returned;
                    b.Book.AvailableCopies++;
                    db.SaveChanges();
                    TempData["Success"] = "Returned successfully.";
                }
                return RedirectToAction("Index");
            }
        }

        // POST: /Borrowings/MarkLost/5
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult MarkLost(int id, decimal fineAmount)
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                var b = db.Borrowings.Include(x => x.Book).FirstOrDefault(x => x.Id == id);
                if (b != null)
                {
                    b.Status = BorrowStatus.Lost;
                    b.FineAmount = fineAmount;
                    b.FinePaid = false;

                    if (b.Book != null)
                        b.Book.TotalCopies = Math.Max(0, b.Book.TotalCopies - 1);

                    db.SaveChanges();
                    TempData["Info"] = $"Marked as Lost. Penalty of R{fineAmount:F2} applied.";
                }
                return RedirectToAction("Index");
            }
        }
    }
}