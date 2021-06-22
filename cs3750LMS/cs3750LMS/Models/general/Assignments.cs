using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cs3750LMS.Models.entites;
using cs3750LMS.Models.general;

namespace cs3750LMS.Models
{
    public class Assignments
    {
        public List<Assignment> AssignmentList { get; set; }
        public List<Submission> SubmissionList { get; set; }
    }
}
