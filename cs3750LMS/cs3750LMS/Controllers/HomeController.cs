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
using System.Text.Json;
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
                string serialUser = HttpContext.Session.GetString("userInfo");
                UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);

                string serialCourse = HttpContext.Session.GetString("userCourses");
                Courses userCourses = serialCourse == null ? null : JsonSerializer.Deserialize<Courses>(serialCourse);

                string serialAssignment = HttpContext.Session.GetString("userAssignments");
                Assignments userAssignments = serialAssignment == null ? null : JsonSerializer.Deserialize<Assignments>(serialAssignment);

                //reload timespans
                string serialTimes = HttpContext.Session.GetString("courseTimes");
                List<TimeStamp> times = JsonSerializer.Deserialize<List<TimeStamp>>(serialTimes);
                userCourses.RefactorTimeSpans(times);

                ViewData["UserCourses"] = userCourses;
                ViewData["Message"] = session;
                ViewData["userAssignments"] = userAssignments;
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
            if (ModelState.IsValid)
            {
                //See if email already exists, if not create user
                if (_context.Users.Count(e => e.Email == testUser.Email) == 0)
                {
                    //create the new user
                    User users = new Models.User
                    {
                        Email = testUser.Email,
                        FirstName = testUser.FirstName,
                        LastName = testUser.LastName,
                        Birthday = testUser.Birthday,
                        Password = Sha256(testUser.Password),
                        AccountType = testUser.AccountType
                    };

                    //add the new user to the database and save changes
                    _context.Add(users);
                    await _context.SaveChangesAsync();

                    //set user email in session, and various info as a session object in session
                    HttpContext.Session.Set<string>("user", users.Email);
                    UserSession session = new UserSession
                    {
                        UserId = _context.Users.Where(x => x.Email == users.Email).Select(y => y.UserId).Single(),
                        Email = users.Email,
                        FirstName = users.FirstName,
                        LastName = users.LastName,
                        Birthday = users.Birthday,
                        AccountType = users.AccountType,
                        ProfileImage = users.ProfileImage,
                        Address1 = users.Address1,
                        Address2 = users.Address2,
                        City = users.City,
                        State = users.State,
                        Zip = users.Zip,
                        Phone = users.Phone,
                        LinkedIn = users.LinkedIn,
                        Github = users.Github,
                        Twitter = users.Twitter,
                        Bio = users.Bio
                    };
                    HttpContext.Session.SetString("userInfo", JsonSerializer.Serialize(session));
                    ViewData["Message"] = session;

                    //save user courses(empty) to session and pass into view data
                    Courses userCourses = new Courses();
                    userCourses.CourseList = new List<Course>();
                    HttpContext.Session.SetString("userCourses", JsonSerializer.Serialize(userCourses));
                    ViewData["UserCourses"] = userCourses;

                    // set an empty set of user assignments and pass into the view data
                    Assignments userAssignments = new Assignments();
                    userAssignments.AssignmentList = new List<Assignment>();
                    HttpContext.Session.SetString("userAssignments", JsonSerializer.Serialize(userAssignments));
                    ViewData["userAssignments"] = userAssignments;

                    //save times
                    List<TimeStamp> times = new TimeStamp().ParseTimes(userCourses);
                    HttpContext.Session.SetString("courseTimes", JsonSerializer.Serialize(times));

                    //on success is now logged in, route to dashboard
                    return View("~/Views/Home/Index.cshtml");
                }
            }

            //on failure route to sign-up
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login([Bind("Email,Password")] UserValidationLogin testLogin)
        {
            if (ModelState.IsValid)
            {
                //grab the user from the database
                User userFound;
                if (_context.Users.Count(y => y.Email == testLogin.Email) == 1)
                {
                    userFound = _context.Users.Where(e => e.Email == testLogin.Email).Single();
                }
                else
                {
                    // user not found send failure
                    Errors failLogin = new Errors
                    {
                        LoginError = "Invalid Email/Password"
                    };

                    ViewData["LogErr"] = failLogin;
                    return View();
                }

                //check password to entered password
                if (userFound.Password == Sha256(testLogin.Password))
                {
                    //store user email and info in session
                    HttpContext.Session.Set<string>("user", userFound.Email);
                    UserSession session = new UserSession
                    {
                        UserId = userFound.UserId,
                        Email = userFound.Email,
                        FirstName = userFound.FirstName,
                        LastName = userFound.LastName,
                        Birthday = userFound.Birthday,
                        AccountType = userFound.AccountType,
                        ProfileImage = userFound.ProfileImage,
                        Address1 = userFound.Address1,
                        Address2 = userFound.Address2,
                        City = userFound.City,
                        State = userFound.State,
                        Zip = userFound.Zip,
                        Phone = userFound.Phone,
                        LinkedIn = userFound.LinkedIn,
                        Github = userFound.Github,
                        Twitter = userFound.Twitter,
                        Bio = userFound.Bio
                    };
                    HttpContext.Session.SetString("userInfo", JsonSerializer.Serialize(session));
                    ViewData["Message"] = session;

                    //grab user courses from database
                    Courses userCourses = new Courses();
                    Assignments userAssignments = new Assignments();

                    //Student course list
                    if (session.AccountType == 0)
                    {
                        List<int> enrolled = _context.Enrollments.Where(y => y.studentID == userFound.UserId).Select(z => z.courseID).ToList();
                        userCourses.CourseList = _context.Courses.Where(x => enrolled.Contains(x.CourseID)).ToList();

                        userAssignments.AssignmentList = _context.Assignments.Where(x => enrolled.Contains(x.CourseID)).OrderBy(y => y.DueDate).ToList();
                    }
                    //Instructor course list
                    if (session.AccountType == 1)
                    {
                        userCourses.CourseList = _context.Courses.Where(x => x.InstructorID == userFound.UserId).ToList();
                        List<int> teaching = userCourses.CourseList.Select(y => y.CourseID).ToList();
                        userAssignments.AssignmentList = _context.Assignments.Where(x => teaching.Contains(x.CourseID)).OrderBy(y => y.DueDate).ToList();
                    }

                    //save the user courses in the session and pass to view
                    HttpContext.Session.SetString("userCourses", JsonSerializer.Serialize(userCourses));
                    ViewData["UserCourses"] = userCourses;

                    // save the user assignments in the sessioin and pass to the view
                    HttpContext.Session.SetString("userAssignments", JsonSerializer.Serialize(userAssignments));
                    ViewData["userAssignments"] = userAssignments;

                    //grab transactions from database
                    //check if student account
                    if (session.AccountType == 0)
                    {
                        // Variable to hold transactions
                        Transactions userTransactions = new Transactions();

                        // Get data from database
                        userTransactions.TransactionList = _context.Transactions.Where(t => t.userID == userFound.UserId).ToList();

                        HttpContext.Session.SetString("userTransactions", JsonSerializer.Serialize(userTransactions));
                    }

                    //save times
                    List<TimeStamp> times = new TimeStamp().ParseTimes(userCourses);
                    HttpContext.Session.SetString("courseTimes", JsonSerializer.Serialize(times));

                    //on success is logged in route to dashboard
                    return View("~/Views/Home/Index.cshtml");
                }
            }

            //on failure route to login and pass error message
            Errors fail = new Errors
            {
                LoginError = "Invalid Email/Password"
            };
            ViewData["LogErr"] = fail;
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