using System.Web.Mvc;

namespace SchoolTimetable.Controllers
{
    public class AccountController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            // Clear session and redirect to the shared login (Teachers.Login)
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Teachers");
        }
    }
}
