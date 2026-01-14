using System.Data;
using System.Diagnostics;
using ASQL_Online_Exam_.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASQL_Online_Exam_.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _db;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _db = context;
        }

        #region Actions

        public IActionResult Index()
        {
            var studentId = GetStudentIdOrRedirect();
            if (studentId == null) return RedirectToLogin();

            var student = GetStudentWithDetails(studentId.Value);
            var viewModel = BuildDashboardViewModel(student);

            // Load exam data from stored procedures
            LoadDashboardExamData(studentId.Value, viewModel);

            ViewData["stdName"] = viewModel.StudentName;
            return View(viewModel);
        }

        public IActionResult MyExams()
        {
            var studentId = GetStudentIdOrRedirect();
            if (studentId == null) return RedirectToLogin();

            var student = _db.Students.FirstOrDefault(s => s.StudentId == studentId);
            ViewData["stdName"] = student?.StudentName;

            var exams = GetStudentExamHistory(studentId.Value);
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

        #endregion

        #region Private Helpers

        private int? GetStudentIdOrRedirect()
        {
            return HttpContext.Session.GetInt32("StudentId");
        }

        private IActionResult RedirectToLogin()
        {
            return RedirectToAction("Login", "Login");
        }

        private Student? GetStudentWithDetails(int studentId)
        {
            return _db.Students
                .Include(s => s.Track)
                    .ThenInclude(t => t.Branches)
                .Include(s => s.Track)
                    .ThenInclude(t => t.Courses)
                .FirstOrDefault(s => s.StudentId == studentId);
        }

        private DashboardViewModel BuildDashboardViewModel(Student? student)
        {
            return new DashboardViewModel
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
        }

        private void LoadDashboardExamData(int studentId, DashboardViewModel viewModel)
        {
            var connection = _db.Database.GetDbConnection();
            
            try
            {
                connection.Open();

                // Today's exams
                viewModel.TodaysExams = ExecuteExamStoredProcedure("usp_GetTodayExamsForStudent", studentId);

                // Upcoming exams
                viewModel.UpcomingExams = ExecuteExamStoredProcedure("usp_GetUpcomingExamsForStudent", studentId);

                // Exam statistics
                LoadExamStatistics(studentId, viewModel);
            }
            finally
            {
                connection.Close();
            }
        }

        private List<AvailableExamViewModel> ExecuteExamStoredProcedure(string spName, int studentId)
        {
            var exams = new List<AvailableExamViewModel>();
            var connection = _db.Database.GetDbConnection();

            using var command = connection.CreateCommand();
            command.CommandText = spName;
            command.CommandType = CommandType.StoredProcedure;

            var param = command.CreateParameter();
            param.ParameterName = "@studentId";
            param.Value = studentId;
            command.Parameters.Add(param);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                exams.Add(ReadExamFromReader(reader));
            }

            return exams;
        }

        private void LoadExamStatistics(int studentId, DashboardViewModel viewModel)
        {
            var connection = _db.Database.GetDbConnection();
            
            using var command = connection.CreateCommand();
            command.CommandText = "usp_GetExamsTakenByStudent";
            command.CommandType = CommandType.StoredProcedure;

            var param = command.CreateParameter();
            param.ParameterName = "@studentId";
            param.Value = studentId;
            command.Parameters.Add(param);

            var grades = new List<int>();
            using var reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                viewModel.TotalExamsTaken++;
                var gradeOrdinal = reader.GetOrdinal("grade");
                if (!reader.IsDBNull(gradeOrdinal))
                {
                    grades.Add(reader.GetInt32(gradeOrdinal));
                }
            }

            viewModel.AverageGrade = grades.Any() ? Math.Round(grades.Average(), 1) : 0;
        }

        private AvailableExamViewModel ReadExamFromReader(System.Data.Common.DbDataReader reader)
        {
            var exam = new AvailableExamViewModel
            {
                ExamId = reader.GetInt32(reader.GetOrdinal("examId")),
            };

            // Safely read optional columns with helper
            exam.ExamTitle = GetSafeString(reader, "examTitle");
            exam.ExamDate = GetSafeDateTime(reader, "examDate");
            exam.ExamDuration = GetSafeInt(reader, "examDuration");
            exam.ExamDescription = GetSafeString(reader, "examDescription");
            exam.CourseId = GetSafeInt(reader, "courseID");
            exam.CourseName = GetSafeString(reader, "courseName");

            return exam;
        }

        private List<StudentExamViewModel> GetStudentExamHistory(int studentId)
        {
            var exams = new List<StudentExamViewModel>();
            var connection = _db.Database.GetDbConnection();

            try
            {
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = "usp_GetExamsTakenByStudent";
                command.CommandType = CommandType.StoredProcedure;

                var param = command.CreateParameter();
                param.ParameterName = "@studentId";
                param.Value = studentId;
                command.Parameters.Add(param);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    exams.Add(new StudentExamViewModel
                    {
                        ExamId = reader.GetInt32(reader.GetOrdinal("examId")),
                        ExamTitle = GetSafeString(reader, "examTitle"),
                        ExamDate = GetSafeDateTime(reader, "examDate"),
                        ExamDuration = GetSafeInt(reader, "examDuration"),
                        ExamDescription = GetSafeString(reader, "examDescription"),
                        CourseId = GetSafeInt(reader, "courseID"),
                        Grade = GetSafeInt(reader, "grade"),
                        totalGrade = GetSafeInt(reader,"totalGrade")
                    });
                }
            }
            finally
            {
                connection.Close();
            }

            return exams;
        }

        #endregion

        #region Data Reader Helpers

        private static string? GetSafeString(System.Data.Common.DbDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
            }
            catch { return null; }
        }

        private static int? GetSafeInt(System.Data.Common.DbDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
            }
            catch { return null; }
        }

        private static DateTime? GetSafeDateTime(System.Data.Common.DbDataReader reader, string columnName)
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
