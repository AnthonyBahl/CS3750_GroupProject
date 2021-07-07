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
                var instructor = _context.Users.Find(2);

                var instructorCourses = _context.Courses.Count(c => c.InstructorID.Equals(instructor.UserId));
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

               var controller = new InstructorController(_context, Environment);

                ClassValidationAdd newClass2 = new ClassValidationAdd();
                newClass2.Instructor = instructor.UserId.ToString();
                newClass2.Department = 3;
                newClass2.ClassNumber = "1234";
                newClass2.ClassTitle = "TestClass";
                newClass2.Description = "This is a test";
                newClass2.Location = "Test Location";
                newClass2.Credits = 4;
                newClass2.Capacity = 40;
                newClass2.MeetDays = "Monday, Wendesday, Friday";
                newClass2.StartTime = startTime;
                newClass2.EndTime = endTime;
                newClass2.Color = "#1ecbe1";


                HttpContext.Session.SetString("userInfo") = (object)instructor;

                // Act
                controller.AddClass(newClass2);

                // Assert

                Assert.Equals(instructorCourses, expectedInstructorCourses);
       
            } // Dispose rolls back everything.

           
        }
    }
}
