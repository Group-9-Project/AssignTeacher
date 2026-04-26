using SchoolTimetable.Models;
using SchoolTimetable.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace SchoolTimetable.Controllers
{
    public class BooksController : BaseController
    {
        // GET: /Books
        public ActionResult Index(string search, string genre, int? grade)
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                var query = db.Books.Where(b => b.IsActive).AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                    query = query.Where(b => b.Title.Contains(search) || b.Author.Contains(search) || b.ISBN.Contains(search));

                if (!string.IsNullOrWhiteSpace(genre))
                    query = query.Where(b => b.Genre == genre);

                if (grade.HasValue)
                    query = query.Where(b => (b.MinGrade == null || b.MinGrade <= grade) && (b.MaxGrade == null || b.MaxGrade >= grade));

                var vm = new BookListViewModel
                {
                    Books = query.OrderBy(b => b.Title).ToList(),
                    Search = search,
                    Genre = genre,
                    Grade = grade,
                    Genres = db.Books.Where(b => b.IsActive && b.Genre != null).Select(b => b.Genre).Distinct().OrderBy(g => g).ToList()
                };
                return View(vm);
            }
        }

        // GET: /Books/Details/5
        public ActionResult Details(int id)
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                var book = db.Books.Include(b => b.Borrowings.Select(br => br.Student)).FirstOrDefault(b => b.Id == id);
                if (book == null) return HttpNotFound();

                Borrowing myBorrow = null;
                // FIXED: Replaced CurrentUser with Session check
                if (Session["UserId"] != null)
                {
                    int uid = Convert.ToInt32(Session["UserId"]);
                    myBorrow = db.Borrowings.FirstOrDefault(b => b.BookId == id && b.AppUserId == uid &&
                        (b.Status == BorrowStatus.Active || b.Status == BorrowStatus.Overdue || b.Status == BorrowStatus.Reserved));
                }
                ViewBag.MyBorrow = myBorrow;
                return View(book);
            }
        }

        // POST /Books/Reserve/5
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Reserve(int id)
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                int uid = Convert.ToInt32(Session["UserId"]);
                var user = db.AppUsers.Find(uid);

                if (user == null)
                {
                    TempData["Error"] = "User session invalid. Please log in again.";
                    return RedirectToAction("Details", new { id });
                }

                // 1. Block check
                if (user.IsBlocked)
                {
                    TempData["Error"] = "Your account is blocked: " + user.BlockReason;
                    return RedirectToAction("Details", new { id });
                }

                var book = db.Books.Find(id);
                if (book == null || book.AvailableCopies <= 0)
                {
                    TempData["Error"] = "No copies available.";
                    return RedirectToAction("Details", new { id });
                }

                // 2. Borrowing limit
                int activeBorrowCount = db.Borrowings.Count(b =>
                    b.AppUserId == uid &&
                    b.Status != BorrowStatus.Returned);

                if (activeBorrowCount >= 3)
                {
                    TempData["Error"] = "You have reached the maximum borrowing limit.";
                    return RedirectToAction("Details", new { id });
                }

                // ✅ VALID RESERVATION
                var reservation = new Borrowing
                {
                    AppUserId = uid,
                    BookId = id,
                    BorrowedDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(2),
                    Status = BorrowStatus.Reserved
                };

                book.AvailableCopies--;

                db.Borrowings.Add(reservation);
                db.SaveChanges();

                TempData["Success"] = "Book reserved successfully.";
                return RedirectToAction("StudentDashboard", "StudentAccount");
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CancelReservation(int id)
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                int uid = Convert.ToInt32(Session["UserId"]);
                var reservation = db.Borrowings.FirstOrDefault(b => b.BookId == id &&
                                    b.AppUserId == uid &&
                                    b.Status == BorrowStatus.Reserved);

                if (reservation != null)
                {
                    var book = db.Books.Find(id);
                    if (book != null) book.AvailableCopies++;

                    db.Borrowings.Remove(reservation);
                    db.SaveChanges();
                    TempData["Success"] = "Reservation cancelled.";
                }
            }
            return RedirectToAction("Details", new { id = id });
        }

        // --- Librarian Actions ---

        public ActionResult Create()
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;
            return View(new BookFormViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Create(BookFormViewModel model)
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            if (!ModelState.IsValid) return View(model);

            using (var db = new ApplicationDbContext())
            {
                var book = new Book
                {
                    Title = model.Title,
                    Author = model.Author,
                    ISBN = model.ISBN,
                    Genre = model.Genre,
                    Publisher = model.Publisher,
                    PublicationYear = model.PublicationYear,
                    TotalCopies = model.TotalCopies,
                    AvailableCopies = model.TotalCopies,
                    Description = model.Description,
                    MinGrade = model.MinGrade,
                    MaxGrade = model.MaxGrade,
                    AddedAt = DateTime.Now,
                    IsActive = true
                };
                db.Books.Add(book);
                db.SaveChanges();
                TempData["Success"] = "Book '" + book.Title + "' added to catalogue.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var redirect = RequireLibrarian();
            if (redirect != null) return redirect;

            using (var db = new ApplicationDbContext())
            {
                var book = db.Books.Find(id);
                if (book != null)
                {
                    book.IsActive = false;
                    db.SaveChanges();
                }
                TempData["Success"] = "Book removed from catalogue.";
            }
            return RedirectToAction("Index");
        }
    }
}