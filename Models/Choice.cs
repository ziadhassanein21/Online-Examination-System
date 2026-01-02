using System;
using System.Collections.Generic;

namespace ASQL_Online_Exam_.Models
{
    public partial class Choice
    {
        public Choice()
        {
            ExamQuestionsPools = new HashSet<ExamQuestionsPool>();
            ModelAnswers = new HashSet<ModelAnswer>();
        }

        public int ChoiceId { get; set; }
        public string ChoiceText { get; set; } = null!;
        public int? QuestionId { get; set; }

        public virtual Question? Question { get; set; }
        public virtual ICollection<ExamQuestionsPool> ExamQuestionsPools { get; set; }
        public virtual ICollection<ModelAnswer> ModelAnswers { get; set; }
    }
}
