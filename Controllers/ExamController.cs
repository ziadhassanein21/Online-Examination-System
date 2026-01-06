using System.Data;
using ASQL_Online_Exam_.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ASQL_Online_Exam_.Controllers
{
    public class ExamController : Controller
    {
        private readonly AppDbContext _context;

        public ExamController(AppDbContext context)
        {
            _context = context;
        }

        #region Actions

        [HttpGet]
        public IActionResult Start(int? examId)
        {
            var studentId = GetStudentId();
            if (studentId == null) return RedirectToLogin();
            if (!examId.HasValue) return RedirectToAction("Index", "Home");

            var viewModel = LoadExamForStudent(studentId.Value, examId.Value);
            viewModel.StudentName = GetStudentName(studentId.Value);

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Submit(int examId, Dictionary<int, int> answers)
        {
            var studentId = GetStudentId();
            if (studentId == null) return RedirectToLogin();

            var result = ProcessExamSubmission(studentId.Value, examId, answers);

            // Pass result to view using ViewBag (compatible with existing Result.cshtml)
            TempData["ExamGrade"] = result.Grade;
            TempData["ExamTitle"] = result.ExamTitle;

            return RedirectToAction("Result");
        }

        [HttpGet]
        public IActionResult Result()
        {
            var studentId = GetStudentId();
            if (studentId == null) return RedirectToLogin();

            var grade = TempData["ExamGrade"] as int?;
            var examTitle = TempData["ExamTitle"] as string;

            if (grade == null) return RedirectToAction("MyExams", "Home");

            ViewBag.Grade = grade;
            ViewBag.ExamTitle = examTitle;
            ViewBag.StudentName = GetStudentName(studentId.Value);

            return View();
        }

        #endregion

        #region Private Helpers

        private int? GetStudentId() => HttpContext.Session.GetInt32("StudentId");

        private IActionResult RedirectToLogin() => RedirectToAction("Login", "Login");

        private string GetStudentName(int studentId)
        {
            return _context.Students
                .FirstOrDefault(s => s.StudentId == studentId)?.StudentName ?? "Student";
        }

        private ExamViewModel LoadExamForStudent(int studentId, int examId)
        {
            var viewModel = new ExamViewModel();
            var questions = new List<ExamQuestionViewModel>();

            using var connection = new SqlConnection(_context.Database.GetConnectionString());
            connection.Open();

            // Load exam details and questions from GetStudentExam SP
            LoadExamData(connection, studentId, examId, viewModel, questions);

            viewModel.Questions = questions;
            return viewModel;
        }

        private void LoadExamData(SqlConnection connection, int studentId, int examId,
            ExamViewModel viewModel, List<ExamQuestionViewModel> questions)
        {
            using var cmd = new SqlCommand("dbo.GetStudentExam", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@studentId", studentId);
            cmd.Parameters.AddWithValue("@examId", examId);

            using var reader = cmd.ExecuteReader();

            // Result set 1: Exam details
            if (reader.Read())
            {
                viewModel.ExamId = reader.GetInt32(reader.GetOrdinal("examId"));
                viewModel.ExamTitle = GetSafeString(reader, "examTitle") ?? "";
                viewModel.ExamDescription = GetSafeString(reader, "examDescription") ?? "";
                viewModel.ExamDuration = GetSafeInt(reader, "examDuration") ?? 60;
                viewModel.ExamDate = GetSafeDateTime(reader, "examDate") ?? DateTime.Now;
                viewModel.CourseName = GetSafeString(reader, "courseName") ?? "";
            }

            // Result set 2: Questions
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    questions.Add(new ExamQuestionViewModel
                    {
                        QuestionId = reader.GetInt32(reader.GetOrdinal("questionId")),
                        QuestionText = GetSafeString(reader, "questionText") ?? "",
                        QuestionType = GetSafeIntConverted(reader, "questionType"),
                        QuestionGrade = GetSafeIntConverted(reader, "questionGrade"),
                        Choices = new List<ChoiceViewModel>()
                    });
                }
            }

            // Result set 3: Choices
            if (reader.NextResult())
            {
                while (reader.Read())
                {
                    var questionId = reader.GetInt32(reader.GetOrdinal("questionId"));
                    var question = questions.FirstOrDefault(q => q.QuestionId == questionId);

                    question?.Choices.Add(new ChoiceViewModel
                    {
                        ChoiceId = reader.GetInt32(reader.GetOrdinal("choiceId")),
                        ChoiceText = GetSafeString(reader, "choiceText") ?? ""
                    });
                }
            }
        }

        private (int Grade, string ExamTitle) ProcessExamSubmission(int studentId, int examId, Dictionary<int, int> answers)
        {
            using var connection = new SqlConnection(_context.Database.GetConnectionString());
            connection.Open();

            // Step 1: Save answers
            SaveAnswers(connection, examId, studentId, answers);

            // Step 2: Calculate grade
            CalculateGrade(connection, studentId, examId);

            // Step 3: Get result
            return GetExamResult(connection, studentId, examId);
        }

        private void SaveAnswers(SqlConnection connection, int examId, int studentId, Dictionary<int, int> answers)
        {
            var answersTable = new DataTable();
            answersTable.Columns.Add("QuestionID", typeof(int));
            answersTable.Columns.Add("ChoiceAnswerID", typeof(int));

            foreach (var answer in answers)
            {
                answersTable.Rows.Add(answer.Key, answer.Value);
            }

            using var cmd = new SqlCommand("dbo.InsertStudentExamAnswers", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ExamID", examId);
            cmd.Parameters.AddWithValue("@StudentID", studentId);

            var tvpParam = cmd.Parameters.AddWithValue("@Answers", answersTable);
            tvpParam.SqlDbType = SqlDbType.Structured;
            tvpParam.TypeName = "dbo.AnswerList";

            cmd.ExecuteNonQuery();
        }

        private void CalculateGrade(SqlConnection connection, int studentId, int examId)
        {
            using var cmd = new SqlCommand("dbo.usp_CalculateExamGrade", connection);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@studentId", studentId);
            cmd.Parameters.AddWithValue("@examId", examId);
            cmd.ExecuteNonQuery();
        }

        private (int Grade, string ExamTitle) GetExamResult(SqlConnection connection, int studentId, int examId)
        {
            const string sql = @"
                SELECT se.grade, e.examTitle 
                FROM studentExam se 
                JOIN examDetails e ON se.examId = e.examId 
                WHERE se.studentId = @studentId AND se.examId = @examId";

            using var cmd = new SqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@studentId", studentId);
            cmd.Parameters.AddWithValue("@examId", examId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var grade = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));
                var title = reader.IsDBNull(1) ? "" : reader.GetString(1);
                return (grade, title);
            }

            return (0, "");
        }

        #endregion

        #region Data Reader Helpers

        private static string? GetSafeString(SqlDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
            }
            catch { return null; }
        }

        private static int? GetSafeInt(SqlDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
            }
            catch { return null; }
        }

        private static int GetSafeIntConverted(SqlDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? 0 : Convert.ToInt32(reader.GetValue(ordinal));
            }
            catch { return 0; }
        }

        private static DateTime? GetSafeDateTime(SqlDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
            }
            catch { return null; }
        }

        #endregion
    }
}
