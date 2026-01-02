using System;
using System.Collections.Generic;

namespace ASQL_Online_Exam_.Models
{
    public partial class Track
    {
        public Track()
        {
            Students = new HashSet<Student>();
            Branches = new HashSet<Branch>();
            Courses = new HashSet<Course>();
        }

        public int TrackId { get; set; }
        public string TrackName { get; set; } = null!;

        public virtual ICollection<Student> Students { get; set; }

        public virtual ICollection<Branch> Branches { get; set; }
        public virtual ICollection<Course> Courses { get; set; }
    }
}
