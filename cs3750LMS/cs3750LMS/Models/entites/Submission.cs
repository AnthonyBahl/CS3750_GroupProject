using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.Models.entites
{
    public class Submission
    {
        [Key]
        public int SubmissionID { get; set; }

        public int AssignmentID { get; set; }

        public int StudentID { get; set; }

        public TimeSpan SubmissionDate { get; set; }

        public int SubmissionType { get; set; }
   
        public int Grade { get; set; }
    }
}
