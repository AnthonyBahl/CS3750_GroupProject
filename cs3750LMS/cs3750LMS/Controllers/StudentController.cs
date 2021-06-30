using cs3750LMS.DataAccess;
using cs3750LMS.Models;
using cs3750LMS.Models.entites;
using cs3750LMS.Models.general;
using cs3750LMS.Models.validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

        //--------------------------View Course logic/ submit assignment start
        [HttpGet]
        public IActionResult ViewCourse(int id)
        {
            //grab the specific course
            string courseKey = "course" + id;

            //see if already exists in session, if not grab data and store to session
            string serialSelected = HttpContext.Session.GetString(courseKey);
            SpecificCourse course = new SpecificCourse();
            if (serialSelected != null)
            {
                course = JsonSerializer.Deserialize<SpecificCourse>(serialSelected);
            }
            else
            {
                string serialCourse = HttpContext.Session.GetString("userCourses");
                Courses userCourses = serialCourse == null ? null : JsonSerializer.Deserialize<Courses>(serialCourse);

                string serialAssignment = HttpContext.Session.GetString("userAssignments");
                Assignments userAssignments = serialAssignment == null ? null : JsonSerializer.Deserialize<Assignments>(serialAssignment);

                course.Selection = userCourses.CourseList.Where(x => x.CourseID == id).Single();
                course.AssignmentList = userAssignments.AssignmentList.Where(y => y.CourseID == id).ToList();

                HttpContext.Session.SetString(courseKey, JsonSerializer.Serialize(course));
            }

            //get user info from session
            string serialUser = HttpContext.Session.GetString("userInfo");
            UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);

            //get submissions
            string serialSubmissions = HttpContext.Session.GetString("userSubmissions");
            List<Submission> submissions = JsonSerializer.Deserialize<List<Submission>>(serialSubmissions);

            ViewData["Submission"] = submissions;
            ViewData["ClickedCourse"] = course;
            ViewData["Message"] = session;

            return View("~/Views/Student/ViewCourse.cshtml");
        }

        [HttpGet]
        public IActionResult SubmitAssignment (int id)
        {

            //get user info from session
            string serialUser = HttpContext.Session.GetString("userInfo");
            UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);

            //get submissions
            string serialSubmissions = HttpContext.Session.GetString("userSubmissions");
            List<Submission> submissions = JsonSerializer.Deserialize<List<Submission>>(serialSubmissions);

            //get assignments
            string serialAssignment = HttpContext.Session.GetString("userAssignments");
            Assignments userAssignments = serialAssignment == null ? null : JsonSerializer.Deserialize<Assignments>(serialAssignment);

            Assignment clickedAssignment = userAssignments.AssignmentList.Where(x => x.AssignmentID == id).Single();


            ViewData["currentCourse"] = clickedAssignment.CourseID;
            ViewData["Submission"] = submissions;
            ViewData["ClickedAssignment"] = clickedAssignment;
            ViewData["Message"] = session;

            return View("~/Views/Student/SubmitAssignment.cshtml");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TextSubmit([Bind("TextSubmission, CourseId, AssignmentId")] SubmitAssignmentValidation submiting)
        {
            //get user info from session
            string serialUser = HttpContext.Session.GetString("userInfo");
            UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);
            if (ModelState.IsValid)
            {
                //create new submission
                Submission newSubmission = new Submission
                {
                    AssignmentID = submiting.AssignmentId,
                    StudentID = session.UserId,
                    SubmissionDate = DateTime.Now,
                    SubmissionType = 1,
                    Grade = -1,
                    Contents = submiting.TextSubmission
                };

                //save to database
                _context.Submissions.Add(newSubmission);
                _context.SaveChanges();

                //get submissions, add new, and save to session
                string serialSubmissions = HttpContext.Session.GetString("userSubmissions");
                List<Submission> submissions = JsonSerializer.Deserialize<List<Submission>>(serialSubmissions);

                submissions.Add(newSubmission);

                HttpContext.Session.SetString("userSubmissions", JsonSerializer.Serialize(submissions));

            }
            else
            {
                return SubmitAssignment(submiting.AssignmentId);
            }
            return ViewCourse(submiting.CourseId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FileSubmit([Bind("FileSubmission,CourseId,AssignmentId")]SubmitAssignmentValidation submiting)
        {
            return ViewCourse(submiting.CourseId);
        }
        //--------------------------View Course logic/submit assignment end
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SearchCourses([Bind("Department,Title")] SearchValidation pars)
        {
            //get user info from session
            string serialUser = HttpContext.Session.GetString("userInfo");
            UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);
            //grab session vars for registration
            string serialEnrollment = HttpContext.Session.GetString("userEnrollment");
            string serialAllCourses = HttpContext.Session.GetString("AllCourses");
            string serialTimesAll = HttpContext.Session.GetString("allCourseTimes");

            Courses allCourses;
            Enrollments enrollment;

            if (serialEnrollment != null && serialAllCourses != null && serialTimesAll != null)
            {
                allCourses = JsonSerializer.Deserialize<Courses>(serialAllCourses);
                enrollment = JsonSerializer.Deserialize<Enrollments>(serialEnrollment);
                //reload timespans

                List<TimeStamp> timesAll = JsonSerializer.Deserialize<List<TimeStamp>>(serialTimesAll);
                allCourses.RefactorTimeSpans(timesAll);
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

                allCourses = new Courses
                {
                    CourseList = _context.Courses.ToList(),
                    CourseInstructors = instructors.InstructorList
                };
                //save times
                List<TimeStamp> timesSaveA = new TimeStamp().ParseTimes(allCourses);
                HttpContext.Session.SetString("allCourseTimes", JsonSerializer.Serialize(timesSaveA));

                //save to session
                HttpContext.Session.SetString("userEnrollment", JsonSerializer.Serialize(enrollment));
                HttpContext.Session.SetString("AllCourses", JsonSerializer.Serialize(allCourses));
            }



            //get student courses from session
            string serialCourse = HttpContext.Session.GetString("userCourses");
            Courses studentCourses = serialCourse == null ? null : JsonSerializer.Deserialize<Courses>(serialCourse);
            //reload timespans
            string serialTimes = HttpContext.Session.GetString("courseTimes");
            List<TimeStamp> times = JsonSerializer.Deserialize<List<TimeStamp>>(serialTimes);
            studentCourses.RefactorTimeSpans(times);

            List<Course> removeList = new List<Course>();
            //filter data
            if(pars.Department != -1)
            {
                for(int i = 0; i < allCourses.CourseList.Count; i++)
                {
                    if(allCourses.CourseList[i].Department != pars.Department)
                    {
                        removeList.Add(allCourses.CourseList[i]);
                    }
                }
            }
            if(pars.Title != null && pars.Title != string.Empty)
            {
                for(int i = 0; i < allCourses.CourseList.Count; i++)
                {
                    if (!allCourses.CourseList[i].ClassTitle.Contains(pars.Title))
                    {
                        removeList.Add(allCourses.CourseList[i]);
                    }
                }
            }

            foreach(Course c in removeList)
            {
                allCourses.CourseList.Remove(c);
            }

            //if departments were grabbed before are saved in session else put in session
            string serialDepts = HttpContext.Session.GetString("Departments");
            Departments depts;
            if (serialDepts != null)
            {
                depts = JsonSerializer.Deserialize<Departments>(serialDepts);
            }
            else
            {
                depts = new Departments
                {
                    DeptsList = _context.Departments.ToList()
                };
            }
            ViewData["Departments"] = depts;
            //pass data to view
            ViewData["Message"] = session;
            ViewData["Courses"] = allCourses;
            ViewData["StudentCourses"] = studentCourses;
            
            return View("~/Views/Student/Register.cshtml");
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
                    string serialTimesAll = HttpContext.Session.GetString("allCourseTimes");

                    Courses allCourses;
                    Enrollments enrollment;

                    if (serialEnrollment != null && serialAllCourses != null && serialTimesAll != null)
                    {
                        allCourses = JsonSerializer.Deserialize<Courses>(serialAllCourses);
                        enrollment = JsonSerializer.Deserialize<Enrollments>(serialEnrollment);
                        //reload timespans

                        List<TimeStamp> timesAll = JsonSerializer.Deserialize<List<TimeStamp>>(serialTimesAll);
                        allCourses.RefactorTimeSpans(timesAll);
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

                        allCourses = new Courses
                        {
                            CourseList = _context.Courses.ToList(),
                            CourseInstructors = instructors.InstructorList
                        };
                        //save times
                        List<TimeStamp> timesSaveA = new TimeStamp().ParseTimes(allCourses);
                        HttpContext.Session.SetString("allCourseTimes", JsonSerializer.Serialize(timesSaveA));

                        //save to session
                        HttpContext.Session.SetString("userEnrollment", JsonSerializer.Serialize(enrollment));
                        HttpContext.Session.SetString("AllCourses", JsonSerializer.Serialize(allCourses));
                    }
                


                    //get student courses from session
                    string serialCourse = HttpContext.Session.GetString("userCourses");
                    Courses studentCourses = serialCourse == null ? null : JsonSerializer.Deserialize<Courses>(serialCourse);
                    //reload timespans
                    string serialTimes = HttpContext.Session.GetString("courseTimes");
                    List<TimeStamp> times = JsonSerializer.Deserialize<List<TimeStamp>>(serialTimes);
                    studentCourses.RefactorTimeSpans(times);
                    //if departments were grabbed before are saved in session else put in session
                    string serialDepts = HttpContext.Session.GetString("Departments");
                    Departments depts;
                    if (serialDepts != null)
                    {
                        depts = JsonSerializer.Deserialize<Departments>(serialDepts);
                    }
                    else
                    {
                        depts = new Departments
                        {
                            DeptsList = _context.Departments.ToList()
                        };
                    }
                    ViewData["Departments"] = depts;
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
        [ValidateAntiForgeryToken]
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
            string serialTimesAll = HttpContext.Session.GetString("allCourseTimes");
            List<TimeStamp> timesAll = JsonSerializer.Deserialize<List<TimeStamp>>(serialTimesAll);
            allCourses.RefactorTimeSpans(timesAll);

            //get student courses from session
            string serialCourse = HttpContext.Session.GetString("userCourses");
            Courses studentCourses = serialCourse == null ? null : JsonSerializer.Deserialize<Courses>(serialCourse);
            //reload timespans
            string serialTimes = HttpContext.Session.GetString("courseTimes");
            List<TimeStamp> times = JsonSerializer.Deserialize<List<TimeStamp>>(serialTimes);
            studentCourses.RefactorTimeSpans(times);

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

            //reload courses
            List<int> enrolled = _context.Enrollments.Where(y => y.studentID == session.UserId).Select(z => z.courseID).ToList();
            studentCourses.CourseList = _context.Courses.Where(x => enrolled.Contains(x.CourseID)).ToList();
            HttpContext.Session.SetString("userCourses", JsonSerializer.Serialize(studentCourses));
            //reload assignments
            Assignments userAssignments = JsonSerializer.Deserialize<Assignments>(HttpContext.Session.GetString("userAssignments"));
            userAssignments.AssignmentList= _context.Assignments.Where(x => enrolled.Contains(x.CourseID)).ToList();
            HttpContext.Session.SetString("userAssignments", JsonSerializer.Serialize(userAssignments));
            //save times
            List<TimeStamp> timesSave = new TimeStamp().ParseTimes(studentCourses);
            HttpContext.Session.SetString("courseTimes", JsonSerializer.Serialize(timesSave));


            //set courses object, and success for next pass
            if (success)
            {
                session.ClassState = 0;
            }
            else
            {
                session.ClassState = 1;
            }
            //if departments were grabbed before are saved in session else put in session
            string serialDepts = HttpContext.Session.GetString("Departments");
            Departments depts;
            if (serialDepts != null)
            {
                depts = JsonSerializer.Deserialize<Departments>(serialDepts);
            }
            else
            {
                depts = new Departments
                {
                    DeptsList = _context.Departments.ToList()
                };
            }
            ViewData["Departments"] = depts;
            //pass data to view
            ViewData["Message"] = session;
            ViewData["Courses"] = allCourses;
            ViewData["StudentCourses"] = studentCourses;
            return View("~/Views/Student/Register.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
            string serialTimesAll = HttpContext.Session.GetString("allCourseTimes");
            List<TimeStamp> timesAll = JsonSerializer.Deserialize<List<TimeStamp>>(serialTimesAll);
            allCourses.RefactorTimeSpans(timesAll);

            //get student courses from session
            string serialCourse = HttpContext.Session.GetString("userCourses");
            Courses studentCourses = serialCourse == null ? null : JsonSerializer.Deserialize<Courses>(serialCourse);
            //reload timespans
            string serialTimes = HttpContext.Session.GetString("courseTimes");
            List<TimeStamp> times = JsonSerializer.Deserialize<List<TimeStamp>>(serialTimes);
            studentCourses.RefactorTimeSpans(times);

            //if model valid add new course
            bool success = false;

            Enrollment DropCourse = (Enrollment)_context.Enrollments.Where(x => (x.studentID == session.UserId && x.courseID == passedEnrollment.courseID)).Single();
            _context.Enrollments.Remove(DropCourse);
            _context.SaveChanges();
            success = true;

            enrollment.EnrollmentList = _context.Enrollments.Where(x => x.studentID == session.UserId).ToList();
            HttpContext.Session.SetString("userEnrollment", JsonSerializer.Serialize(enrollment));

            //reload courses
            List<int> enrolled = _context.Enrollments.Where(y => y.studentID == session.UserId).Select(z => z.courseID).ToList();
            studentCourses.CourseList = _context.Courses.Where(x => enrolled.Contains(x.CourseID)).ToList();
            HttpContext.Session.SetString("userCourses", JsonSerializer.Serialize(studentCourses));
            //reload assignments
            Assignments userAssignments = JsonSerializer.Deserialize<Assignments>(HttpContext.Session.GetString("userAssignments"));
            userAssignments.AssignmentList = _context.Assignments.Where(x => enrolled.Contains(x.CourseID)).ToList();
            HttpContext.Session.SetString("userAssignments", JsonSerializer.Serialize(userAssignments));
            //save times
            List<TimeStamp> timesSave = new TimeStamp().ParseTimes(studentCourses);
            HttpContext.Session.SetString("courseTimes", JsonSerializer.Serialize(timesSave));

            //set courses object, and success for next pass
            if (success)
            {
                session.ClassState = 0;
            }
            else
            {
                session.ClassState = 1;
            }
            //if departments were grabbed before are saved in session else put in session
            string serialDepts = HttpContext.Session.GetString("Departments");
            Departments depts;
            if (serialDepts != null)
            {
                depts = JsonSerializer.Deserialize<Departments>(serialDepts);
            }
            else
            {
                depts = new Departments
                {
                    DeptsList = _context.Departments.ToList()
                };
            }
            ViewData["Departments"] = depts;
            ViewData["Message"] = session;
            ViewData["Courses"] = allCourses;
            ViewData["StudentCourses"] = studentCourses;
            return View("~/Views/Student/Register.cshtml");
        }

        [HttpGet]
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

                    // Transactions
                    string serialTransactions = HttpContext.Session.GetString("userTransactions");
                    Transactions userTransactions = serialTransactions == null ? null : JsonSerializer.Deserialize<Transactions>(serialTransactions);

                    ViewData["UserTransactions"] = userTransactions;
                    ViewData["Message"] = session;
                    ViewData["Courses"] = allCourses;
                    ViewData["StudentCourses"] = studentCourses;
                    return View();
                }
            }
            return View("~/Views/Home/Login.cshtml");
        }

        // POST: 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PaymentAsync(string ccname, string ccnum, string ccmonth, string ccyear, string cccvv, string amt)
        {
            if (HttpContext.Session.Get<string>("user") != null)
            {
                //get user info from session
                string serialUser = HttpContext.Session.GetString("userInfo");
                UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);

                // Is a Student
                if (session.AccountType == 0)
                {
                    //grab session vars for registration
                    string serialEnrollment = HttpContext.Session.GetString("userEnrollment");
                    string serialAllCourses = HttpContext.Session.GetString("AllCourses");

                    Courses allCourses;
                    Enrollments enrollment;

                    // Check to see if info was found in the session
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

                    // Payment request to stripe
                    HttpClient client = new HttpClient();
                    string url = "https://api.stripe.com/v1/tokens";

                    string ccErrMsg = "";

                    client.BaseAddress = new Uri(url); 
                    client.DefaultRequestHeaders.Accept.Clear(); // clear preexisting headers

                    // set headers
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "sk_test_51J1K0xA6qDyGoLeeC6aj7Rm39c8lFFfTYZ9k4KyAy6oxH30YYJEKbE73mewvQAAc0jkzATCmsOuzwZ7pZ42bXBSc00t8sYSK1X");

                    // defind cc data
                    var bodyData = new Dictionary<string, string>
                    {
                        { "card[number]",  ccnum },
                        { "card[exp_month]", ccmonth },
                        { "card[exp_year]", ccyear },
                        { "card[cvc]", ccyear }
                    };

                    // encode data
                    var content = new FormUrlEncodedContent(bodyData);

                    // make request
                    var res = await client.PostAsync(client.BaseAddress, content);
                    if (res.IsSuccessStatusCode)
                    {
                        var responseString = await res.Content.ReadAsStringAsync();
                       
                        // parse json response
                        JsonDocument doc = JsonDocument.Parse(responseString);
                        // grab root json element
                        JsonElement root = doc.RootElement;
                        // grab token id field
                        string tokenId = root.GetProperty("id").ToString();
                        
                        // do next request

                        client = new HttpClient();
                        url = "https://api.stripe.com/v1/charges";

                        client.BaseAddress = new Uri(url);
                        client.DefaultRequestHeaders.Accept.Clear(); // clear preexisting headers

                        // set headers
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "sk_test_51J1K0xA6qDyGoLeeC6aj7Rm39c8lFFfTYZ9k4KyAy6oxH30YYJEKbE73mewvQAAc0jkzATCmsOuzwZ7pZ42bXBSc00t8sYSK1X");
                      
                        // calculate ammount in dollars
                        int iAmt = Int32.Parse(amt);
                        string dollarAmt = (iAmt * 100).ToString();

                        // defind second request data 
                        bodyData = new Dictionary<string, string>
                        {
                            { "amount", dollarAmt },
                            { "currency", "usd"  },
                            { "source",  tokenId },
                            { "description", ccname + "'s Payment" }
                        };

                        // encode data
                        content = new FormUrlEncodedContent(bodyData);

                        // make request
                        var chargesRes = await client.PostAsync(client.BaseAddress, content);
                        if (chargesRes.IsSuccessStatusCode)
                        {
                            // Charges response from the request 
                            var chargesResString = await chargesRes.Content.ReadAsStringAsync();
                            JsonDocument chargesDoc = JsonDocument.Parse(chargesResString);
                            JsonElement chargesRoot = chargesDoc.RootElement;

                            // Parse the amount string and convert it to an int
                            int iChargeAmount;
                            int.TryParse(chargesRoot.GetProperty("amount").ToString(), out iChargeAmount);

                            // Create transaction object
                            Transaction newTransaction = new Transaction
                            {
                                Date = DateTime.Now,
                                userID = session.UserId,
                                amount = iChargeAmount,
                                status = "Settled"
                            };

                            //add the new transaction to the database and save changes
                            _context.Add(newTransaction);
                            await _context.SaveChangesAsync();

                            // Update Session
                            Transactions newUserTransactions = new Transactions();
                            newUserTransactions.TransactionList = _context.Transactions.Where(t => t.userID == session.UserId).ToList();
                            HttpContext.Session.SetString("userTransactions", JsonSerializer.Serialize(newUserTransactions));
                        }
                        else
                        {
                            ccErrMsg = "Payment not processed, Check your card inputs";
                        }
                    }
                    else
                    {
                        ccErrMsg = "Payment not processed, Check your card inputs";
                    }

                    //get student courses from session
                    string serialCourse = HttpContext.Session.GetString("userCourses");
                    Courses studentCourses = serialCourse == null ? null : JsonSerializer.Deserialize<Courses>(serialCourse);

                    // Transactions
                    string serialTransactions = HttpContext.Session.GetString("userTransactions");
                    Transactions userTransactions = serialTransactions == null ? null : JsonSerializer.Deserialize<Transactions>(serialTransactions);

                    ViewData["UserTransactions"] = userTransactions;
                    ViewData["CCMessage"] = ccErrMsg;
                    ViewData["Message"] = session;
                    ViewData["Courses"] = allCourses;
                    ViewData["StudentCourses"] = studentCourses;
                    return View("Payment");
                }
            }
            return View("~/Views/Home/Login.cshtml");
        }
    }
}
