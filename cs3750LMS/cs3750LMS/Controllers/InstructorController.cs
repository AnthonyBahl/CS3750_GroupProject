using cs3750LMS.DataAccess;
using cs3750LMS.Models;
using cs3750LMS.Models.entites;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.Controllers
{
    public class InstructorController : Controller
    {
        private readonly cs3750Context _context;
        public InstructorController(cs3750Context context)
        {
            _context = context;
        }

        public IActionResult AddClass()
        {
            if (HttpContext.Session.Get<string>("user") != null)
            {
                //get the session object for next pass
                User userFound = _context.Users.Where(u => u.Email == HttpContext.Session.Get<string>("user")).Single();
                UserSession session = new UserSession
                {
                    Email = userFound.Email,
                    FirstName = userFound.FirstName,
                    LastName = userFound.LastName,
                    Birthday = userFound.Birthday,
                    AccountType = userFound.AccountType
                };
                if (userFound.AccountType == 1)
                {
                    //set courses object for next pass
                    Courses courses = new Courses
                    {
                        CourseList = _context.Courses.Where(x => x.InstructorID == userFound.UserId).ToList()
                    };

                    Departments depts = new Departments
                    {
                        DeptsList = _context.Departments.ToList()
                    };
                    ViewData["DepartmentData"] = depts;
                    ViewData["Message"] = session;
                    ViewData["Courses"] = courses;
                    return View();
                }
            }
            return View("~/Views/Home/Login.cshtml");
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken ]
        public IActionResult AddClass([Bind("Instructor,Department,ClassNumber,ClassTitle,Description,Location,Credits,Capacity,MeetDays,StartTime,EndTime")] ClassValidationAdd newClass)
        {
            //get the session object for next pass
            User userFound = _context.Users.Where(u => u.Email == HttpContext.Session.Get<string>("user")).Single();
            UserSession session = new UserSession
            {
                Email = userFound.Email,
                FirstName = userFound.FirstName,
                LastName = userFound.LastName,
                Birthday = userFound.Birthday,
                AccountType = userFound.AccountType
            };

            //if model valid add new course
            bool success = false;
            if (ModelState.IsValid)
            {
                Course newCourse = new Course
                {
                    InstructorID = userFound.UserId,
                    Department = newClass.Department,
                    ClassNumber = newClass.ClassNumber,
                    ClassTitle = newClass.ClassTitle,
                    Description = newClass.Description,
                    Location = newClass.Location,
                    Credits = newClass.Credits,
                    Capacity = newClass.Capacity,
                    MeetDays = newClass.MeetDays,
                    StartTime = newClass.StartTime,
                    EndTime = newClass.EndTime
                };

                _context.Courses.Add(newCourse);
                _context.SaveChanges();
                success = true;
            }

            //set courses object, and success for next pass
            session.Success = success;
            Courses courses = new Courses
            {
                CourseList = _context.Courses.Where(x => x.InstructorID == userFound.UserId).ToList()
            };
            Departments depts = new Departments
            {
                DeptsList = _context.Departments.ToList()
            };
            ViewData["DepartmentData"] = depts;
            ViewData["Message"] = session;
            ViewData["Courses"] = courses;
            return View();
        }
    }
}
