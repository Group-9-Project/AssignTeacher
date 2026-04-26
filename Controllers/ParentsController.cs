using SchoolTimetable.Models;
using SchoolTimetable.Services;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace SchoolTimetable.Controllers
{
    public sealed class ParentsController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();
        private readonly EmailService _emailService = new EmailService();

        public ParentsController()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var apiKey = ConfigurationManager.AppSettings["Stripe:SecretKey"];
            if (!string.IsNullOrEmpty(apiKey))
            {
                StripeConfiguration.ApiKey = apiKey;
            }
        }

      

        public ActionResult Index()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login");
            int parentId = (int)Session["UserId"];

            var apps = db.Applications.AsNoTracking()
                         .Where(a => a.ParentId == parentId)
                         .ToList();
            return View(apps);
        }

        public ActionResult MyApplications()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login");
            int currentUserId = (int)Session["UserId"];
            var myApps = db.Applications.AsNoTracking()
                           .Where(a => a.ParentId == currentUserId)
                           .OrderByDescending(a => a.SubmissionDate)
                           .ToList();
            return View(myApps);
        }

        public ActionResult Payment(int? id)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login");
            if (id == null) return RedirectToAction("MyApplications");

            var app = db.Applications.Find(id);
            if (app == null || app.ParentId != (int)Session["UserId"]) return HttpNotFound();

            return View(app);
        }

        // --- STRIPE PAYMENT LOGIC ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateCheckoutSession(int applicationId)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login");

            var app = db.Applications.Find(applicationId);
            if (app == null || app.ParentId != (int)Session["UserId"]) return HttpNotFound();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                Mode = "payment",
                SuccessUrl = Url.Action("PaymentSuccess", "Parents", new { id = applicationId }, Request.Url.Scheme),
                CancelUrl = Url.Action("Payment", "Parents", new { id = applicationId }, Request.Url.Scheme),
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = 100000,
                            Currency = "zar",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = "DGSS Registration Fee",
                                Description = $"Student: {app.FirstName} {app.LastName}"
                            }
                        },
                        Quantity = 1
                    }
                }
            };

            var service = new SessionService();
            try
            {
                var session = service.Create(options);
                return Redirect(session.Url);
            }
            catch (StripeException ex)
            {
                TempData["Error"] = "Stripe Error: " + ex.Message;
                return RedirectToAction("Payment", new { id = applicationId });
            }
        }

        public ActionResult PaymentSuccess(int id)
        {
            if (Session["UserId"] == null) return RedirectToAction("Login");
            int currentUserId = (int)Session["UserId"];

            var app = db.Applications.FirstOrDefault(a => a.Id == id && a.ParentId == currentUserId);

            if (app != null && !app.IsPaid)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // FIX: Update Application Record
                        app.IsPaid = true;

                        // NOTE: If you need a "Paid" status, add it to your Enum in the model first!
                        // For now, we keep it as Accept so it stays in their list or history correctly.
                        app.Status = ApplicationStatus.Accept;

                        db.Entry(app).State = EntityState.Modified;

                        var student = db.Students.FirstOrDefault(s => s.ApplicationId == app.Id);
                        if (student != null)
                        {
                            student.RegistrationFeePaid = true;
                            student.Balance = 0.00m;
                            db.Entry(student).State = EntityState.Modified;
                        }

                        db.SaveChanges();
                        transaction.Commit();

                        string parentName = Session["UserName"]?.ToString() ?? "Parent";
                        _emailService.SendRegistrationFeeConfirmation(
                            parentName,
                            $"{app.FirstName} {app.LastName}",
                            app.Email,
                            "PAY-" + id.ToString("D5")
                        );

                        TempData["Success"] = "Payment successful! Your application status has been updated.";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        TempData["Error"] = "System error during update: " + ex.Message;
                    }
                }
            }

            return View(app);
        }

        public ActionResult PaymentList()
        {
            if (Session["UserId"] == null) return RedirectToAction("Login");
            int parentId = (int)Session["UserId"];

            // FIX: Use the Enum ApplicationStatus.Reject instead of the string "Reject"
            var unpaidApps = db.Applications
                        .Where(a => a.ParentId == parentId
                                 && a.IsProcessed == true
                                 && a.IsPaid == false
                                 && a.Status != ApplicationStatus.Reject)
                        .ToList();

            return View(unpaidApps);
        }

        // --- AUTHENTICATION ---

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password)
        {
            var parent = db.Parents.FirstOrDefault(p => p.Email == email && p.Password == password);
            if (parent != null)
            {
                Session["UserId"] = parent.Id;
                Session["UserName"] = parent.FullName;
                Session["UserEmail"] = parent.Email;
                Session["UserRole"] = "Parent";
                return RedirectToAction("Index");
            }
            ViewBag.Error = "Invalid email or password.";
            return View();
        }
        // GET: Parents/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        // POST: Parents/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Parent model)
        {
            if (ModelState.IsValid)
            {
                var existingParent = db.Parents.Any(p => p.Email == model.Email);
                if (existingParent)
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(model);
                }

                model.CreatedAt = DateTime.Now;

                db.Parents.Add(model);
                db.SaveChanges();

                TempData["Success"] = "Registration successful!";
                return RedirectToAction("Login", "Teachers");
            }

            // If we reach here, validation failed. 
            // This will send the errors back to the ViewBag for your HTML to display.
            var errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            ViewBag.Error = string.Join(" | ", errorMessages);

            return View(model);
        }
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            var parent = db.Parents.FirstOrDefault(p => p.Email == email);

            if (parent == null)
            {
                ViewBag.Error = "No account found with that email address.";
                return View();
            }

            try
            {
                string otp = new Random().Next(100000, 999999).ToString();

                parent.ResetCode = otp;
                parent.ResetExpiry = DateTime.Now.AddMinutes(15);

                // Only update these two fields
                db.Entry(parent).Property(x => x.ResetCode).IsModified = true;
                db.Entry(parent).Property(x => x.ResetExpiry).IsModified = true;

                // Disable validation for this save
                db.Configuration.ValidateOnSaveEnabled = false;

                db.SaveChanges();

                db.Configuration.ValidateOnSaveEnabled = true;

                bool emailSent = _emailService.SendParentResetCode(
                    parent.FullName,
                    parent.Email,
                    otp
                );

                if (emailSent)
                {
                    TempData["Success"] = "Reset code sent successfully.";
                    return RedirectToAction("ResetPassword", new { email = email });
                }

                ViewBag.Error = "Failed to send reset email.";
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }

            return View();
        }
        // GET: Parents/ResetPassword
        public ActionResult ResetPassword(string email)
        {
            ViewBag.TargetEmail = email;
            return View();
        }

        [HttpPost]
        public ActionResult ResetPassword(string email, string code, string newPassword, string confirmNewPassword)
        {
            if (newPassword != confirmNewPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                ViewBag.TargetEmail = email;
                return View();
            }

            var parent = db.Parents.FirstOrDefault(p => p.Email == email && p.ResetCode == code);

            if (parent == null)
            {
                ViewBag.Error = "Invalid reset code.";
                ViewBag.TargetEmail = email;
                return View();
            }

            if (parent.ResetExpiry < DateTime.Now)
            {
                ViewBag.Error = "Reset code has expired.";
                ViewBag.TargetEmail = email;
                return View();
            }

            try
            {
                parent.Password = newPassword;
                parent.ResetCode = null;
                parent.ResetExpiry = null;

                // Update only these fields
                db.Entry(parent).Property(x => x.Password).IsModified = true;
                db.Entry(parent).Property(x => x.ResetCode).IsModified = true;
                db.Entry(parent).Property(x => x.ResetExpiry).IsModified = true;

                // Disable full model validation
                db.Configuration.ValidateOnSaveEnabled = false;

                db.SaveChanges();

                db.Configuration.ValidateOnSaveEnabled = true;

                TempData["Success"] = "Password reset successful.";
                return RedirectToAction("Login", "Teachers");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                ViewBag.TargetEmail = email;
                return View();
            }
        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "Teachers");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}