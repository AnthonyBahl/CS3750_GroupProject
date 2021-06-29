﻿using cs3750LMS.Models.entites;
using cs3750LMS.Models.general;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.Models
{
    public class SpecificCourse
    {
        public Course Selection { get; set; }
        public List<Assignment> AssignmentList { get; set; }
        public int ModeSetting { get; set; } //for display result based on action
    }
}
