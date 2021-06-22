using cs3750LMS.Models.entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.Models.general
{
    public class TimeStamp
    {
        public int CourseId { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }

        public List<TimeStamp> ParseTimes(Courses courseListing){
            List<TimeStamp> userTimes = new List<TimeStamp>();
            if (courseListing.CourseList != null)
            {
                foreach (Course c in courseListing.CourseList)
                {
                    TimeStamp newStamp = new TimeStamp();
                    newStamp.CourseId = c.CourseID;
                    newStamp.startTime = DateTime.Today + c.StartTime;
                    newStamp.endTime = DateTime.Today + c.EndTime;
                    userTimes.Add(newStamp);
                }
            }

            return userTimes;
        }
    }
}
