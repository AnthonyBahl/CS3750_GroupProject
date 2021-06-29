using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.Models
{
    public class SubmitAssignmentValidation
    {
        public IFormFile FileSubmission { get; set; }
        [MaxLength(1000,ErrorMessage ="Maximum character Length is 1000")]
        public string TextSubmission { get; set; }
        public int CourseId { get; set; }
        public int AssignmentId { get; set; }
    }
}
