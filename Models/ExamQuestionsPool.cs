using System;
using System.Collections.Generic;

namespace ASQL_Online_Exam_.Models
{
    public partial class ExamQuestionsPool
    {
        public int ExamQuestionsPool1 { get; set; }
        public int ExamId { get; set; }
        public int QuestionId { get; set; }
        public int? ChoiceAnswerId { get; set; }
        public int StudentId { get; set; }

        public virtual Choice? ChoiceAnswer { get; set; }
        public virtual ExamDetail Exam { get; set; } = null!;
        public virtual Question Question { get; set; } = null!;
        public virtual Student Student { get; set; } = null!;
    }
}
