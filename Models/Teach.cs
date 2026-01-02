using System;
using System.Collections.Generic;

namespace ASQL_Online_Exam_.Models
{
    public partial class Teach
    {
        public int InstructorId { get; set; }
        public int CourseId { get; set; }
        public DateTime TeachingYear { get; set; }

        public virtual Course Course { get; set; } = null!;
        public virtual Instructor Instructor { get; set; } = null!;
    }
}
