using System.Diagnostics;
using System.Data;
using ASQL_Online_Exam_.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ASQL_Online_Exam_.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext db;
        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            db = context;
        }
       

       

        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("StudentId") == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var studentId = HttpContext.Session.GetInt32("StudentId");
            var student = db.Students
                .Include(s => s.Track)
                    .ThenInclude(t => t.Branches)
                .Include(s => s.Track)
                    .ThenInclude(t => t.Courses)
                .FirstOrDefault(s => s.StudentId == studentId);

            var viewModel = new DashboardViewModel
            {
                StudentName = student?.StudentName ?? "Student",
                StudentEmail = student?.StudentEmail,
                StudentPhone = student?.StudentPhoneNumber,
                EnrollmentDate = student?.StudentEnrollementDate,
                TrackName = student?.Track?.TrackName,
                BranchName = student?.Track?.Branches.FirstOrDefault()?.BranchName,
                BranchLocation = student?.Track?.Branches.FirstOrDefault()?.BranchLocation,
                TotalCourses = student?.Track?.Courses.Count ?? 0
            };

            var connection = db.Database.GetDbConnection();
            connection.Open();

            try
            {
                // First, get completed exam IDs for this student
                var completedExamIds = new HashSet<int>();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT examId FROM studentExam WHERE studentId = @studentId";
                    var param = command.CreateParameter();
                    param.ParameterName = "@studentId";
                    param.Value = studentId;
                    command.Parameters.Add(param);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            completedExamIds.Add(reader.GetInt32(0));
                        }
                    }
                }

                // Get today's available exams (excluding completed)
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "usp_GetAvailableExamsForStudent";
                    command.CommandType = CommandType.StoredProcedure;

                    var param = command.CreateParameter();
                    param.ParameterName = "@studentId";
                    param.Value = studentId;
                    command.Parameters.Add(param);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var exam = ReadExamFromReader(reader);
                            // Only add if not completed
                            if (!completedExamIds.Contains(exam.ExamId))
                            {
                                viewModel.TodaysExams.Add(exam);
                            }
                        }
                    }
                }

                // Get upcoming exams (excluding completed)
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "usp_GetUpcomingExamsForStudent";
                    command.CommandType = CommandType.StoredProcedure;

                    var param = command.CreateParameter();
                    param.ParameterName = "@studentId";
                    param.Value = studentId;
                    command.Parameters.Add(param);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var exam = ReadExamFromReader(reader);
                            // Only add if not completed
                            if (!completedExamIds.Contains(exam.ExamId))
                            {
                                viewModel.UpcomingExams.Add(exam);
                            }
                        }
                    }
                }

                // Get stats from taken exams
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "usp_GetExamsTakenByStudent";
                    command.CommandType = CommandType.StoredProcedure;

                    var param = command.CreateParameter();
                    param.ParameterName = "@studentId";
                    param.Value = studentId;
                    command.Parameters.Add(param);

                    var grades = new List<int>();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            viewModel.TotalExamsTaken++;
                            if (!reader.IsDBNull(reader.GetOrdinal("grade")))
                            {
                                grades.Add(reader.GetInt32(reader.GetOrdinal("grade")));
                            }
                        }
                    }
                    viewModel.AverageGrade = grades.Any() ? Math.Round(grades.Average(), 1) : 0;
                }
            }
            finally
            {
                connection.Close();
            }

            ViewData["stdName"] = viewModel.StudentName;
            return View(viewModel);
        }

        private AvailableExamViewModel ReadExamFromReader(System.Data.Common.DbDataReader reader)
        {
            var exam = new AvailableExamViewModel
            {
                ExamId = reader.GetInt32(reader.GetOrdinal("examId")),
            };

            // Safely read optional columns
            try { exam.ExamTitle = reader.IsDBNull(reader.GetOrdinal("examTitle")) ? null : reader.GetString(reader.GetOrdinal("examTitle")); } catch { }
            try { exam.ExamDate = reader.IsDBNull(reader.GetOrdinal("examDate")) ? null : reader.GetDateTime(reader.GetOrdinal("examDate")); } catch { }
            try { exam.ExamDuration = reader.IsDBNull(reader.GetOrdinal("examDuration")) ? null : reader.GetInt32(reader.GetOrdinal("examDuration")); } catch { }
            try { exam.ExamDescription = reader.IsDBNull(reader.GetOrdinal("examDescription")) ? null : reader.GetString(reader.GetOrdinal("examDescription")); } catch { }
            try { exam.CourseId = reader.IsDBNull(reader.GetOrdinal("courseID")) ? null : reader.GetInt32(reader.GetOrdinal("courseID")); } catch { }
            try { exam.CourseName = reader.IsDBNull(reader.GetOrdinal("courseName")) ? null : reader.GetString(reader.GetOrdinal("courseName")); } catch { }

            return exam;
        }

        public IActionResult MyExams()
        {
            if (HttpContext.Session.GetInt32("StudentId") == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var studentId = HttpContext.Session.GetInt32("StudentId");
            var student = db.Students.FirstOrDefault(s => s.StudentId == studentId);
            ViewData["stdName"] = student?.StudentName;

            var exams = new List<StudentExamViewModel>();

            var connection = db.Database.GetDbConnection();
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "usp_GetExamsTakenByStudent";
                command.CommandType = CommandType.StoredProcedure;
                
                var param = command.CreateParameter();
                param.ParameterName = "@studentId";
                param.Value = studentId;
                command.Parameters.Add(param);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        exams.Add(new StudentExamViewModel
                        {
                            ExamId = reader.GetInt32(reader.GetOrdinal("examId")),
                            ExamTitle = reader.IsDBNull(reader.GetOrdinal("examTitle")) ? null : reader.GetString(reader.GetOrdinal("examTitle")),
                            ExamDate = reader.IsDBNull(reader.GetOrdinal("examDate")) ? null : reader.GetDateTime(reader.GetOrdinal("examDate")),
                            ExamDuration = reader.IsDBNull(reader.GetOrdinal("examDuration")) ? null : reader.GetInt32(reader.GetOrdinal("examDuration")),
                            ExamDescription = reader.IsDBNull(reader.GetOrdinal("examDescription")) ? null : reader.GetString(reader.GetOrdinal("examDescription")),
                            CourseId = reader.IsDBNull(reader.GetOrdinal("courseID")) ? null : reader.GetInt32(reader.GetOrdinal("courseID")),
                            Grade = reader.IsDBNull(reader.GetOrdinal("grade")) ? null : reader.GetInt32(reader.GetOrdinal("grade"))
                        });
                    }
                }
            }

            connection.Close();

            return View(exams);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
