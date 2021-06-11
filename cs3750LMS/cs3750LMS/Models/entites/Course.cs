using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.Models.entites
{
    public class Course
    {
        [Key]
        public int CourseID { get; set; }
        public int InstructorID { get; set; }

        public int Department { get; set; }

        public string ClassNumber { get; set; }
   
        public string ClassTitle { get; set; }
    
        public string Description { get; set; }

        public string Location { get; set; }
     
        public int Credits { get; set; }

        public int Capacity { get; set; }
       
        public string MeetDays { get; set; }
        
        public TimeSpan StartTime { get; set; }
        
        public TimeSpan EndTime { get; set; }
    }
}
