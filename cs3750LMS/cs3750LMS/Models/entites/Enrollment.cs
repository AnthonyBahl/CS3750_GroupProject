using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.Models.entites
{
    public class Enrollment
    {
        [Key]
        public int enrollmentID { get; set; }
        public int studentID { get; set; }
        public int courseID { get; set; }
    }
}
