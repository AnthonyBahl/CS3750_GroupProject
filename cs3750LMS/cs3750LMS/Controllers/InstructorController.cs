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
    public class InstructorController : Controller
    {
        private readonly cs3750Context _context;
        public InstructorController(cs3750Context context)
        {
            _context = context;
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

                HttpContext.Session.SetString(courseKey,JsonSerializer.Serialize(course));
            }

            course.ModeSetting = 1;

            ViewData["ClickedCourse"] = course;
            ViewData["Message"] = session;
            return View("~/Views/Instructor/CourseEdit.cshtml");
        }
        //-------------------------------Course Edit Logic End----------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAssignment([Bind("CourseID,Title,Description,MaxPoints,DueDate,DueTime,SubmitType")] AssignmentValidationAdd assignment){
            if (ModelState.IsValid)
            {
                Assignment newA = new Assignment
                {
                    CourseID = assignment.CourseID,
                    Title = assignment.Title,
                    Description = assignment.Description,
                    MaxPoints = assignment.MaxPoints,
                    DueDate = assignment.DueDate + assignment.DueTime,
                    SubmissionType = assignment.SubmitType
                };

                //Add to database
                _context.Assignments.Add(newA);
                _context.SaveChanges();

                string courseKey = "course" + assignment.CourseID;
                string serialSelected = HttpContext.Session.GetString(courseKey);
                SpecificCourse course = JsonSerializer.Deserialize<SpecificCourse>(serialSelected);
                course.AssignmentList.Add(newA);
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
                _assignment = _context.Assignments.Where(x =>x.AssignmentID == editAssignment.AssignmentID).Single();                                                            

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
                    //reload timespans
                    string serialTimes = HttpContext.Session.GetString("courseTimes");
                    List<TimeStamp> times = JsonSerializer.Deserialize<List<TimeStamp>>(serialTimes);
                    userCourses.RefactorTimeSpans(times);

                    //if departments were grabbed before are saved in session else put in session
                    string serialDepts = HttpContext.Session.GetString("Departments");
                    Departments depts;
                    if(serialDepts != null)
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
            //reload timespans
            string serialTimes = HttpContext.Session.GetString("courseTimes");
            List<TimeStamp> times = JsonSerializer.Deserialize<List<TimeStamp>>(serialTimes);
            userCourses.RefactorTimeSpans(times);

            //if model valid add new course
            bool success = false;
            if (ModelState.IsValid)
            {
                Course newCourse = new Course
                {
                    InstructorID = session.UserId,
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
                //update session saved courses
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
            }

            //pass data to view
            ViewData["DepartmentData"] = depts;
            ViewData["Message"] = session;
            ViewData["Courses"] = userCourses;
            return View();
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

            //reload timespans
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
            }

            //pass data to view
            ViewData["DepartmentData"] = depts;
            ViewData["Message"] = session;
            ViewData["Courses"] = userCourses;
            return View("AddClass");
        }
    }
}
