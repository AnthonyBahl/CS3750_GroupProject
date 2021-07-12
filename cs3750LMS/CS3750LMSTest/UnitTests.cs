using Microsoft.VisualStudio.TestTools.UnitTesting;
using cs3750LMS.Controllers;
using System;
using cs3750LMS.Models;
using System.Transactions;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using cs3750LMS.Models.Repository;

namespace CS3750LMSTest
{
    [TestClass]
    public class UnitTests
    {
        // variables to access db and controller
        cs3750Context _context;

        private IHostingEnvironment Environment;

        // constructor of what happens at every time this class is called
        public UnitTests()
        {
            // connect to database
            var serviceProvider = new ServiceCollection()
           .AddEntityFrameworkSqlServer()
           .BuildServiceProvider();

            var builder = new DbContextOptionsBuilder<cs3750Context>();
            builder.UseSqlServer($"Data Source=titan.cs.weber.edu,10433;Initial Catalog=LMSBinEnt;USER ID=LMSBinEnt;Password=8!N4Ry3n7")
                    .UseInternalServiceProvider(serviceProvider);

            _context = new cs3750Context(builder.Options);
            _context.Database.Migrate(); 
        }

        [TestMethod]
        public void InstructorCanCreateACourseTest()
        {
            // in a transaction scope so it will not be run in the database
            using (new TransactionScope())
            {
                // Arrange
                // start with a known instructor
                var instructor = _context.Users.Find(1025);

                // grab the instructor course count
                var instructorCourses = _context.Courses.Count(c => c.InstructorID.Equals(instructor.UserId));
                var expectedInstructorCourses = instructorCourses + 1; // grab the expected result

                // define time spans
                TimeSpan startTime = new TimeSpan(21, 40, 50);
                TimeSpan endTime = new TimeSpan(21, 50, 50); 

                // call instructor controller with the context and enviroment passed in
                var controller = new InstructorController(_context, Environment);

                // create a new class object
                ClassValidationAdd newClass = new ClassValidationAdd();

                // add fields
                newClass.Instructor = instructor.UserId.ToString();
                newClass.Department = 3;
                newClass.ClassNumber = "1234";
                newClass.ClassTitle = "TestClass";
                newClass.Description = "This is a test";
                newClass.Location = "Test Location";
                newClass.Credits = 4;
                newClass.Capacity = 40;
                newClass.MeetDays = "xyxyxyx";
                newClass.StartTime = startTime;
                newClass.EndTime = endTime;
                newClass.Color = "#1ecbe1";

                // Act
                // add the class in the controller
                controller.AddClassTodb(instructor.UserId, newClass);
                // find out the count again
                instructorCourses = _context.Courses.Count(c => c.InstructorID.Equals(instructor.UserId));
               
                // Assert
                Assert.AreEqual(instructorCourses, expectedInstructorCourses);
       
            } // Dispose rolls back everything.
        }

        /// <summary>
        /// This method tests to make sure that the create assignment functionality does not break.
        /// </summary>
        [TestMethod]
        public void InstructorCanCreateAnAssignmentTest() {
            // Set up Transaction Scope so that nothing is added to the database
            using (new TransactionScope())
            {
                // Grab user 1025 who is a test instructor in the database
                var instructor = _context.Users.Find(1025);

                // grab list of courses
                var instructorCourses = _context.Courses.Where(c => c.InstructorID.Equals(instructor.UserId)).ToList();
                // Get latest course
                var latestCourse = instructorCourses.Last();
                // Get assignment count
                int currentAssignmentCount = _context.Assignments.Count(a => a.CourseID.Equals(latestCourse.CourseID));

                // Create new assignment
                AssignmentValidationAdd newAssignment = new AssignmentValidationAdd();

                // Assign values to each field
                newAssignment.CourseID = latestCourse.CourseID;
                newAssignment.Title = "Test Assignment";
                newAssignment.Description = "This is just a test";
                newAssignment.MaxPoints = 100;
                newAssignment.DueDate = DateTime.Now.AddDays(5);
                newAssignment.DueTime = new TimeSpan(21, 40, 50);
                newAssignment.SubmitType = 1;

                // Create an instance of the Instructor controller
                InstructorController controller = new InstructorController(_context, Environment);

                // Add the Assignment in the controller
                controller.AddAssignmentTodb(newAssignment);

                // Check assignment count again
                var updatedAssignmentCount = _context.Assignments.Count(a => a.CourseID.Equals(latestCourse.CourseID));

                // Determine if everything is working.
                Assert.AreEqual(updatedAssignmentCount, currentAssignmentCount + 1);

            }
        }

    }
}
