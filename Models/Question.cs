using System;
using System.Collections.Generic;

namespace ASQL_Online_Exam_.Models
{
    public partial class Question
    {
        public Question()
        {
            Choices = new HashSet<Choice>();
            ExamQuestionsPools = new HashSet<ExamQuestionsPool>();
        }

        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = null!;
        public int? QuestionGrade { get; set; }
        public bool? QuestionType { get; set; }
        public int? CourseId { get; set; }

        public virtual Course? Course { get; set; }
        public virtual ModelAnswer? ModelAnswer { get; set; }
        public virtual ICollection<Choice> Choices { get; set; }
        public virtual ICollection<ExamQuestionsPool> ExamQuestionsPools { get; set; }
    }
}
