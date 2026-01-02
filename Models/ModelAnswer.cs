using System;
using System.Collections.Generic;

namespace ASQL_Online_Exam_.Models
{
    public partial class ModelAnswer
    {
        public int QuestionId { get; set; }
        public int ChoiceId { get; set; }

        public virtual Choice Choice { get; set; } = null!;
        public virtual Question Question { get; set; } = null!;
    }
}
