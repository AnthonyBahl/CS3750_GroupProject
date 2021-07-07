using Microsoft.VisualStudio.TestTools.UnitTesting;
using cs3750LMS.Controllers;
using System.Collections.Generic;
using System;
using cs3750LMS.Models;
using System.Transactions;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using cs3750LMS.DataAccess;

namespace CS3750LMSTest
{
    [TestClass]
    public class UnitTests
    {
        private cs3750Context db = new cs3750Context();

        cs3750Context _context;
        private IHostingEnvironment Environment;
        public UnitTests()
        {
            var serviceProvider = new ServiceCollection()
           .AddEntityFrameworkSqlServer()
           .BuildServiceProvider();

            var builder = new DbContextOptionsBuilder<cs3750Context>();
            builder.UseSqlServer($"Data Source=titan.cs.weber.edu,10433;Initial Catalog=LMSBinEnt;USER ID=LMSBinEnt;Password=8!N4Ry3n7")
                    .UseInternalServiceProvider(serviceProvider);

            _context = new cs3750Context(builder.Options);
            _context.Database.Migrate(); 
            //Environment.EnvironmentName();
        }

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
                var instructor = _context.Users.Find(1025);

                var instructorCourses = _context.Courses.Count(c => c.InstructorID.Equals(instructor.UserId));
                var expectedInstructorCourses = instructorCourses + 1;

                TimeSpan startTime = new TimeSpan(21, 40, 50);
                TimeSpan endTime = new TimeSpan(21, 50, 50); 

               var controller = new InstructorController(_context, Environment);

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
                controller.AddClassTodb(instructor.UserId, newClass);
                instructorCourses = _context.Courses.Count(c => c.InstructorID.Equals(instructor.UserId));
                // Assert

                Assert.AreEqual(instructorCourses, expectedInstructorCourses);
       
            } // Dispose rolls back everything.

           
        }
    }
}
