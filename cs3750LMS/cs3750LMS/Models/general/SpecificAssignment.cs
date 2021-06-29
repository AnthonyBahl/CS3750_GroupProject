using cs3750LMS.Models.entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.Models.general
{
    public class SpecificAssignment
    {
        public Assignment Selection { get; set; }
        public List<Submission> SubmissionList { get; set; }
        public int ModeSetting { get; set; } //for display result based on action
    }
}
