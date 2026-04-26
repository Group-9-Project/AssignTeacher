using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SchoolTimetable.Models;

namespace SchoolTimetable.Controllers
{
    public class Entrance_testController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        // GET: Entrance_test
        public ActionResult Index()
        {
            var model = db.Entrance_tests.ToList();
            return View(model);
        }

        // GET: Entrance_test/Create
        public ActionResult Create()
        {
            // Explicitly returning the Create view with a fresh model
            return View("Create", new Entrance_test());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            Entrance_test model,
            string CorrectAnswerOne, string CorrectAnswerTwo, string CorrectAnswerThree,
            string CorrectAnswerFour, string CorrectAnswerFive)
        {
            ModelState.Remove("StudentNumber");
            // 1. Initial Validation
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Helper to map radio selection (A, B, or C) to the actual text stored in the Answer fields
            string MapChoiceToAnswer(string choice, string aa, string bb, string cc)
            {
                if (string.IsNullOrEmpty(choice)) return null;

                switch (choice.ToUpper().Trim())
                {
                    case "A": return aa;
                    case "B": return bb;
                    case "C": return cc;
                    default: return null;
                }
            }

            // 2. Mapping the correct answers based on radio button selection
            model.CorrectAnswer1 = MapChoiceToAnswer(CorrectAnswerOne, model.AnswerOneAA, model.AnswerOneBB, model.AnswerOneCC);
            model.CorrectAnswer2 = MapChoiceToAnswer(CorrectAnswerTwo, model.AnswerTwoAA, model.AnswerTwoBB, model.AnswerTwoCC);
            model.CorrectAnswer3 = MapChoiceToAnswer(CorrectAnswerThree, model.AnswerThreeAA, model.AnswerThreeBB, model.AnswerThreeCC);
            model.CorrectAnswer4 = MapChoiceToAnswer(CorrectAnswerFour, model.AnswerFourAA, model.AnswerFourBB, model.AnswerFourCC);
            model.CorrectAnswer5 = MapChoiceToAnswer(CorrectAnswerFive, model.AnswerFiveAA, model.AnswerFiveBB, model.AnswerFiveCC);

            // 3. Ensure all mappings were successful (User selected an option for every question)
            if (model.CorrectAnswer1 == null || model.CorrectAnswer2 == null || model.CorrectAnswer3 == null ||
                model.CorrectAnswer4 == null || model.CorrectAnswer5 == null)
            {
                ModelState.AddModelError("", "Please select a correct option (A, B, or C) for all 5 questions before saving.");
                return View(model);
            }

            // 4. Initialize scoring fields
            model.counter = 0;
            model.TotalScore = 0;

            try
            {
                // 5. Persist to Database
                db.Entrance_tests.Add(model);
                db.SaveChanges();

                TempData["Success"] = $"Test '{model.Title}' created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Database error: " + ex.Message);
                return View(model);
            }
        }

     


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}