using cs3750LMS.DataAccess;
using cs3750LMS.Models;
using cs3750LMS.Models.entites;
using cs3750LMS.Models.general;
using cs3750LMS.Models.validation;
using cs3750LMS.Models.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace cs3750LMS.Controllers
{
    public class PublicController : Controller
    {
        private readonly cs3750Context _context;
        private IHostingEnvironment Environment;
        private readonly INotificationRepository _notification;
        public PublicController(cs3750Context context, IHostingEnvironment _enrionment, INotificationRepository notification)
        {
            _context = context;
            Environment = _enrionment;
            _notification = notification;
        }


        //sends user to calendar page if logged in and passes needed session data to view
        public IActionResult Calendar()
        {
            if (HttpContext.Session.Get<string>("user") != null)
            {
                string serialUser = HttpContext.Session.GetString("userInfo");
                UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);

                // Courses
                string serialCourse = HttpContext.Session.GetString("userCourses");
                Courses userCourses = serialCourse == null ? null : JsonSerializer.Deserialize<Courses>(serialCourse);

                // Assignments
                string serialAssignment = HttpContext.Session.GetString("userAssignments");
                Assignments userAssignments = serialAssignment == null ? null : JsonSerializer.Deserialize<Assignments>(serialAssignment);

                //reload timespans
                string serialTimes = HttpContext.Session.GetString("courseTimes");
                List<TimeStamp> times = JsonSerializer.Deserialize<List<TimeStamp>>(serialTimes);
                userCourses.RefactorTimeSpans(times);

                ViewData["UserCourses"] = userCourses;
                ViewData["UserAssignments"] = userAssignments;
                ViewData["Message"] = session;
                ViewData["url"] = "/Public/Calendar";
                return View();
            }
            return View("~/Views/Home/Login.cshtml");
        }

        //sends user to profile page if logged in and passes needed session data to view
        public IActionResult Profile()
        {
            if (HttpContext.Session.Get<string>("user") != null)
            {
                string serialStates = HttpContext.Session.GetString("States");
                States states = new States();
                if (serialStates != null)
                {
                    states = JsonSerializer.Deserialize<States>(serialStates);
                }
                else
                {
                    states.StatesList = _context.States.ToList();
                    HttpContext.Session.SetString("States", JsonSerializer.Serialize(states));
                }

                string serialUser = HttpContext.Session.GetString("userInfo");
                UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);

                ViewData["States"] = states;
                ViewData["Message"] = session;
                ViewData["url"] = "/Public/Profile";
                return View();
            }
            return View("~/Views/Home/Login.cshtml");
        }


        //updates user profile to database from the profile page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([Bind("ProfileImage,Email,FirstName,LastName,Birthday,ProfileImage,Address1,Address2,City,State,Zip,Phone,GitHubLink,TwitterLink,LinkedInLink,Bio")] UserValidationUpdate testUser)
        {
            User users = new User();
            bool updateSuccess = false;
            if (ModelState.IsValid)
            {
                if (_context.Users.Count(e => e.Email == testUser.Email) == 1)
                {
                    updateSuccess = ProfileDBUpdate(testUser, _context);
                }
            }
            UserSession session;
            if (updateSuccess)
            {
                users = _context.Users.Where(e => e.Email == testUser.Email).Single();
                session = new UserSession
                {
                    UserId = users.UserId,
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
            }
            else
            {
                string serialUser = HttpContext.Session.GetString("userInfo");
                session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);
            }

            session.IsUpdate = updateSuccess;

            string serialStates = HttpContext.Session.GetString("States");
            States states = new States();
            if (serialStates != null)
            {
                states = JsonSerializer.Deserialize<States>(serialStates);
            }
            else
            {
                states.StatesList = _context.States.ToList();
                HttpContext.Session.SetString("States", JsonSerializer.Serialize(states));
            }



            ViewData["States"] = states;
            ViewData["Message"] = session;
            ViewData["url"] = "/Public/Profile";
            return View("Profile");
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

        public bool ProfileDBUpdate(UserValidationUpdate updateUser, cs3750Context _context)
        {
            try
            {
                User users = new User();

                users = _context.Users.Where(x => x.Email == updateUser.Email).Single();
                users.Email = updateUser.Email;
                users.FirstName = updateUser.FirstName;
                users.LastName = updateUser.LastName;
                users.Phone = updateUser.Phone;
                users.Birthday = updateUser.Birthday;

                if (updateUser.ProfileImage != null)
                {
                    //start picture logic
                    string wwwPath = this.Environment.WebRootPath;
                    string contentPath = this.Environment.ContentRootPath;
                    string path = Path.Combine(this.Environment.WebRootPath, "Images");

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    string dbPath = Path.GetFileName(updateUser.ProfileImage.FileName);                   //name of file, could save to db as well
                    string FullPath = Path.Combine(path, dbPath);                               //save to database for later reference

                    //delete from files
                    if (System.IO.File.Exists(users.ProfileImage))
                    {
                        System.IO.File.Delete(users.ProfileImage);
                    }
                    //add to files
                    using (FileStream stream = new FileStream(FullPath, FileMode.Create))
                    {
                        updateUser.ProfileImage.CopyTo(stream);
                    }
                    users.ProfileImage = "/Images/" + updateUser.ProfileImage.FileName;
                }

                //////////////////////////end pic logic
                users.Address1 = updateUser.Address1;
                users.Address2 = updateUser.Address2;
                users.City = updateUser.City;
                users.State = updateUser.State;
                users.Zip = updateUser.Zip;
                users.Phone = updateUser.Phone;
                users.LinkedIn = updateUser.LinkedInLink;
                users.Github = updateUser.GitHubLink;
                users.Twitter = updateUser.TwitterLink;
                users.Bio = updateUser.Bio;

                _context.SaveChanges();
                return true;
            } 
            catch
            {
                return false;
            }
            
        }



        public bool CreateNotification(int recipientId, int referenceId, string type, string message, INotificationRepository _noti)
        {
            //create notification for graded assignment. 
            Notification noti = new Notification
            {
                RecipientID = recipientId,  //this is the ID of the person receiving the Notification. 
                ReferenceID = referenceId,         //this makes it so when the student clicks on the notification, it takes them to the course page or when the teacher clicks on it, it takes them to the submission page. 
                NotificationType = type,   //type of notifications "Assignment"
                Message = message,        //Notification Message
                DateCreated = DateTime.Now,
                DateViewed = DateTime.Now //had to put this in because it would error if it wasn't initialized. 
            };


            //calls the repository function add which adds a notification to the database.                 
            _noti.Add(noti);

            return true;
        }

    }
}
