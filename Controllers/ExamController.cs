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

        [HttpGet]
        public IActionResult Start(int? examId)
        {
            if (HttpContext.Session.GetInt32("StudentId") == null)
            {
                return RedirectToAction("Login", "Login");
            }

            if (!examId.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            var studentId = HttpContext.Session.GetInt32("StudentId")!.Value;

            var viewModel = new ExamViewModel();
            var questions = new List<ExamQuestionViewModel>();

            var connection = _context.Database.GetDbConnection();
            connection.Open();

            try
            {
                // First, generate exam questions for the student if not already generated
                using (var generateCmd = connection.CreateCommand())
                {
                    generateCmd.CommandText = "dbo.usp_GenerateExamForStudent";
                    generateCmd.CommandType = CommandType.StoredProcedure;

                    // @studentId first, then @examId (matching SP signature)
                    var genStudentParam = generateCmd.CreateParameter();
                    genStudentParam.ParameterName = "@studentId";
                    genStudentParam.Value = studentId;
                    generateCmd.Parameters.Add(genStudentParam);

                    var genExamParam = generateCmd.CreateParameter();
                    genExamParam.ParameterName = "@examId";
                    genExamParam.Value = examId.Value;
                    generateCmd.Parameters.Add(genExamParam);

                    try
                    {
                        generateCmd.ExecuteNonQuery();
                        viewModel.DebugMessage = "GenerateExam succeeded. ";
                    }
                    catch (Exception ex)
                    {
                        // Capture error but continue - might be "already generated"
                        viewModel.DebugMessage = $"GenerateExam: {ex.Message}. ";
                    }
                }

                // Now get the exam details and questions
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "dbo.GetStudentExam";
                    command.CommandType = CommandType.StoredProcedure;

                    var studentParam = command.CreateParameter();
                    studentParam.ParameterName = "@studentId";
                    studentParam.Value = studentId;
                    command.Parameters.Add(studentParam);

                    var examParam = command.CreateParameter();
                    examParam.ParameterName = "@examId";
                    examParam.Value = examId.Value;
                    command.Parameters.Add(examParam);

                    using (var reader = command.ExecuteReader())
                    {
                        // First result set: Exam details
                        if (reader.Read())
                        {
                            viewModel.ExamId = reader.GetInt32(reader.GetOrdinal("examId"));
                            viewModel.ExamTitle = reader.IsDBNull(reader.GetOrdinal("examTitle")) ? "" : reader.GetString(reader.GetOrdinal("examTitle"));
                            viewModel.ExamDescription = reader.IsDBNull(reader.GetOrdinal("examDescription")) ? "" : reader.GetString(reader.GetOrdinal("examDescription"));
                            viewModel.ExamDuration = reader.IsDBNull(reader.GetOrdinal("examDuration")) ? 60 : reader.GetInt32(reader.GetOrdinal("examDuration"));
                            viewModel.ExamDate = reader.IsDBNull(reader.GetOrdinal("examDate")) ? DateTime.Now : reader.GetDateTime(reader.GetOrdinal("examDate"));
                            viewModel.CourseName = reader.IsDBNull(reader.GetOrdinal("courseName")) ? "" : reader.GetString(reader.GetOrdinal("courseName"));
                            viewModel.DebugMessage += $"Got exam: {viewModel.ExamTitle}. ";
                        }
                        else
                        {
                            viewModel.DebugMessage += "No exam details found. ";
                        }

                        // Second result set: Questions
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                // Use GetValue + Convert to handle both BIT and INT types
                                var questionTypeOrdinal = reader.GetOrdinal("questionType");
                                var questionGradeOrdinal = reader.GetOrdinal("questionGrade");
                                
                                questions.Add(new ExamQuestionViewModel
                                {
                                    QuestionId = reader.GetInt32(reader.GetOrdinal("questionId")),
                                    QuestionText = reader.IsDBNull(reader.GetOrdinal("questionText")) ? "" : reader.GetString(reader.GetOrdinal("questionText")),
                                    QuestionType = reader.IsDBNull(questionTypeOrdinal) ? 0 : Convert.ToInt32(reader.GetValue(questionTypeOrdinal)),
                                    QuestionGrade = reader.IsDBNull(questionGradeOrdinal) ? 0 : Convert.ToInt32(reader.GetValue(questionGradeOrdinal)),
                                    Choices = new List<ChoiceViewModel>()
                                });
                            }
                            viewModel.DebugMessage += $"Found {questions.Count} questions. ";
                        }
                        else
                        {
                            viewModel.DebugMessage += "No questions result set. ";
                        }

                        // Third result set: Choices
                        if (reader.NextResult())
                        {
                            int choiceCount = 0;
                            while (reader.Read())
                            {
                                var questionId = reader.GetInt32(reader.GetOrdinal("questionId"));
                                var question = questions.FirstOrDefault(q => q.QuestionId == questionId);
                                
                                if (question != null)
                                {
                                    question.Choices.Add(new ChoiceViewModel
                                    {
                                        ChoiceId = reader.GetInt32(reader.GetOrdinal("choiceId")),
                                        ChoiceText = reader.IsDBNull(reader.GetOrdinal("choiceText")) ? "" : reader.GetString(reader.GetOrdinal("choiceText"))
                                    });
                                    choiceCount++;
                                }
                            }
                            viewModel.DebugMessage += $"Found {choiceCount} choices. ";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                viewModel.DebugMessage += $"Error: {ex.Message}";
            }
            finally
            {
                connection.Close();
            }

            viewModel.Questions = questions;
            viewModel.DebugMessage += $"StudentId={studentId}, ExamId={examId}";

            // Get student name
            var student = _context.Students.FirstOrDefault(s => s.StudentId == studentId);
            viewModel.StudentName = student?.StudentName ?? "Student";

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Submit(int examId, Dictionary<int, int> answers)
        {
            if (HttpContext.Session.GetInt32("StudentId") == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var studentId = HttpContext.Session.GetInt32("StudentId")!.Value;
            int? grade = null;
            string examTitle = "";

            // Create connection string from DbContext
            var connectionString = _context.Database.GetConnectionString();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Create the DataTable for the AnswerList TVP
                var answersTable = new DataTable();
                answersTable.Columns.Add("QuestionID", typeof(int));
                answersTable.Columns.Add("ChoiceAnswerID", typeof(int));

                foreach (var answer in answers)
                {
                    answersTable.Rows.Add(answer.Key, answer.Value);
                }

                // Step 1: Save answers
                using (var command = new SqlCommand("dbo.InsertStudentExamAnswers", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@ExamID", examId);
                    command.Parameters.AddWithValue("@StudentID", studentId);

                    var tvpParam = command.Parameters.AddWithValue("@Answers", answersTable);
                    tvpParam.SqlDbType = SqlDbType.Structured;
                    tvpParam.TypeName = "dbo.AnswerList";

                    command.ExecuteNonQuery();
                }

                // Step 2: Calculate grade using usp_CalculateExamGrade
                using (var gradeCmd = new SqlCommand("dbo.usp_CalculateExamGrade", connection))
                {
                    gradeCmd.CommandType = CommandType.StoredProcedure;

                    gradeCmd.Parameters.AddWithValue("@studentId", studentId);
                    gradeCmd.Parameters.AddWithValue("@examId", examId);

                    gradeCmd.ExecuteNonQuery();
                }

                // Step 3: Get the calculated grade from studentExam table
                using (var getGradeCmd = new SqlCommand(
                    "SELECT se.grade, e.examTitle FROM studentExam se " +
                    "JOIN examDetails e ON se.examId = e.examId " +
                    "WHERE se.studentId = @studentId AND se.examId = @examId", connection))
                {
                    getGradeCmd.Parameters.AddWithValue("@studentId", studentId);
                    getGradeCmd.Parameters.AddWithValue("@examId", examId);

                    using (var reader = getGradeCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            grade = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));
                            examTitle = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        }
                    }
                }
            }

            // Store result in TempData for the result page
            TempData["ExamGrade"] = grade;
            TempData["ExamTitle"] = examTitle;
            TempData["ExamId"] = examId;
            
            return RedirectToAction("Result");
        }

        [HttpGet]
        public IActionResult Result()
        {
            if (HttpContext.Session.GetInt32("StudentId") == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var grade = TempData["ExamGrade"] as int?;
            var examTitle = TempData["ExamTitle"] as string;

            if (grade == null)
            {
                return RedirectToAction("MyExams", "Home");
            }

            ViewBag.Grade = grade;
            ViewBag.ExamTitle = examTitle;
            ViewBag.StudentName = _context.Students
                .FirstOrDefault(s => s.StudentId == HttpContext.Session.GetInt32("StudentId"))?.StudentName;

            return View();
        }
    }

    // ViewModels for the exam
    public class ExamViewModel
    {
        public int ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string ExamDescription { get; set; } = string.Empty;
        public int ExamDuration { get; set; }
        public DateTime ExamDate { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string DebugMessage { get; set; } = string.Empty;
        public List<ExamQuestionViewModel> Questions { get; set; } = new();
    }

    public class ExamQuestionViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public int QuestionType { get; set; }
        public int QuestionGrade { get; set; }
        public List<ChoiceViewModel> Choices { get; set; } = new();
    }

    public class ChoiceViewModel
    {
        public int ChoiceId { get; set; }
        public string ChoiceText { get; set; } = string.Empty;
    }
}
