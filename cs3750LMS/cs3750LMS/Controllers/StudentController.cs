using cs3750LMS.DataAccess;
using cs3750LMS.Models;
using cs3750LMS.Models.entites;
using cs3750LMS.Models.general;
using cs3750LMS.Models.Repository;
using cs3750LMS.Models.validation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
        private IHostingEnvironment Environment;
        private readonly INotificationRepository _notification;
        private PublicController publicController;
        public StudentController(cs3750Context context, IHostingEnvironment _environment, INotificationRepository notification)
        {
            _context = context;
            Environment = _environment;
            _notification = notification;
            publicController = new PublicController(_context, _environment, _notification);
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
            if (submissions.Count(x=>x.AssignmentID == clickedAssignment.AssignmentID) >0 )
            {
                ViewData["AlreadySubmitted"] = true;
                ViewData["AssignmentStats"] = GetAssignmentStats(clickedAssignment);
            }
            else
            {
                ViewData["AlreadySubmitted"] = false;
            }
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



                //getCourses for notification
                string serialCourse = HttpContext.Session.GetString("userCourses");
                Courses userCourses = serialCourse == null ? null : JsonSerializer.Deserialize<Courses>(serialCourse);

                //get Assignments for notification
                string serialAssignment = HttpContext.Session.GetString("userAssignments");
                Assignments courseAssignments = serialAssignment == null ? null : JsonSerializer.Deserialize<Assignments>(serialAssignment);


                //Need these to create the message for the notification. 
                int courseID = submiting.CourseId;
                String CourseName = userCourses.CourseList.Where(c => c.CourseID == courseID).Select(v => v.ClassTitle).FirstOrDefault();
                String AssignmentName = courseAssignments.AssignmentList.Where(a => a.AssignmentID == submiting.AssignmentId).Select(x => x.Title).FirstOrDefault();
                int InstructorID = userCourses.CourseList.Where(c => c.CourseID == courseID).Select(i => i.InstructorID).FirstOrDefault();
                String notiMessage = CourseName + " | " + AssignmentName + " was submitted.";

                publicController.CreateNotification(InstructorID, courseID, "Assignment", notiMessage);

            }
            else //fail case
            {
                return SubmitAssignment(submiting.AssignmentId);
            }
            return ViewCourse(submiting.CourseId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FileSubmit([Bind("FileSubmission,CourseId,AssignmentId")]SubmitAssignmentValidation submiting)
        {
            //get user info from session
            string serialUser = HttpContext.Session.GetString("userInfo");
            UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);
            if (ModelState.IsValid)
            {
                //file storage   stored in file assignment id directory first, and student id second, with the file inside                 
                string wwwPath = this.Environment.WebRootPath;
                string contentPath = this.Environment.ContentRootPath;
                string path = Path.Combine(this.Environment.WebRootPath, "Submissions/" + submiting.AssignmentId + "/"+session.UserId);

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string dbPath = Path.GetFileName(submiting.FileSubmission.FileName);                   //name of file, could save to db as well
                string FullPath = Path.Combine(path, dbPath);                               //save to database for later reference


                //add to files
                using (FileStream stream = new FileStream(FullPath, FileMode.Create))
                {
                    submiting.FileSubmission.CopyTo(stream);
                }


                //create new submission
                Submission newSubmission = new Submission
                {
                    AssignmentID = submiting.AssignmentId,
                    StudentID = session.UserId,
                    SubmissionDate = DateTime.Now,
                    SubmissionType = 0,
                    Grade = -1,
                    //Contents = "/Submissions/" + submiting.AssignmentId + "/" + session.UserId + "/" + submiting.FileSubmission.FileName //save file path to database
                    Contents = submiting.FileSubmission.FileName
                };

                //save to database
                _context.Submissions.Add(newSubmission);
                _context.SaveChanges();

                //get submissions, add new, and save to session
                string serialSubmissions = HttpContext.Session.GetString("userSubmissions");
                List<Submission> submissions = JsonSerializer.Deserialize<List<Submission>>(serialSubmissions);

                submissions.Add(newSubmission);

                HttpContext.Session.SetString("userSubmissions", JsonSerializer.Serialize(submissions));


                //getCourses for notification
                string serialCourse = HttpContext.Session.GetString("userCourses");
                Courses userCourses = serialCourse == null ? null : JsonSerializer.Deserialize<Courses>(serialCourse);

                //get Assignments for notification
                string serialAssignment = HttpContext.Session.GetString("userAssignments");
                Assignments courseAssignments = serialAssignment == null ? null : JsonSerializer.Deserialize<Assignments>(serialAssignment);


                //Need these to create the message for the notification. 
                int courseID = submiting.CourseId;
                String CourseName = userCourses.CourseList.Where(c => c.CourseID == courseID).Select(v => v.ClassTitle).FirstOrDefault();
                String AssignmentName = courseAssignments.AssignmentList.Where(a => a.AssignmentID == submiting.AssignmentId).Select(x => x.Title).FirstOrDefault();
                int InstructorID = userCourses.CourseList.Where(c => c.CourseID == courseID).Select(i => i.InstructorID).FirstOrDefault();
                String notiMessage = CourseName + " | " + AssignmentName + " was submitted.";

                publicController.CreateNotification(InstructorID, courseID, "Assignment", notiMessage);

            }
            else //fail case
            {
                return SubmitAssignment(submiting.AssignmentId);
            }

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
                SIUsers instructors = new SIUsers
                {
                    SIUusers = _context.Users.Where(x => x.AccountType == 1).ToList(),
                    SIUserList = new List<SIUser>()
                };

                for (int i = 0; i < instructors.SIUusers.Count; i++)
                {
                    SIUser newInstructor = new SIUser();
                    newInstructor.UserId = instructors.SIUusers[i].UserId;
                    newInstructor.FirstName = instructors.SIUusers[i].FirstName;
                    newInstructor.LastName = instructors.SIUusers[i].LastName;
                    instructors.SIUserList.Add(newInstructor);
                }

                allCourses = new Courses
                {
                    CourseList = _context.Courses.ToList(),
                    CourseInstructors = instructors.SIUserList
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
                for (int i = 0; i < allCourses.CourseList.Count; i++)
                {
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                    string title = textInfo.ToTitleCase(pars.Title);
                    if (!allCourses.CourseList[i].ClassTitle.Contains(title) && !allCourses.CourseList[i].ClassTitle.Contains(pars.Title.ToUpper()) && !allCourses.CourseList[i].ClassTitle.Contains(pars.Title.ToLower()))
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
                        //reload time spans

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
                        SIUsers instructors = new SIUsers
                        {
                            SIUusers = _context.Users.Where(x => x.AccountType == 1).ToList(),
                            SIUserList = new List<SIUser>()
                        };

                        for (int i = 0; i < instructors.SIUusers.Count; i++)
                        {
                            SIUser newInstructor = new SIUser();
                            newInstructor.UserId = instructors.SIUusers[i].UserId;
                            newInstructor.FirstName = instructors.SIUusers[i].FirstName;
                            newInstructor.LastName = instructors.SIUusers[i].LastName;
                            instructors.SIUserList.Add(newInstructor);
                        }

                        allCourses = new Courses
                        {
                            CourseList = _context.Courses.ToList(),
                            CourseInstructors = instructors.SIUserList
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
                    //reload time spans
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
                        SIUsers instructors = new SIUsers
                        {
                            SIUusers = _context.Users.Where(x => x.AccountType == 1).ToList(),
                            SIUserList = new List<SIUser>()
                        };

                        for (int i = 0; i < instructors.SIUusers.Count; i++)
                        {
                            SIUser newInstructor = new SIUser();
                            newInstructor.UserId = instructors.SIUusers[i].UserId;
                            newInstructor.FirstName = instructors.SIUusers[i].FirstName;
                            newInstructor.LastName = instructors.SIUusers[i].LastName;
                            instructors.SIUserList.Add(newInstructor);
                        }

                        //grab all courses
                        allCourses = new Courses
                        {
                            CourseList = _context.Courses.ToList(),
                            CourseInstructors = instructors.SIUserList
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
                        SIUsers instructors = new SIUsers
                        {
                            SIUusers = _context.Users.Where(x => x.AccountType == 1).ToList(),
                            SIUserList = new List<SIUser>()
                        };

                        for (int i = 0; i < instructors.SIUusers.Count; i++)
                        {
                            SIUser newInstructor = new SIUser();
                            newInstructor.UserId = instructors.SIUusers[i].UserId;
                            newInstructor.FirstName = instructors.SIUusers[i].FirstName;
                            newInstructor.LastName = instructors.SIUusers[i].LastName;
                            instructors.SIUserList.Add(newInstructor);
                        }

                        //grab all courses
                        allCourses = new Courses
                        {
                            CourseList = _context.Courses.ToList(),
                            CourseInstructors = instructors.SIUserList
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
                      
                        // calculate amount in dollars
                        int iAmt = Int32.Parse(amt);
                        string dollarAmt = (iAmt * 100).ToString();

                        // define second request data 
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

        private AssignmentStats GetAssignmentStats(Assignment assignment)
        {
            AssignmentStats stats = new AssignmentStats();
            stats.Assignment = assignment;
            // Get Max
            List<Submission> submissions = _context.Submissions.Where(x => x.AssignmentID == assignment.AssignmentID).ToList();
            stats.SubmissionList = submissions.Where(x => x.Grade > -1).ToList();
            var query = from a in submissions
                        where a.Grade > -1
                        select a.Grade;

            stats.Max = query.Max();
            // Get Min
            stats.Min = query.Min();
            // Get Avg
            stats.Avg = query.Average();
            stats.GradeDistribution = new List<int> { 0, 0, 0, 0, 0};
            foreach(int item in query)
            {
                double percent = ((double)item / assignment.MaxPoints) * 100;

                if(percent >= 90)
                {
                    stats.GradeDistribution[0]++;
                } 
                else if (percent <= 89 && percent >= 80)
                {
                    stats.GradeDistribution[1]++;
                } 
                else if (percent <= 79 && percent >= 70)
                {
                    stats.GradeDistribution[2]++;
                }
                else if (percent <= 69 && percent >= 60)
                {
                    stats.GradeDistribution[3]++;
                }
                else if (percent <= 59 && percent > 0)
                {
                    stats.GradeDistribution[4]++;
                }
            }

            return stats;
        }
    }
}
