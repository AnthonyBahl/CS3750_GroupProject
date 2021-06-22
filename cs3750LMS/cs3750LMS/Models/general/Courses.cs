using cs3750LMS.Models.entites;
using cs3750LMS.Models.general;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.Models
{
    public class Courses
    {
        public List<Course> CourseList { get; set; }
        public List<String> InstructorNames { get; set; }
        public List<Instructor> CourseInstructors { get; set; }

        //By passing in the user list, will match from this objects course list
        // and will fill the instructor full names in order to match with this object's
        //course list
        public void SetInstructorNames(List<User> instructors)
        {
            InstructorNames = new List<string>();
            for (int i = 0; i < CourseList.Count; i++)
            {
                InstructorNames.Add(instructors
                    .Where(x => x.UserId == CourseList[i].InstructorID)
                    .Select(y => y.FirstName + " " + y.LastName)
                    .Single());
            }
        }

        public void RefactorTimeSpans(List<TimeStamp> times)
        {
            if (times != null && CourseList != null)
            {
                for (int i = 0; i < CourseList.Count; i++)
                {
                    CourseList[i].StartTime = times[i].startTime.TimeOfDay;
                    CourseList[i].EndTime = times[i].endTime.TimeOfDay;
                }
            }
        }

        public string ParseDays(int index)
        {
            string days = "";
            Course selected = CourseList[index];
            if (selected.MeetDays[0] == 'y')
                days += "Sunday ";
            if (selected.MeetDays[1] == 'y')
                days += "Monday ";
            if (selected.MeetDays[2] == 'y')
                days += "Tuesday ";
            if (selected.MeetDays[3] == 'y')
                days += "Wednesday ";
            if (selected.MeetDays[4] == 'y')
                days += "Thursday ";
            if (selected.MeetDays[5] == 'y')
                days += "Friday ";
            if (selected.MeetDays[6] == 'y')
                days += "Saturday ";

            return days;
        }

        public string ParseDaysShort(int index)
        {
            string days = "";
            Course selected = CourseList[index];
            if (selected.MeetDays[0] == 'y')
                days += "Su";
            if (selected.MeetDays[1] == 'y')
                days += "M";
            if (selected.MeetDays[2] == 'y')
                days += "Tu";
            if (selected.MeetDays[3] == 'y')
                days += "W";
            if (selected.MeetDays[4] == 'y')
                days += "Th";
            if (selected.MeetDays[5] == 'y')
                days += "F";
            if (selected.MeetDays[6] == 'y')
                days += "Sa";

            return days;
        }
    }
}
