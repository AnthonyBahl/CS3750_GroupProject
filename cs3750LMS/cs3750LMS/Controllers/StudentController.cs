using cs3750LMS.DataAccess;
using cs3750LMS.Models;
using cs3750LMS.Models.entites;
using cs3750LMS.Models.general;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.Controllers
{
    public class StudentController : Controller
    {
        private readonly cs3750Context _context;
        public StudentController(cs3750Context context)
        {
            _context = context;
        }

        public IActionResult Register()
        {
            if (HttpContext.Session.Get<string>("user") != null)
            {
                User userFound = _context.Users.Where(u => u.Email == HttpContext.Session.Get<string>("user")).Single();
                UserSession session = new UserSession
                {
                    Email = userFound.Email,
                    FirstName = userFound.FirstName,
                    LastName = userFound.LastName,
                    Birthday = userFound.Birthday,
                    AccountType = userFound.AccountType
                };
                if (userFound.AccountType == 0)
                {
                    //set enrollment object for next pass
                    Enrollments enrollment = new Enrollments
                    {
                        EnrollmentList = _context.Enrollments.Where(x => x.studentID == userFound.UserId).ToList()
                    };

                    Courses courses = new Courses
                    {
                        CourseList = _context.Courses.ToList()
                    };

                    Courses studentCourses = new Courses
                    {
                        CourseList = new List<Course>()
                    };

                    for (int i = 0; i < courses.CourseList.Count; i++)
                    { 
                        for (int j = 0; j < enrollment.EnrollmentList.Count; j++)
                        {
                            if(courses.CourseList[i].CourseID == enrollment.EnrollmentList[j].courseID)
                            {
                                studentCourses.CourseList.Add(courses.CourseList[i]);
                            }
                        }
                    }

                    ViewData["Message"] = session;
                    ViewData["Enrollment"] = enrollment;
                    ViewData["Courses"] = courses;
                    ViewData["StudentCourses"] = studentCourses;
                    return View();
                }
            }
            return View("~/Views/Home/Login.cshtml");
        }
        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Register([Bind("studentID,courseID")] Enrollment passedEnrollment)
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

            Enrollment newEnrollment = new Enrollment
            {
                studentID = userFound.UserId,
                courseID = passedEnrollment.courseID,

            };
            
            _context.Enrollments.Add(newEnrollment);
            _context.SaveChanges();
            success = true;

            //set courses object, and success for next pass
            if (success)
            {
                session.ClassState = 0;
            }
            else
            {
                session.ClassState = 1;
            }
            //set enrollment object for next pass
            Enrollments enrollment = new Enrollments
            {
                EnrollmentList = _context.Enrollments.Where(x => x.studentID == userFound.UserId).ToList()
            };

            Courses courses = new Courses
            {
                CourseList = _context.Courses.ToList()
            };

            Courses studentCourses = new Courses
            {
                CourseList = new List<Course>()
            };

            for (int i = 0; i < courses.CourseList.Count; i++)
            {
                for (int j = 0; j < enrollment.EnrollmentList.Count; j++)
                {
                    if (courses.CourseList[i].CourseID == enrollment.EnrollmentList[j].courseID)
                    {
                        studentCourses.CourseList.Add(courses.CourseList[i]);
                    }
                }
            }

            ViewData["Message"] = session;
            ViewData["Enrollment"] = enrollment;
            ViewData["Courses"] = courses;
            ViewData["StudentCourses"] = studentCourses;
            return View("~/Views/Student/Register.cshtml");
        }
    }
}
