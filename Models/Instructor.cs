using System;
using System.Collections.Generic;

namespace ASQL_Online_Exam_.Models
{
    public partial class Instructor
    {
        public Instructor()
        {
            Teaches = new HashSet<Teach>();
        }

        public int InstructorId { get; set; }
        public string? InstructorSocialSecurityNumber { get; set; }
        public string? InstructorName { get; set; }
        public string? InstructorEmail { get; set; }
        public string? InstructorPasswordHased { get; set; }
        public string? IsntructorPasswordSalt { get; set; }
        public string? InstructorPhoneNumber { get; set; }
        public DateTime? InstructorhireDate { get; set; }
        public int? InstructorBranchId { get; set; }
        public bool? InstructorActive { get; set; }

        public virtual Branch? InstructorBranch { get; set; }
        public virtual ICollection<Teach> Teaches { get; set; }
    }
}
