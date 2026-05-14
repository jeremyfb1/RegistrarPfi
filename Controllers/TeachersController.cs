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
    public class TeachersController : Controller
    {
        private void InitSessionVariables()
        {
            if (Session["CurrentTeacherId"] == null) Session["CurrentTeacherId"] = 0;
            if (Session["Search"] == null) Session["Search"] = false;
            if (Session["SearchString"] == null) Session["SearchString"] = "";
        }


        public ActionResult List()
        {
            InitSessionVariables();
            return View();
        }
        public ActionResult GetTeachers(bool forceRefresh = false)
        {
            try
            {

                bool searchActive = Session["Search"] != null ? (bool)Session["Search"] : false;
                string searchString = Session["SearchString"]?.ToString() ?? "";
                bool searchChanged = Session["LastSearch"]?.ToString() != searchString;


                if (DB.Users.HasChanged || DB.Teachers.HasChanged || forceRefresh || searchChanged)
                {
                    Session["LastSearch"] = searchString;
                    var teachers = DB.Teachers.ToList();

                    if (searchActive && !string.IsNullOrEmpty(searchString))
                    {
                        searchString = searchString.ToLower();
                        teachers = teachers.Where(t => t.FullName.ToLower().Contains(searchString) ||
                                                   t.Code.ToLower().Contains(searchString)).ToList();
                    }

                    ViewBag.Search = searchString;
                    return PartialView(teachers);
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
            ViewBag.PageTitle = "Prof - Détails";
            Session["CurrentTeacherId"] = id;
            Session["CurrentId"] = id;
            return View();
        }

        public ActionResult GetTeacherDetails(bool forceRefresh = false)
        {
            try
            {
                InitSessionVariables();
                int teacherId = (int)Session["CurrentTeacherId"];
                Teacher teacher = DB.Teachers.Get(teacherId);

                if (DB.Users.HasChanged || DB.Teachers.HasChanged ||
                    DB.Allocations.HasChanged || DB.Courses.HasChanged || forceRefresh)
                {
                    if (teacher != null)
                    {
                        return PartialView(teacher);
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
            int id = Session["CurrentTeacherId"] != null ? (int)Session["CurrentTeacherId"] : 0;
            if (id == 0) return RedirectToAction("List");

            Teacher teacher = DB.Teachers.Get(id);

            if (teacher != null)
            {
                Session["code"] = teacher.Code;

                var coursesAssignedToOthers = DB.Allocations.ToList()
                    .Where(a => a.TeacherId != teacher.Id)
                    .Select(a => a.CourseId)
                    .ToList();

                var nextSessionCourses = DB.Courses.ToList()
                    .Where(c => Models.NextSession.ValidSessions.Contains(c.Session))
                    .Where(c => !coursesAssignedToOthers.Contains(c.Id))
                    .OrderBy(c => c.Session)
                    .ToList();

                ViewBag.Allocations = teacher.NextSessionCoursesToSelectList; 
                ViewBag.Courses = SelectListUtilities<Course>.Convert(nextSessionCourses, "Caption");

                return View(teacher);
            }

            return RedirectToAction("List");
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        [UserAccess(Access.Write)]
        public ActionResult Edit(Teacher teacher, List<int> selectedCoursesId)
        {
            if (selectedCoursesId != null)
            {
                selectedCoursesId.RemoveAll(courseId =>
                    DB.Allocations.ToList().Any(a => a.CourseId == courseId && a.TeacherId != teacher.Id)
                );
            }
            else
            {
                selectedCoursesId = new List<int>();
            }

            teacher.Id = (int)Session["CurrentTeacherId"];
            teacher.Code = (string)Session["code"];

            if (teacher.IsValid())
            {
                DB.Teachers.Update(teacher);
                teacher.UpdateAllocations(selectedCoursesId);

                return RedirectToAction("Details", new { id = teacher.Id });
            }

            return Redirect("/Accounts/Login?message=Accès illégal! &success=false");
        }
        [UserAccess(Models.Access.Write)]

        public ActionResult Create()
        {
            return View(new Teacher());
        }
        [HttpPost]
        /* Install anti forgery token verification attribute.
         * the goal is to prevent submission of data from a page 
         * that has not been produced by this application*/
        [ValidateAntiForgeryToken()]
        [UserAccess(Models.Access.Write)]
        public ActionResult Create(Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                teacher.genererCode();
                DB.Teachers.Add(teacher);
                return RedirectToAction("List");
            }

            ViewBag.PageTitle = "Prof - Ajout";
            return View(teacher);
        }


        [UserAccess(Models.Access.Write)]
        public ActionResult Delete(int id)
        {
            Teacher teacher = DB.Teachers.Get(id);
            teacher.DeleteAllAllocations();
            teacher.DeleteNextSessionAllocations();
            DB.Teachers.Delete(id);

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
            return RedirectToAction("Index");
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
    }
}