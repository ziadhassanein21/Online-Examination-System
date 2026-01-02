using System;
using System.Collections.Generic;

namespace ASQL_Online_Exam_.Models
{
    public class DashboardViewModel
    {
        public string StudentName { get; set; } = string.Empty;
        public string? StudentEmail { get; set; }
        public string? StudentPhone { get; set; }
        public DateTime? EnrollmentDate { get; set; }
        public string? TrackName { get; set; }
        public string? BranchName { get; set; }
        public string? BranchLocation { get; set; }
        public List<AvailableExamViewModel> TodaysExams { get; set; } = new();
        public List<AvailableExamViewModel> UpcomingExams { get; set; } = new();
        public int TotalExamsTaken { get; set; }
        public double AverageGrade { get; set; }
        public int TotalCourses { get; set; }
    }

    public class AvailableExamViewModel
    {
        public int ExamId { get; set; }
        public string? ExamTitle { get; set; }
        public DateTime? ExamDate { get; set; }
        public int? ExamDuration { get; set; }
        public string? ExamDescription { get; set; }
        public int? CourseId { get; set; }
        public string? CourseName { get; set; }
    }
}
