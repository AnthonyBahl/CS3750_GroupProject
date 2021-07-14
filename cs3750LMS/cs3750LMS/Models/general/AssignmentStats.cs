using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cs3750LMS.Models.entites;
using cs3750LMS.Models.general;

namespace cs3750LMS.Models.general
{
    public class AssignmentStats
    {
        public Assignment Assignment { get; set; }
        public List<Submission> SubmissionList { get; set; }
        public int Max { get; set; }
        public int Min { get; set; }
        public double Avg { get; set; }
        public List<int> GradeDistribution { get; set; }
    }
}
