using cs3750LMS.Models.entites;
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

        //By passingin the user list, will match from this objects course list
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
    }
}
