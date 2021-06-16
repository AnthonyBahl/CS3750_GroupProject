using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;
using cs3750LMS.Models.validation;
using Microsoft.AspNetCore.Http;

namespace cs3750LMS.Models
{
    public class ClassValidationAdd
    {
        [Required]
        public string Instructor { get; set; }
        [Required]
        public int Department { get; set; }
        [Required]
        [StringLength(30, ErrorMessage = "Maximum length of 30 characters")]
        public string ClassNumber { get; set; }
        [Required]
        [StringLength(60, ErrorMessage = "Maximum length of 60 characters")]
        public string ClassTitle { get; set; }
        [Required]
        [StringLength(255, ErrorMessage = "Maximum length of 255 characters")]
        public string Description { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Maximum length of 100 characters")]
        public string Location { get; set; }
        [Required]
        public int Credits { get; set; }
        [Required]
        public int Capacity { get; set; }
        [Required]
        [RegularExpression("^(?!xxxxxxx).*$", ErrorMessage = "Must Select at least one day to meet")]
        public string MeetDays { get; set; }
        [Required]
        public TimeSpan StartTime { get; set; }
        [Required]
        public TimeSpan EndTime { get; set; }
        [Required]
        public string Color { get; set; }
    }
}
