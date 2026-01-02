using System;
using System.Collections.Generic;

namespace ASQL_Online_Exam_.Models
{
    public partial class Course
    {
        public Course()
        {
            ExamDetails = new HashSet<ExamDetail>();
            Questions = new HashSet<Question>();
            Teaches = new HashSet<Teach>();
            Topics = new HashSet<Topic>();
            Tracks = new HashSet<Track>();
        }

        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? CourseCode { get; set; }
        public string? CourseDiscription { get; set; }

        public virtual ICollection<ExamDetail> ExamDetails { get; set; }
        public virtual ICollection<Question> Questions { get; set; }
        public virtual ICollection<Teach> Teaches { get; set; }
        public virtual ICollection<Topic> Topics { get; set; }

        public virtual ICollection<Track> Tracks { get; set; }
    }
}
