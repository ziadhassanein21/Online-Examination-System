using System;
using System.Collections.Generic;

namespace ASQL_Online_Exam_.Models
{
    public partial class Branch
    {
        public Branch()
        {
            Instructors = new HashSet<Instructor>();
            Tracks = new HashSet<Track>();
        }

        public int BranchId { get; set; }
        public string BranchName { get; set; } = null!;
        public string BranchLocation { get; set; } = null!;

        public virtual ICollection<Instructor> Instructors { get; set; }

        public virtual ICollection<Track> Tracks { get; set; }
    }
}
