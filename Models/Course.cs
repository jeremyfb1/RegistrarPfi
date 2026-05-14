using DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Models
{
    public class Course : Record
    {
        public string Code { get; set; }
        public string Title { get; set; }
        public int Session { get; set; }
        [JsonIgnore]
        public string Caption => $"[{Session}] {Code} {Title}";

        [JsonIgnore] public List<Registration> Registrations => DB.Registrations.ToList().Where(r => r.CourseId == Id).ToList();
        [JsonIgnore] public List<Registration> NextSessionRegistrations => DB.Registrations.ToList().Where(r => r.CourseId == Id && r.IsNextSession).ToList();

        [JsonIgnore] public List<Allocation> Allocations => DB.Allocations.ToList().Where(a => a.CourseId == Id).ToList();
        [JsonIgnore] public List<Allocation> NextSessionAllocations => DB.Allocations.ToList().Where(a => a.CourseId == Id && a.IsNextSession).ToList();



        [JsonIgnore] 
        public List<Student> Students
        {
            get
            {
                var students = new List<Student>();
                foreach (var registration in Registrations.OrderBy(r => r.Student.Code))
                {
                    students.Add(registration.Student);
                }
                return students;
            }
        }
        [JsonIgnore]
        public List<Student> NextSessionStudents
        {
                get
                {
                    var students = new List<Student>();
                    foreach (var registration in NextSessionRegistrations.OrderBy(r => r.Student.Code))
                    {
                        students.Add(registration.Student);
                    }
                    return students;
                }
        }

        [JsonIgnore] public SelectList StudentsSelectList => SelectListUtilities<Student>.Convert(Students, "Caption");
        [JsonIgnore] public SelectList NextSessionStudentsSelectList => SelectListUtilities<Student>.Convert(NextSessionStudents, "Caption");

        [JsonIgnore]
        public List<Teacher> Teachers
        {
            get
            {
                var teachers = new List<Teacher>();
                foreach (var allocation in Allocations.OrderBy(a => a.Teacher.LastName))
                {
                    teachers.Add(allocation.Teacher);
                }
                return teachers;
            }
        }

        [JsonIgnore]
        public List<Teacher> NextSessionTeachers
        {
            get
            {
                var teachers = new List<Teacher>();
                foreach (var allocation in NextSessionAllocations.OrderBy(a => a.Teacher.LastName))
                {
                    teachers.Add(allocation.Teacher);
                }
                return teachers;
            }
        }

        [JsonIgnore] public SelectList TeachersSelectList => SelectListUtilities<Teacher>.Convert(Teachers, "Caption");
        [JsonIgnore] public SelectList NextSessionTeachersSelectList => SelectListUtilities<Teacher>.Convert(NextSessionTeachers, "Caption");

        public void DeleteAllRegistrations()
        {
            foreach (Registration registration in Registrations)
                DB.Registrations.Delete(registration.Id);
        }
        public void DeleteNextSessionRegistrations()
        {
            foreach (Registration registration in NextSessionRegistrations)
                DB.Registrations.Delete(registration.Id);
        }
        public void UpdateRegistrations(List<int> selectedStudentsId)
        {
            DeleteNextSessionRegistrations();
            if (selectedStudentsId != null)
                foreach (int studentId in selectedStudentsId)
                {
                    DB.Registrations.Add(new Registration { StudentId = studentId, CourseId = Id });
                }
        }


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
        public void UpdateAllocations(List<int> selectedTeachersId)
        {
            DeleteNextSessionAllocations();
            if (selectedTeachersId != null)
                foreach (int teacherId in selectedTeachersId)
                {
                    DB.Allocations.Add(new Allocation { TeacherId = teacherId, CourseId = Id });
                }
        }


    }
}