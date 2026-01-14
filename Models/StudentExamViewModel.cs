using System;

namespace ASQL_Online_Exam_.Models
{
    public class StudentExamViewModel
    {
        public int ExamId { get; set; }
        public string? ExamTitle { get; set; }
        public DateTime? ExamDate { get; set; }
        public int? ExamDuration { get; set; }
        public string? ExamDescription { get; set; }
        public int? CourseId { get; set; }
        public int? Grade { get; set; }
        public int? totalGrade { get; set; }
    }
}
