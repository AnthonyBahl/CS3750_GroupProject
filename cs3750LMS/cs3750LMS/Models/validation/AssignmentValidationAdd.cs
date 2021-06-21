using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.Models
{
    public class AssignmentValidationAdd
    {
        [Required]
        public int CourseID { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Maximum length of 100 characters")]
        public string Title { get; set; }
        [Required]
        [StringLength(256, ErrorMessage = "Maximum length of 256 characters")]
        public string Description { get; set; }
        [Required]
        public int MaxPoints { get; set; }
        [Required]
        public DateTime DueDate { get; set; }
        [Required]
        public TimeSpan DueTime { get; set; }
        [Required]
        public int SubmitType { get; set; }

    }
}
