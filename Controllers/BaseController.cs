using System.Web.Mvc;
using SchoolTimetable.Models;

namespace SchoolTimetable.Controllers
{
    public class BaseController : Controller
    {
        // Check if a user is logged in at all
        protected bool IsLoggedIn => Session["UserId"] != null;

        // Check if the specific Librarian role is set in the session
        protected bool IsLibrarian => Session["UserRole"]?.ToString() == "Librarian";

        protected ActionResult RequireLogin()
        {
            if (!IsLoggedIn)
                return RedirectToAction("Login", "Teachers");
            return null;
        }

        protected ActionResult RequireLibrarian()
        {
            // 1. If not logged in, go to Login page
            var loginCheck = RequireLogin();
            if (loginCheck != null) return loginCheck;

            // 2. If logged in but NOT a librarian, go to Login (or an Access Denied page)
            if (!IsLibrarian)
            {
                TempData["Error"] = "Access Denied: Librarian permissions required.";
                return RedirectToAction("Login", "Teachers");
            }

            return null; // All checks passed
        }
    }
}