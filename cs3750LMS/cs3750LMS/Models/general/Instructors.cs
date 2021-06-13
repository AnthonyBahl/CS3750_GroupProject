using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.Models.general
{
    public class Instructors
    {
        public List<User> InstructorUsers { get; set; }
        public List<Instructor> InstructorList { get; set; }
    }
}