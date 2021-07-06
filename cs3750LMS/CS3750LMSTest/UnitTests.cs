using Microsoft.VisualStudio.TestTools.UnitTesting;
using cs3750LMS.Controllers;
using System.Collections.Generic;
using System;
using cs3750LMS.Models;
using System.Transactions;
using System.Linq;

namespace CS3750LMSTest
{
    [TestClass]
    public class UnitTests
    {
        private cs3750Context db = new cs3750Context();

        [TestMethod]
        public void InstructorCanCreateACourseTest()
        {
            using (new TransactionScope())
            {
                //Start with known instructor id
                //Find out how many courses instructor is teaching
                //Call this sum N
                //Run code to create a new course => from project
                //Find out how many courses instructor is teaching
                //Should now be N+1

                // maybe use context

                // Arrange
                var instructor = db.Users.Find(2);

                // var instructor = instructors.FindInstructor(1);
                var instructorCourses = db.Courses.Count(c => c.InstructorID.Equals(instructor.UserId));
                var expectedInstructorCourses = instructorCourses + 1;

                TimeSpan startTime = new TimeSpan(1, 12, 23, 62);
                TimeSpan endTime = new TimeSpan(4, 1, 23, 73);

                //newClass: Instructor,Department,ClassNumber,ClassTitle,Description,Location,Credits,Capacity,MeetDays,StartTime,EndTime,Color"
                var newClass = new Dictionary<string, object>(){
                  {"Instructor", instructor.UserId },
                  {"Department", 3},
                  {"ClassNumber", "1234"},
                  {"ClassTitle", "TestClass"},
                  {"Description", "This is a test"},
                  {"Location", "Test Location"},
                  {"Credits", 4},
                  {"Capacity", 40},
                  {"MeetDays", "Monday, Wendesday, Friday"},
                  {"StartTime", startTime},
                  {"EndTime", endTime},
                  {"Color", "#1ecbe1"}
                };

                var controller = new InstructorController(db);

                // Act
                controller.AddClass();

                // Assert

                Assert.Equals(instructorCourses, expectedInstructorCourses);
       
            } // Dispose rolls back everything.

           
        }
    }
}
