using DAL;
using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static Controllers.AccessControl;

namespace Controllers
{
    public class CoursesController : Controller
    {
        private void InitSessionVariables()
        {
            if (Session["CurrentCourseId"] == null)  Session["CurrentCourseId"] = 0;
            
        }



        public ActionResult List()
        {
            return View();
        }
        public ActionResult GetCourses(bool forceRefresh = false, string searchString = "")
        {
            try
            {
                bool searchChanged = Session["LastSearch"]?.ToString() != searchString;

                if (DB.Users.HasChanged || DB.Students.HasChanged || DB.Teachers.HasChanged || DB.Courses.HasChanged || forceRefresh || searchChanged)
                {
                    Session["LastSearch"] = searchString;
                    var courses = DB.Courses.ToList();

                    if (!string.IsNullOrEmpty(searchString))
                    {
                        searchString = searchString.ToLower();
                        courses = courses.Where(c => c.Title.ToLower().Contains(searchString) || c.Code.ToLower().Contains(searchString)).ToList();
                    }

                    var sessionsList = courses.Select(c => c.Session).Distinct().ToList();
                    Session["CoursesSessionsList"] = sessionsList;
                    ViewBag.Search = searchString;

                    return PartialView(courses);
                }

                return Content("");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return Content("Erreur interne " + ex.Message);
            }

        }

        public ActionResult Details(int id)
        {
            InitSessionVariables();
            ViewBag.PageTitle = "Cours - Détails";
            Session["CurrentCourseId"] = id;
            Session["CurrentId"] = id;
            return View();
        }

        public ActionResult GetCourseDetails(bool forceRefresh = false)
        {
            try
            {
                InitSessionVariables();
                int courseId = (int)Session["CurrentCourseId"];
                Course course = DB.Courses.Get(courseId);
                if (DB.Users.HasChanged || DB.Students.HasChanged || DB.Teachers.HasChanged || DB.Courses.HasChanged || forceRefresh)
                {
                    if (course != null)
                    {
                        return PartialView(course);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return Content("Erreur interne " + ex.Message);
            }
        }


        [UserAccess(Access.Write)]
        public ActionResult Edit()
        {
            int id = Session["CurrentCourseId"] != null ? (int)Session["CurrentCourseId"] : 0;
            if (id == 0) return RedirectToAction("List");

            Course course = DB.Courses.Get(id);

            if (course != null)
            {

                Session["code"] = course.Code;
                ViewBag.Registrations = course.NextSessionStudentsSelectList;
                var nextSessionStudents = DB.Students.ToList()
                    .OrderBy(s => s.Code)
                    .ToList();

                ViewBag.Students = SelectListUtilities<Student>.Convert(nextSessionStudents, "Caption");

                return View(course);
            }

            return RedirectToAction("List");
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        [UserAccess(Access.Write)]
        public ActionResult Edit(Course course, List<int> selectedStudentsId)
        {

            course.Id = (int)Session["CurrentCourseId"];
            if (course.IsValid())
            {
                DB.Courses.Update(course);
                course.UpdateRegistrations(selectedStudentsId);

                return RedirectToAction("Details", new { id = course.Id });
            }

            return Redirect("/Accounts/Login?message=Accès illégal! &success=false");
        }

        public ActionResult Create()
        {
            return View(new Course());
        }
        [HttpPost]
        /* Install anti forgery token verification attribute.
         * the goal is to prevent submission of data from a page 
         * that has not been produced by this application*/
        [ValidateAntiForgeryToken()]
        [UserAccess(Models.Access.Write)]
        public ActionResult Create(Course course)
        {
            if (ModelState.IsValid)
            {
                DB.Courses.Add(course);
                return RedirectToAction("List");
            }

            ViewBag.PageTitle = "Cours - Ajout";
            return View(course);
        }


        [UserAccess(Models.Access.Write)]
        public ActionResult Delete(int id)
        {
            Course course = DB.Courses.Get(id);
            course.DeleteAllRegistrations();
            course.DeleteNextSessionRegistrations();
            course.DeleteAllAllocations();
            course.DeleteNextSessionAllocations();
            DB.Courses.Delete(id);

            return RedirectToAction("List");
        }
    }
}
