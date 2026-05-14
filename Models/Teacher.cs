using DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Models
{
    public class Teacher : Record
    {

        public string FirstName { get; set; }

        public string LastName { get; set; }
        public string Code { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public DateTime StartDate { get; set; }

        const string Avatars_Folder = @"/App_Assets/Teachers/";
        const string Default_Avatar = @"no_avatar.png";

        [ImageAsset(Avatars_Folder, Default_Avatar)]
        public string Avatar { get; set; } = Avatars_Folder + Default_Avatar;




        [JsonIgnore] public string FullName => LastName + " " + FirstName;

        [JsonIgnore] public List<Allocation> Allocations => DB.Allocations.ToList().Where(r => r.TeacherId == Id).ToList();
        [JsonIgnore] public List<Allocation> NextSessionAllocations => DB.Allocations.ToList().Where(r => r.TeacherId == Id && r.IsNextSession).ToList();
        [JsonIgnore]
        public List<Course> Courses
        {
            get
            {
                var courses = new List<Course>();
                foreach (var allocation in Allocations.OrderBy(a => a.Course.Code))
                {
                    courses.Add(allocation.Course);
                }
                return courses;
            }
        }
        [JsonIgnore]
        public List<Course> NextSessionCourses
        {
            get
            {
                var courses = new List<Course>();
                foreach (var allocation in NextSessionAllocations.OrderBy(a => a.Course.Code))
                {
                    courses.Add(allocation.Course);
                }
                return courses;
            }
        }
        [JsonIgnore] public SelectList CoursesSelectList => SelectListUtilities<Course>.Convert(Courses, "Caption");

        [JsonIgnore]
        public SelectList NextSessionCoursesToSelectList => SelectListUtilities<Course>.Convert(NextSessionCourses, "Caption");

        public void DeleteAllAllocations()
        {
            foreach (Allocation allocation in Allocations)
                DB.Allocations.Delete(allocation.Id);
        }
        public void DeleteNextSessionAllocations()
        {
            foreach (Allocation allocation in NextSessionAllocations)
                DB.Allocations.Delete(allocation.Id);
        }
        public void UpdateAllocations(List<int> selectedCoursesId)
        {
            DeleteNextSessionAllocations();
            if (selectedCoursesId != null)
                foreach (int courseId in selectedCoursesId)
                {
                    DB.Allocations.Add(new Allocation { TeacherId = Id, CourseId = courseId });
                }
        }

        public void genererCode()
        {
            Random random = new Random();
            bool unique = false;
            string nouveauCode;
            do
            {
                nouveauCode = "CLG-420-" + random.Next(10000, 99999).ToString();
                unique = !DB.Teachers.ToList().Any(t => t.Code == nouveauCode);
            } while (!unique);
            this.Code = nouveauCode;
        }
    }
}