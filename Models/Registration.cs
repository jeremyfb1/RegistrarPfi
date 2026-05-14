using DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Models
{
    public class Registration : Record
    {
        public Registration()
        {
            Year = NextSession.Year;
        }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public int Year { get; set; }
        [JsonIgnore]
        public Course Course => DB.Courses.Get(CourseId);
        [JsonIgnore]
        public Student Student => DB.Students.Get(StudentId);
        [JsonIgnore]
        public bool IsNextSession => Year == NextSession.Year && NextSession.ValidSessions.Contains(Course.Session);
    }
}