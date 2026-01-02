using System;
using System.Collections.Generic;

namespace ASQL_Online_Exam_.Models
{
    public partial class StudentExam
    {
        public int StudentId { get; set; }
        public int ExamId { get; set; }
        public int? Grade { get; set; }

        public virtual ExamDetail Exam { get; set; } = null!;
        public virtual Student Student { get; set; } = null!;
    }
}
