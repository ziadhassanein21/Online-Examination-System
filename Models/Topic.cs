using System;
using System.Collections.Generic;

namespace ASQL_Online_Exam_.Models
{
    public partial class Topic
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; } = null!;
        public int? CourseId { get; set; }

        public virtual Course? Course { get; set; }
    }
}
