using System;
using System.Collections.Generic;

namespace ASQL_Online_Exam_.Models
{
    public partial class ExamDetail
    {
        public ExamDetail()
        {
            ExamQuestionsPools = new HashSet<ExamQuestionsPool>();
            StudentExams = new HashSet<StudentExam>();
        }

        public int ExamId { get; set; }
        public string? ExamTitle { get; set; }
        public int? ExamDuration { get; set; }
        public DateTime? ExamDate { get; set; }
        public string? ExamDescription { get; set; }
        public int? CourseId { get; set; }

        public virtual Course? Course { get; set; }
        public virtual ICollection<ExamQuestionsPool> ExamQuestionsPools { get; set; }
        public virtual ICollection<StudentExam> StudentExams { get; set; }
    }
}
