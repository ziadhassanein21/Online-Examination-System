using System;
using System.Collections.Generic;

namespace ASQL_Online_Exam_.Models
{
    public partial class Student
    {
        public Student()
        {
            ExamQuestionsPools = new HashSet<ExamQuestionsPool>();
            StudentExams = new HashSet<StudentExam>();
        }

        public int StudentId { get; set; }
        public string? StudentSocialSecurityNumber { get; set; }
        public string? StudentName { get; set; }
        public string? StudentEmail { get; set; }
        public DateTime? StudentEnrollementDate { get; set; }
        public string? StudentPasswordHashed { get; set; }
        public string? StudentPasswordSalt { get; set; }
        public string? StudentPhoneNumber { get; set; }
        public int? TrackId { get; set; }

        public virtual Track? Track { get; set; }
        public virtual ICollection<ExamQuestionsPool> ExamQuestionsPools { get; set; }
        public virtual ICollection<StudentExam> StudentExams { get; set; }
    }
}
