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
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace cs3750LMS.Controllers
{
    public class InstructorController : Controller
    {
        private readonly cs3750Context _context;
        private IHostingEnvironment Environment;
        private readonly INotificationRepository _notification;
        public InstructorController(cs3750Context context, IHostingEnvironment _environment)
        {
            _context = context;
            Environment = _environment;
        }
        public InstructorController(cs3750Context context, IHostingEnvironment _environment, INotificationRepository _notification)
        {
            _context = context;
            Environment = _environment;
            this._notification = _notification;
        }


        //-------------------------------Specific Course Edit logic Begin--------------
        [HttpGet]
        public IActionResult CourseEdit(int id)
        {
            string courseKey = "course" + id;
            string serialUser = HttpContext.Session.GetString("userInfo");
            UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);



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
                course.Selection = userCourses.CourseList.Where(x => x.CourseID == id).Single();
                course.AssignmentList = _context.Assignments.Where(y => y.CourseID == id).ToList();

                HttpContext.Session.SetString(courseKey, JsonSerializer.Serialize(course));
            }

            course.ModeSetting = 1;

            ViewData["ClickedCourse"] = course;
            ViewData["Message"] = session;
            return View("~/Views/Instructor/CourseEdit.cshtml");
        }
        //-------------------------------Course Edit Logic End----------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAssignment([Bind("CourseID,Title,Description,MaxPoints,DueDate,DueTime,SubmitType")] AssignmentValidationAdd assignment)
        {
            if (ModelState.IsValid)
            {
                Assignment newA = AddAssignmentTodb(assignment);

                string courseKey = "course" + assignment.CourseID;
                string serialSelected = HttpContext.Session.GetString(courseKey);
                SpecificCourse course = JsonSerializer.Deserialize<SpecificCourse>(serialSelected);
                course.AssignmentList.Add(newA);
                //set assignments object for next pass
                string serialAssignment = HttpContext.Session.GetString("userAssignments");
                Assignments userAssignments = serialAssignment == null ? null : JsonSerializer.Deserialize<Assignments>(serialAssignment);
                //update session saved courses
                userAssignments.AssignmentList.Add(newA);
                HttpContext.Session.SetString("userAssignments", JsonSerializer.Serialize(userAssignments));
                HttpContext.Session.SetString(courseKey, JsonSerializer.Serialize(course));
            }
            return CourseEdit(assignment.CourseID);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditAssignment([Bind("CourseID,Title,Description,MaxPoints,DueDate,DueTime,SubmitType, AssignmentID")] AssignmentValidationAdd editAssignment)
        {
            Assignment _assignment = new Assignment();

            if (ModelState.IsValid)
            {
                //get the assignment from the database
                _assignment = _context.Assignments.Where(x => x.AssignmentID == editAssignment.AssignmentID).Single();

                //update database assignment with edited form assignment fields
                _assignment.CourseID = editAssignment.CourseID;
                _assignment.Title = editAssignment.Title;
                _assignment.Description = editAssignment.Description;
                _assignment.MaxPoints = editAssignment.MaxPoints;
                _assignment.DueDate = editAssignment.DueDate + editAssignment.DueTime;
                _assignment.SubmissionType = editAssignment.SubmitType;

                //Update the database to save the changes. 
                _context.SaveChanges();

                //Update the Session
                string courseKey = "course" + editAssignment.CourseID;
                string serialSelected = HttpContext.Session.GetString(courseKey);
                SpecificCourse course = JsonSerializer.Deserialize<SpecificCourse>(serialSelected);


                Assignment session_assignment = new Assignment();

                //grab the assignment that is in the session. 
                session_assignment = course.AssignmentList.Where(x => x.AssignmentID == editAssignment.AssignmentID).Single();

                //update the session assignment fields
                session_assignment.CourseID = editAssignment.CourseID;
                session_assignment.Title = editAssignment.Title;
                session_assignment.Description = editAssignment.Description;
                session_assignment.MaxPoints = editAssignment.MaxPoints;
                session_assignment.DueDate = editAssignment.DueDate + editAssignment.DueTime;
                session_assignment.SubmissionType = editAssignment.SubmitType;


                //save the session
                HttpContext.Session.SetString(courseKey, JsonSerializer.Serialize(course));

            }
            return CourseEdit(editAssignment.CourseID);
        }




        //-----------------------------Add class Logic Begin-----------------
        public IActionResult AddClass()
        {
            if (HttpContext.Session.Get<string>("user") != null)
            {
                //get the session object for next pass
                string serialUser = HttpContext.Session.GetString("userInfo");
                UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);

                if (session.AccountType == 1)
                {
                    //set courses object for next pass
                    string serialCourse = HttpContext.Session.GetString("userCourses");
                    Courses userCourses = serialCourse == null ? null : JsonSerializer.Deserialize<Courses>(serialCourse);
                    //reload time spans
                    string serialTimes = HttpContext.Session.GetString("courseTimes");
                    List<TimeStamp> times = JsonSerializer.Deserialize<List<TimeStamp>>(serialTimes);
                    userCourses.RefactorTimeSpans(times);

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
                        HttpContext.Session.SetString("Departments", JsonSerializer.Serialize(depts));
                    }

                    //pass data to view
                    ViewData["DepartmentData"] = depts;
                    ViewData["Message"] = session;
                    ViewData["Courses"] = userCourses;
                    return View();
                }
            }
            return View("~/Views/Home/Login.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddClass([Bind("Instructor,Department,ClassNumber,ClassTitle,Description,Location,Credits,Capacity,MeetDays,StartTime,EndTime,Color")] ClassValidationAdd newClass)
        {
            //get the session object for next pass
            string serialUser = HttpContext.Session.GetString("userInfo");
            UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);

            //set courses object for next pass
            string serialCourse = HttpContext.Session.GetString("userCourses");
            Courses userCourses = serialCourse == null ? null : JsonSerializer.Deserialize<Courses>(serialCourse);
            //reload time spans
            string serialTimes = HttpContext.Session.GetString("courseTimes");
            List<TimeStamp> times = JsonSerializer.Deserialize<List<TimeStamp>>(serialTimes);
            userCourses.RefactorTimeSpans(times);

            //if model valid add new course
            bool success = false;
            if (ModelState.IsValid)
            {
                // new course is the add class method below
                Course newCourse = AddClassTodb(session.UserId, newClass);
                //update session saved courses
                userCourses.CourseList.Add(newCourse);
                HttpContext.Session.SetString("userCourses", JsonSerializer.Serialize(userCourses));
                success = true;
                //save times
                List<TimeStamp> timesSave = new TimeStamp().ParseTimes(userCourses);
                HttpContext.Session.SetString("courseTimes", JsonSerializer.Serialize(timesSave));
            }
            //----------------------------Add class logic end---------------------------

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
                HttpContext.Session.SetString("Departments", JsonSerializer.Serialize(depts));
            }

            //pass data to view
            ViewData["DepartmentData"] = depts;
            ViewData["Message"] = session;
            ViewData["Courses"] = userCourses;
            return View();
        }

        // Function to add a class into to the database with the givin parameters
        public Course AddClassTodb(int UserId, ClassValidationAdd newClass)
        {
            Course newCourse = new Course
            {
                InstructorID = UserId,
                Department = newClass.Department,
                ClassNumber = newClass.ClassNumber,
                ClassTitle = newClass.ClassTitle,
                Description = newClass.Description,
                Location = newClass.Location,
                Credits = newClass.Credits,
                Capacity = newClass.Capacity,
                MeetDays = newClass.MeetDays,
                StartTime = newClass.StartTime,
                EndTime = newClass.EndTime,
                Color = newClass.Color
            };

            //update database
            _context.Courses.Add(newCourse);
            _context.SaveChanges();

            // return the new course
            return newCourse;
        }

        // Function to add a class into to the database with the givin parameters
        public Assignment AddAssignmentTodb(AssignmentValidationAdd _Assignment)
        {
            Assignment newA = new Assignment
            {
                CourseID = _Assignment.CourseID,
                Title = _Assignment.Title,
                Description = _Assignment.Description,
                MaxPoints = _Assignment.MaxPoints,
                DueDate = _Assignment.DueDate + _Assignment.DueTime,
                SubmissionType = _Assignment.SubmitType
            };

            //Add to database
            _context.Assignments.Add(newA);
            _context.SaveChanges();

            // return the new course
            return newA;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClassAsync([Bind("Instructor,Department,ClassNumber,ClassTitle,Description,Location,Credits,Capacity,MeetDays,StartTime,EndTime,Color,CourseID")] ClassValidationAdd updatedClass)
        {
            //get the session object for next pass
            string serialUser = HttpContext.Session.GetString("userInfo");
            UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);

            //set courses object for next pass
            string serialCourse = HttpContext.Session.GetString("userCourses");
            Courses userCourses = serialCourse == null ? null : JsonSerializer.Deserialize<Courses>(serialCourse);

            //reload times pans
            string serialTimes = HttpContext.Session.GetString("courseTimes");
            List<TimeStamp> times = JsonSerializer.Deserialize<List<TimeStamp>>(serialTimes);
            userCourses.RefactorTimeSpans(times);

            Course _course = new Course();
            Course session_course = new Course();
            bool success = false;
            if (ModelState.IsValid)
            {
                // Create connection to the database
                _course = _context.Courses.Where(x => x.CourseID == updatedClass.CourseID).Single();
                _course.InstructorID = session.UserId;
                _course.Department = updatedClass.Department;
                _course.ClassNumber = updatedClass.ClassNumber;
                _course.ClassTitle = updatedClass.ClassTitle;
                _course.Description = updatedClass.Description;
                _course.Location = updatedClass.Location;
                _course.Credits = updatedClass.Credits;
                _course.Capacity = updatedClass.Capacity;
                _course.MeetDays = updatedClass.MeetDays;
                _course.StartTime = updatedClass.StartTime;
                _course.EndTime = updatedClass.EndTime;
                _course.Color = updatedClass.Color;
                // Update Database
                await _context.SaveChangesAsync();

                // Update Session
                session_course = userCourses.CourseList.Where(x => x.CourseID == updatedClass.CourseID).Single();
                session_course.InstructorID = session.UserId;
                session_course.Department = updatedClass.Department;
                session_course.ClassNumber = updatedClass.ClassNumber;
                session_course.ClassTitle = updatedClass.ClassTitle;
                session_course.Description = updatedClass.Description;
                session_course.Location = updatedClass.Location;
                session_course.Credits = updatedClass.Credits;
                session_course.Capacity = updatedClass.Capacity;
                session_course.MeetDays = updatedClass.MeetDays;
                session_course.StartTime = updatedClass.StartTime;
                session_course.EndTime = updatedClass.EndTime;
                session_course.Color = updatedClass.Color;

                //update session saved courses
                HttpContext.Session.SetString("userCourses", JsonSerializer.Serialize(userCourses));

                success = true;
            }
            //----------------------------Add class logic end---------------------------

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
                HttpContext.Session.SetString("Departments", JsonSerializer.Serialize(depts));
            }

            //pass data to view
            ViewData["DepartmentData"] = depts;
            ViewData["Message"] = session;
            ViewData["Courses"] = userCourses;
            return View("AddClass");
        }

        [HttpGet]
        public IActionResult Submissions(int id)
        {
            string assignmentKey = "assignment" + id;
            string serialUser = HttpContext.Session.GetString("userInfo");
            UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);

            string serialAssignments = HttpContext.Session.GetString("userAssignments");
            Assignments courseAssignments = serialAssignments == null ? null : JsonSerializer.Deserialize<Assignments>(serialAssignments);

            SpecificAssignment assignment = new SpecificAssignment();

            if (courseAssignments.AssignmentList.Any(x => x.AssignmentID == id))
            {
                assignment.Selection = courseAssignments.AssignmentList.Where(x => x.AssignmentID == id).Single();
            }
            else
            {
                assignment.Selection = _context.Assignments.Where(x => x.AssignmentID == id).Single();
            }
            assignment.SubmissionList = _context.Submissions.Where(y => y.AssignmentID == id).ToList();
            HttpContext.Session.SetString(assignmentKey, JsonSerializer.Serialize(assignment));

            //grab enrollment
            Enrollments enrollment;
            enrollment = new Enrollments
            {
                EnrollmentList = _context.Enrollments.Where(x => x.courseID == assignment.Selection.CourseID).ToList()
            };

            //grab students
            List<User> queryList = _context.Users.Where(x => x.AccountType == 0).ToList();
            var query = from a in queryList
                        join b in enrollment.EnrollmentList on a.UserId equals b.studentID
                        select a;

            SIUsers students = new SIUsers
            {
                SIUusers = query.ToList(),
                SIUserList = new List<SIUser>()
            };

            foreach (var obj in query)
            {
                SIUser newStudent = new SIUser();
                newStudent.UserId = obj.UserId;
                newStudent.FirstName = obj.FirstName;
                newStudent.LastName = obj.LastName;
                students.SIUserList.Add(newStudent);
            }
            HttpContext.Session.SetString("courseStudents", JsonSerializer.Serialize(students));

            assignment.ModeSetting = 1;

            ViewData["ClickedAssignment"] = assignment;
            ViewData["Students"] = students;
            ViewData["Message"] = session;
            return View("~/Views/Instructor/Submissions.cshtml");
        }

        [HttpGet]
        public IActionResult SubmissionDetail(int id)
        {
            string serialUser = HttpContext.Session.GetString("userInfo");
            UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);

            string serialAssignments = HttpContext.Session.GetString("userAssignments");
            Assignments courseAssignments = serialAssignments == null ? null : JsonSerializer.Deserialize<Assignments>(serialAssignments);

            string serialStudents = HttpContext.Session.GetString("courseStudents");
            SIUsers courseStudents = serialStudents == null ? null : JsonSerializer.Deserialize<SIUsers>(serialStudents);

            Submission submission = new Submission();
            submission = _context.Submissions.Where(y => y.SubmissionID == id).Single();

            SpecificAssignment assignment = new SpecificAssignment();

            if (courseAssignments.AssignmentList.Any(x => x.AssignmentID == submission.AssignmentID))
            {
                assignment.Selection = courseAssignments.AssignmentList.Where(x => x.AssignmentID == submission.AssignmentID).Single();
            }
            else
            {
                assignment.Selection = _context.Assignments.Where(x => x.AssignmentID == submission.AssignmentID).Single();
            }

            SIUser student = courseStudents.SIUserList.Where(x => x.UserId == submission.StudentID).Single();

            assignment.ModeSetting = 1;
            string path = "";

            if (submission.SubmissionType == 0)
            {
                path = "https://localhost:44354/" + "/Submissions/" + assignment.Selection.AssignmentID + "/" + student.UserId + "/" + submission.Contents;
            }

            ViewData["Assignment"] = assignment;
            ViewData["Submission"] = submission;
            ViewData["Student"] = student;
            ViewData["File"] = path;
            ViewData["Message"] = session;
            return View("~/Views/Instructor/SubmissionDetail.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmissionDetailAsync([Bind("SubmissionID", "AssignmentID", "StudentID", "SubmissionDate", "SubmissionType", "Grade", "Contents")] GradeValidation updatedGrade)
        {
            string serialUser = HttpContext.Session.GetString("userInfo");
            UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);

            string serialAssignments = HttpContext.Session.GetString("userAssignments");
            Assignments courseAssignments = serialAssignments == null ? null : JsonSerializer.Deserialize<Assignments>(serialAssignments);

            string serialStudents = HttpContext.Session.GetString("courseStudents");
            SIUsers courseStudents = serialStudents == null ? null : JsonSerializer.Deserialize<SIUsers>(serialStudents);

            Submission submission = new Submission();
            bool success = false;
            if (ModelState.IsValid)
            {
                // Create connection to the database
                submission = _context.Submissions.Where(x => x.SubmissionID == updatedGrade.SubmissionID).Single();
                submission.Grade = updatedGrade.Grade;
                // Update Database
                await _context.SaveChangesAsync();

                success = true;
            }


            //getCourses for notification
            string serialCourse = HttpContext.Session.GetString("userCourses");
            Courses userCourses = serialCourse == null ? null : JsonSerializer.Deserialize<Courses>(serialCourse);

            //Need these to create the message for the notification. 
            int courseID = courseAssignments.AssignmentList.Where(a => a.AssignmentID == updatedGrade.AssignmentID).Select(x => x.CourseID).FirstOrDefault();
            String CourseName = userCourses.CourseList.Where(c => c.CourseID == courseID).Select(v => v.ClassTitle).FirstOrDefault();
            String AssignmentName = courseAssignments.AssignmentList.Where(a => a.AssignmentID == updatedGrade.AssignmentID).Select(x => x.Title).FirstOrDefault();


            //create notification for graded assignment. 
            Notification message = new Notification
            {
                RecipientID = submission.StudentID,  //this will send it to the student
                ReferenceID = courseID,         //this makes it so when the student clicks on the notification, it takes them to the course page. 
                NotificationType = "Assignment",
                Message = CourseName + " | " + AssignmentName + " grade was changed",
                DateCreated = DateTime.Now,
                DateViewed = DateTime.Now //had to put this in because it would error if it wasn't initalized. 
            };

            //calls the repository function add which adds a notification to the database. 
            this._notification.Add(message);





            SpecificAssignment assignment = new SpecificAssignment();

            if (courseAssignments.AssignmentList.Any(x => x.AssignmentID == submission.AssignmentID))
            {
                assignment.Selection = courseAssignments.AssignmentList.Where(x => x.AssignmentID == updatedGrade.AssignmentID).Single();
            }
            else
            {
                assignment.Selection = _context.Assignments.Where(x => x.AssignmentID == updatedGrade.AssignmentID).Single();
            }
            assignment.SubmissionList = _context.Submissions.Where(y => y.AssignmentID == updatedGrade.AssignmentID).ToList();

            //grab enrollment
            Enrollments enrollment;
            enrollment = new Enrollments
            {
                EnrollmentList = _context.Enrollments.Where(x => x.courseID == assignment.Selection.CourseID).ToList()
            };

            assignment.ModeSetting = 1;

            ViewData["ClickedAssignment"] = assignment;
            ViewData["Students"] = courseStudents;
            ViewData["Message"] = session;
            //OOOOOOOOOOOOOOOOOOOOOOOOOO --------- DELETE THIS COMMENT LATER ------------ OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO
            //ViewData["Notifications"] = userNotifications;
            //OOOOOOOOOOOOOOOOOOOOOOOOOO --------- DELETE THIS COMMENT LATER ------------ OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO
            return View("Submissions", assignment.Selection.AssignmentID);
        }

    }
}
