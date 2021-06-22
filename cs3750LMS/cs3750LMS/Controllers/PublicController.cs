using cs3750LMS.DataAccess;
using cs3750LMS.Models;
using cs3750LMS.Models.entites;
using cs3750LMS.Models.general;
using cs3750LMS.Models.validation;
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
        public PublicController(cs3750Context context, IHostingEnvironment _enrionment)
        {
            _context = context;
            Environment = _enrionment;
        }


        //sends user to calendar page if logged in and passes needed session data to view
        public IActionResult Calendar()
        {
            if (HttpContext.Session.Get<string>("user") != null)
            {
                string serialUser = HttpContext.Session.GetString("userInfo");
                UserSession session = serialUser == null ? null : JsonSerializer.Deserialize<UserSession>(serialUser);

                string serialCourse = HttpContext.Session.GetString("userCourses");
                Courses userCourses = serialCourse == null ? null : JsonSerializer.Deserialize<Courses>(serialCourse);

                ViewData["UserCourses"] = userCourses;
                ViewData["Message"] = session;
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
                return View();
            }
            return View("~/Views/Home/Login.cshtml");
        }


        //updates user profile to database from the profile page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([Bind("ProfileImage,Email,FirstName,LastName,Birthday,ProfileImage,Address1,Address2,City,State,Zip,Phone,gitHubLink,TwitterLink,LinkedInLink,Bio")] UserValidationUpdate testUser)
        {
            User users = new User();
            bool updateSuccess = false;
            if (ModelState.IsValid)
            {
                if (_context.Users.Count(e => e.Email == testUser.Email) == 1)
                {

                    users = _context.Users.Where(x => x.Email == testUser.Email).Single();
                    users.Email = testUser.Email;
                    users.FirstName = testUser.FirstName;
                    users.LastName = testUser.LastName;
                    users.Phone = testUser.Phone;
                    users.Birthday = testUser.Birthday;

                    if (testUser.ProfileImage != null) {
                        //start picture logic
                        string wwwPath = this.Environment.WebRootPath;
                        string contentPath = this.Environment.ContentRootPath;
                        string path = Path.Combine(this.Environment.WebRootPath, "Images");

                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        string dbPath = Path.GetFileName(testUser.ProfileImage.FileName);                   //name of file, could save to db as well
                        string FullPath = Path.Combine(path, dbPath);                               //save to database for later reference
                        

                        //delete from files
                        if (System.IO.File.Exists(users.ProfileImage))
                        {
                            System.IO.File.Delete(users.ProfileImage);
                        }
                        //add to files
                        using (FileStream stream = new FileStream(FullPath, FileMode.Create))
                        {
                            testUser.ProfileImage.CopyTo(stream);
                        }
                        users.ProfileImage = "/Images/"+testUser.ProfileImage.FileName;
                    }

                   
                    //////////////////////////end pic logic
                    users.Address1 = testUser.Address1;
                    users.Address2 = testUser.Address2;
                    users.City = testUser.City;
                    users.State = testUser.State;
                    users.Zip = testUser.Zip;
                    users.Phone = testUser.Phone;
                    users.LinkedIn = testUser.LinkedInLink;
                    users.Github = testUser.GitHubLink;
                    users.Twitter = testUser.TwitterLink;
                    users.Bio = testUser.Bio;

                    await _context.SaveChangesAsync();
                    updateSuccess = true;
                }
            }
            UserSession session;
            if (updateSuccess)
            {
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
                    Github =users.Github,
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

        



    }
}
