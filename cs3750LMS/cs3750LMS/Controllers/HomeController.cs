using cs3750LMS.DataAccess;
using cs3750LMS.Models;
using cs3750LMS.Models.entites;
using cs3750LMS.Models.general;
using cs3750LMS.Models.validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace cs3750LMS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly cs3750Context _context;
        public HomeController(ILogger<HomeController> logger, cs3750Context context)
        {
            _logger = logger;
            _context = context;
        }
        public IActionResult Index()
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

//-----Added Code-----
                // Get Courses from Database
                Courses courses = new Courses
                {
                    CourseList = _context.Courses.ToList()
                };

                Courses userCourses = new Courses
                {
                    CourseList = new List<Course>()
                };

                // If the user is a student
                if (session.AccountType == 0)
                {

                    //set enrollment object for next pass
                    Enrollments enrollment = new Enrollments
                    {
                        EnrollmentList = _context.Enrollments.Where(x => x.studentID == userFound.UserId).ToList()
                    };

                    // Populating the user course list
                    for (int i = 0; i < courses.CourseList.Count; i++)
                    {
                        for (int j = 0; j < enrollment.EnrollmentList.Count; j++)
                        {
                            if (courses.CourseList[i].CourseID == enrollment.EnrollmentList[j].courseID)
                            {
                                userCourses.CourseList.Add(courses.CourseList[i]);
                            }
                        }
                    }

                }
                // If the user is an instructor
                else if (session.AccountType == 1)
                {
                    // Populating the user course list
                    for (int i = 0; i < courses.CourseList.Count; i++)
                    {
                        if (courses.CourseList[i].InstructorID == session.UserId)  //If the course is taught by the instructor
                        {
                            userCourses.CourseList.Add(courses.CourseList[i]);  //add it to the list. 
                        }
                    }
                }



                ViewData["UserCourses"] = userCourses;
 //-----ADDED CODE END

                ViewData["Message"] = session;
                return View();
            }
            return View("~/Views/Home/Login.cshtml");
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return View("~/Views/Home/Login.cshtml");
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp([Bind("Email,FirstName,LastName,Birthday,Password,ConfirmPassword,AccountType")] UserValidationSignUp testUser)
        {
            if (ModelState.IsValid) { 
                if(_context.Users.Count(e => e.Email == testUser.Email) == 0)
                {

                    User users = new Models.User
                    {
                        Email = testUser.Email,
                        FirstName = testUser.FirstName,
                        LastName = testUser.LastName,
                        Birthday = testUser.Birthday,
                        Password = Sha256(testUser.Password),
                        AccountType = testUser.AccountType
                    };

                    _context.Add(users);
                    await _context.SaveChangesAsync();

                    HttpContext.Session.Set<string>("user", users.Email);
                    UserSession session = new UserSession
                    {
                        Email = testUser.Email,
                        FirstName = testUser.FirstName,
                        LastName = testUser.LastName,
                        Birthday = testUser.Birthday,
                        AccountType = testUser.AccountType
                    };

                    ViewData["Message"] = session;

                    return View("~/Views/Home/Index.cshtml");
                }
            }
           
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login([Bind("Email,Password")] UserValidationLogin testLogin)
        {
            if (ModelState.IsValid)
            {
                User userFound;
                if (_context.Users.Count(y => y.Email == testLogin.Email) == 1)
                {
                    userFound = _context.Users.Where(e => e.Email == testLogin.Email).Single();
                }
                else
                {
                    Errors failLogin = new Errors
                    {
                        LoginError = "Invalid Email/Password"
                    };
                    ViewData["LogErr"] = failLogin;
                    return View();
                }
                if (userFound.Password == Sha256(testLogin.Password))
                {

                    HttpContext.Session.Set<string>("user", userFound.Email);
                    UserSession session = new UserSession
                    {
                        Email = userFound.Email,
                        FirstName = userFound.FirstName,
                        LastName = userFound.LastName,
                        Birthday = userFound.Birthday,
                        AccountType = userFound.AccountType
                    };
                    ViewData["Message"] = session;
                    return View("~/Views/Home/Index.cshtml");
                }
            }
            Errors fail = new Errors
            {
                LoginError = "Invalid Email/Password"
            };
            ViewData["LogErr"]= fail;
            return View();
        } 

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public static String Sha256(String value)
        {
            StringBuilder hash = new StringBuilder();

            SHA256 security = SHA256.Create();
            Encoding enc = Encoding.UTF8;
            Byte[] result = security.ComputeHash(enc.GetBytes(value));

            foreach (byte b in result)
                hash.Append(b.ToString());

            return hash.ToString();
        }
    }
}
