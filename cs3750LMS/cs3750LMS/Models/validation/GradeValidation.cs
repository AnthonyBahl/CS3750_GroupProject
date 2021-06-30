using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.Models.validation
{
    public class GradeValidation
    {
        public int SubmissionID { get; set; }

        public int AssignmentID { get; set; }

        public int StudentID { get; set; }

        public DateTime SubmissionDate { get; set; }

        public int SubmissionType { get; set; }

        [Required]
        [Range(.01, float.MaxValue, ErrorMessage = "Please enter a value bigger than {1}")]
        public int Grade { get; set; }

        public string Contents { get; set; }
    }
}
