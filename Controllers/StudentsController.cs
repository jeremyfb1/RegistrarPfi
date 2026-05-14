using DAL;
using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using static Controllers.AccessControl;

namespace Controllers
{
    public class StudentsController : Controller
    {


        private void InitSessionVariables()
        {
            if (Session["CurrentStudentId"] == null) Session["CurrentStudentId"] = 0;
            if (Session["Search"] == null) Session["Search"] = false;
            if (Session["SearchString"] == null) Session["SearchString"] = "";
            if (Session["SelectedYear"] == null) Session["SelectedYear"] = "";

        }




        public ActionResult List()
        {
            InitSessionVariables();
            return View();
        }
        public ActionResult GetStudents(bool forceRefresh = false)
        {
            try
            {
                InitSessionVariables();
                bool searchActive = Session["Search"] != null ? (bool)Session["Search"] : false;
                string searchString = Session["SearchString"]?.ToString() ?? "";

                string selectedCategory = Session["SelectedYear"]?.ToString() ?? "";

                bool searchChanged = Session["LastSearch"]?.ToString() != searchString;

                if (DB.Users.HasChanged || DB.Students.HasChanged || DB.Teachers.HasChanged || DB.Courses.HasChanged || forceRefresh || searchChanged)
                {
                    Session["LastSearch"] = searchString;
                    var students = DB.Students.ToList();

                    var yearsList = students.Select(s => s.Year).Distinct().OrderByDescending(y => y).ToList();
                    Session["StudentsYearsList"] = yearsList;

                    if (searchActive)
                    {
                        if (!string.IsNullOrEmpty(searchString))
                        {
                            searchString = searchString.ToLower();
                            students = students.Where(s => s.FullName.ToLower().Contains(searchString) || s.Code.ToLower().Contains(searchString)).ToList();
                        }

                        if (!string.IsNullOrEmpty(selectedCategory))
                        {
                            int yearFilter = int.Parse(selectedCategory);
                            students = students.Where(s => s.Year == yearFilter).ToList();
                        }
                    }

                    ViewBag.Search = searchString;
                    return PartialView(students);
                }

                return Content("");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return Content("Erreur interne " + ex.Message);
            }
        }

        public ActionResult GetStudentDetails(bool forceRefresh = false)
        {
            try
            {
                InitSessionVariables();
                int studentId = (int)Session["CurrentStudentId"];
                Student student = DB.Students.Get(studentId);
                if (DB.Users.HasChanged || DB.Students.HasChanged || DB.Teachers.HasChanged || DB.Courses.HasChanged || forceRefresh)
                {
                    if (student != null)
                    {
                        return PartialView(student);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return Content("Erreur interne " + ex.Message);
            }
        }
        public ActionResult Details(int id)
        {
            InitSessionVariables();
            ViewBag.PageTitle = "Étudiant - Détails";
            Session["CurrentStudentId"] = id;
            Session["CurrentId"] = id;
            return View();
        }


        [UserAccess(Access.Write)]
        public ActionResult Edit()
        {
            int id = Session["CurrentStudentId"] != null ? (int)Session["CurrentStudentId"] : 0;
            if (id == 0) return RedirectToAction("List");

            Student student = DB.Students.Get(id);

            if (student != null)
            {

                Session["code"] = student.Code;
                ViewBag.Registrations = student.NextSessionCoursesToSelectList;
                var nextSessionCourses = DB.Courses.ToList()
                    .Where(c => Models.NextSession.ValidSessions.Contains(c.Session))
                    .OrderBy(c => c.Session)
                    .ToList();

                ViewBag.Courses = SelectListUtilities<Course>.Convert(nextSessionCourses, "Caption");

                return View(student);
            }

            return RedirectToAction("List");
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        [UserAccess(Access.Write)]
        public ActionResult Edit(Student student, List<int> selectedCoursesId)
        {

            student.Id = (int)Session["CurrentStudentId"];
            student.Code = (string)Session["code"];
            if (student.IsValid())
            {
                DB.Students.Update(student);
                student.UpdateRegistrations(selectedCoursesId);

                return RedirectToAction("Details", new { id = student.Id });
            }

            return Redirect("/Accounts/Login?message=Accès illégal! &success=false");
        }
        [UserAccess(Models.Access.Write)]

        public ActionResult Create()
        {
            return View(new Student());
        }
        [HttpPost]
        /* Install anti forgery token verification attribute.
         * the goal is to prevent submission of data from a page 
         * that has not been produced by this application*/
        [ValidateAntiForgeryToken()]
        [UserAccess(Models.Access.Write)]
        public ActionResult Create(Student student)
        {
            if (ModelState.IsValid)
            {
                student.genererCode();
                DB.Students.Add(student);
                return RedirectToAction("List");
            }

            ViewBag.PageTitle = "Étudiant - Ajout";
            return View(student);
        }


        [UserAccess(Models.Access.Write)]
        public ActionResult Delete(int id)
        {
            Student student = DB.Students.Get(id);
            student.DeleteAllRegistrations();
            student.DeleteNextSessionRegistrations();
            DB.Students.Delete(id);

            return RedirectToAction("List");
        }

        public ActionResult SetYear()
        {
            ViewBag.PageTitle = "Session courante";
            ViewBag.Year = NextSession.Year;
            ViewBag.Session = NextSession.ValidSessions.Contains(1) ? "Automne" : "Hiver";
            return View();
        }

        [HttpPost]
        public ActionResult SetYear(int year, string session)
        {
            NextSession.CurrentDate = new DateTime(year, (session == "Automne" ? 8 : 1), 15);
            return RedirectToAction("List");
        }
        public ActionResult ToggleSearch()
        {
            InitSessionVariables();
            ResetMediasPaging();
            if (Session["Search"] == null) Session["Search"] = false;
            Session["Search"] = !(bool)Session["Search"];
            return RedirectToAction("List");
        }

        private void ResetMediasPaging()
        {
            Session["pageNum"] = 1;
            Session["EndOfMedias"] = false;
        }
        public ActionResult SetSearchString(string value)
        {
            ResetMediasPaging();
            Session["SearchString"] = value.ToLower();
            return RedirectToAction("List");
        }
        public ActionResult GetStudentsCategoriesList()
        {
            var years = Session["StudentsYearsList"] as List<int> ?? new List<int>();
            return PartialView(years);
        }

        public ActionResult SetSearchCategory(string value)
        {
            Session["SelectedYear"] = value;
            return RedirectToAction("List");
        }
    }
}