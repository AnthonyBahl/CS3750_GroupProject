using cs3750LMS.DataAccess;
using cs3750LMS.Models;
using cs3750LMS.Models.entites;
using cs3750LMS.Models.general;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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
                //get user info from session
                string serialUser = HttpContext.Session.GetString("userInfo");
                UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);

                if (session.AccountType == 0)
                {
                    //grab session vars for registration
                    string serialEnrollment = HttpContext.Session.GetString("userEnrollment");
                    string serialAllCourses = HttpContext.Session.GetString("AllCourses");

                    Courses allCourses;
                    Enrollments enrollment;

                    if (serialEnrollment != null && serialAllCourses != null)
                    {
                        allCourses = JsonSerializer.Deserialize<Courses>(serialAllCourses);
                        enrollment = JsonSerializer.Deserialize<Enrollments>(serialEnrollment);
                    }
                    else
                    {
                        //grab enrollment
                        enrollment = new Enrollments
                        {
                            EnrollmentList = _context.Enrollments.Where(x => x.studentID == session.UserId).ToList()
                        };

                        //grab instructors
                        Instructors instructors = new Instructors
                        {
                            InstructorUsers = _context.Users.Where(x => x.AccountType == 1).ToList(),
                            InstructorList = new List<Instructor>()
                        };

                        for (int i = 0; i < instructors.InstructorUsers.Count; i++)
                        {
                            Instructor newInstructor = new Instructor();
                            newInstructor.UserId = instructors.InstructorUsers[i].UserId;
                            newInstructor.FirstName = instructors.InstructorUsers[i].FirstName;
                            newInstructor.LastName = instructors.InstructorUsers[i].LastName;
                            instructors.InstructorList.Add(newInstructor);
                        }

                        //grab all courses
                        allCourses = new Courses
                        {
                            CourseList = _context.Courses.ToList(),
                            CourseInstructors = instructors.InstructorList
                        };

                        //save to session
                        HttpContext.Session.SetString("userEnrollment", JsonSerializer.Serialize(enrollment));
                        HttpContext.Session.SetString("AllCourses",JsonSerializer.Serialize(allCourses));
                    }


                    //get student courses from session
                    string serialCourse = HttpContext.Session.GetString("userCourses");
                    Courses studentCourses = serialCourse == null ? null : JsonSerializer.Deserialize<Courses>(serialCourse);

                    //pass data to view
                    ViewData["Message"] = session;
                    ViewData["Courses"] = allCourses;
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
            //get user info from session
            string serialUser = HttpContext.Session.GetString("userInfo");
            UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);

            //grab session vars for registration
            string serialEnrollment = HttpContext.Session.GetString("userEnrollment");
            string serialAllCourses = HttpContext.Session.GetString("AllCourses");

            Courses allCourses = JsonSerializer.Deserialize<Courses>(serialAllCourses);
            Enrollments enrollment = JsonSerializer.Deserialize<Enrollments>(serialEnrollment);

            //get student courses from session
            string serialCourse = HttpContext.Session.GetString("userCourses");
            Courses studentCourses = serialCourse == null ? null : JsonSerializer.Deserialize<Courses>(serialCourse);

            //if model valid add new course
            bool success = false;
  
            Enrollment newEnrollment = new Enrollment
            {
                studentID = session.UserId,
                courseID = passedEnrollment.courseID
            };

            List<Enrollment> errorCheck = _context.Enrollments.Where(x => (x.studentID == passedEnrollment.studentID && x.courseID == passedEnrollment.courseID)).ToList();
            _context.Enrollments.Add(newEnrollment);
            _context.SaveChanges();
            success = true;

            enrollment.EnrollmentList = _context.Enrollments.Where(x => x.studentID == session.UserId).ToList();
            HttpContext.Session.SetString("userEnrollment", JsonSerializer.Serialize(enrollment));


            //set courses object, and success for next pass
            if (success)
            {
                session.ClassState = 0;
            }
            else
            {
                session.ClassState = 1;
            }

            //pass data to view
            ViewData["Message"] = session;
            ViewData["Courses"] = allCourses;
            ViewData["StudentCourses"] = studentCourses;
            return View("~/Views/Student/Register.cshtml");
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Drop([Bind("studentID,courseID")] Enrollment passedEnrollment)
        {
            //get user info from session
            string serialUser = HttpContext.Session.GetString("userInfo");
            UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);

            //grab session vars for registration
            string serialEnrollment = HttpContext.Session.GetString("userEnrollment");
            string serialAllCourses = HttpContext.Session.GetString("AllCourses");

            Courses allCourses = JsonSerializer.Deserialize<Courses>(serialAllCourses);
            Enrollments enrollment = JsonSerializer.Deserialize<Enrollments>(serialEnrollment);

            //get student courses from session
            string serialCourse = HttpContext.Session.GetString("userCourses");
            Courses studentCourses = serialCourse == null ? null : JsonSerializer.Deserialize<Courses>(serialCourse);

            //if model valid add new course
            bool success = false;

            Enrollment DropCourse = (Enrollment)_context.Enrollments.Where(x => (x.studentID == session.UserId && x.courseID == passedEnrollment.courseID)).Single();
            _context.Enrollments.Remove(DropCourse);
            _context.SaveChanges();
            success = true;

            enrollment.EnrollmentList = _context.Enrollments.Where(x => x.studentID == session.UserId).ToList();
            HttpContext.Session.SetString("userEnrollment", JsonSerializer.Serialize(enrollment));

            //set courses object, and success for next pass
            if (success)
            {
                session.ClassState = 0;
            }
            else
            {
                session.ClassState = 1;
            }

            ViewData["Message"] = session;
            ViewData["Courses"] = allCourses;
            ViewData["StudentCourses"] = studentCourses;
            return View("~/Views/Student/Register.cshtml");
        }

        public IActionResult Payment()
        {
            if (HttpContext.Session.Get<string>("user") != null)
            {
                //get user info from session
                string serialUser = HttpContext.Session.GetString("userInfo");
                UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);

                if (session.AccountType == 0)
                {
                    //grab session vars for registration
                    string serialEnrollment = HttpContext.Session.GetString("userEnrollment");
                    string serialAllCourses = HttpContext.Session.GetString("AllCourses");

                    Courses allCourses;
                    Enrollments enrollment;

                    if (serialEnrollment != null && serialAllCourses != null)
                    {
                        allCourses = JsonSerializer.Deserialize<Courses>(serialAllCourses);
                        enrollment = JsonSerializer.Deserialize<Enrollments>(serialEnrollment);
                    }
                    else
                    {
                        //grab enrollment
                        enrollment = new Enrollments
                        {
                            EnrollmentList = _context.Enrollments.Where(x => x.studentID == session.UserId).ToList()
                        };

                        //grab instructors
                        Instructors instructors = new Instructors
                        {
                            InstructorUsers = _context.Users.Where(x => x.AccountType == 1).ToList(),
                            InstructorList = new List<Instructor>()
                        };

                        for (int i = 0; i < instructors.InstructorUsers.Count; i++)
                        {
                            Instructor newInstructor = new Instructor();
                            newInstructor.UserId = instructors.InstructorUsers[i].UserId;
                            newInstructor.FirstName = instructors.InstructorUsers[i].FirstName;
                            newInstructor.LastName = instructors.InstructorUsers[i].LastName;
                            instructors.InstructorList.Add(newInstructor);
                        }

                        //grab all courses
                        allCourses = new Courses
                        {
                            CourseList = _context.Courses.ToList(),
                            CourseInstructors = instructors.InstructorList
                        };

                        //save to session
                        HttpContext.Session.SetString("userEnrollment", JsonSerializer.Serialize(enrollment));
                        HttpContext.Session.SetString("AllCourses", JsonSerializer.Serialize(allCourses));
                    }


                    //get student courses from session
                    string serialCourse = HttpContext.Session.GetString("userCourses");
                    Courses studentCourses = serialCourse == null ? null : JsonSerializer.Deserialize<Courses>(serialCourse);

                    ViewData["Message"] = session;
                    ViewData["Courses"] = allCourses;
                    ViewData["StudentCourses"] = studentCourses;
                    return View();
                }
            }
            return View("~/Views/Home/Login.cshtml");
        }
    }
}
