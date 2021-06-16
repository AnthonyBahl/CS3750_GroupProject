using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.Models.entites
{
    public class Assignment
    {
        [Key]
        public int AssignmentID { get; set; }

        public int CourseID { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }
   
        public int MaxPoints { get; set; }
    
        public DateTime DueDate { get; set; }

        public int SubmissionType { get; set; }
    }
}
