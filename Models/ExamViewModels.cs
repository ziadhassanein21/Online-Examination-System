namespace ASQL_Online_Exam_.Models
{
    /// <summary>
    /// ViewModel for the exam-taking page
    /// </summary>
    public class ExamViewModel
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string ExamDescription { get; set; } = string.Empty;
        public int ExamDuration { get; set; } = 60;
        public DateTime ExamDate { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string? DebugMessage { get; set; }
        public List<ExamQuestionViewModel> Questions { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for a question in an exam
    /// </summary>
    public class ExamQuestionViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int QuestionType { get; set; }
        public int QuestionGrade { get; set; }
        public List<ChoiceViewModel> Choices { get; set; } = new();
    }

    /// <summary>
    /// ViewModel for a choice/option in a question
    /// </summary>
    public class ChoiceViewModel
    {
        public int ChoiceId { get; set; }
        public string ChoiceText { get; set; } = string.Empty;
    }

    /// <summary>
    /// ViewModel for exam result display
    /// </summary>
    public class ExamResultViewModel
    {
        public int Grade { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public bool IsPassed => Grade >= 50;
        public string GradeClass => Grade >= 85 ? "excellent" : Grade >= 70 ? "good" : Grade >= 50 ? "pass" : "fail";
        public string GradeLabel => Grade >= 85 ? "Excellent!" : Grade >= 70 ? "Good Job!" : Grade >= 50 ? "Passed" : "Failed";
    }
}
