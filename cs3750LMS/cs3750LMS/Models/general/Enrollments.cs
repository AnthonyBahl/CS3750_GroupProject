using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cs3750LMS.Models.entites;

namespace cs3750LMS.Models.general
{
    public class Enrollments
    {
        public List<Course> CourseList { get; set; }
        public List<Enrollment> EnrollmentList { get; set; }
        public List<Course> StudentCourseList { get; set; }
    }
}
