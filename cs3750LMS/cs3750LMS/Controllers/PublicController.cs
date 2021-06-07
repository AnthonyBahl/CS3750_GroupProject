using cs3750LMS.DataAccess;
using cs3750LMS.Models;
using cs3750LMS.Models.validation;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace cs3750LMS.Controllers
{
    public class PublicController : Controller
    {
        private readonly cs3750Context _context;
        public PublicController(cs3750Context context)
        {
            _context = context;
        }

        //sends user to calendar page if logged in and passes needed session data to view
        public IActionResult Calendar()
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
                User userFound = _context.Users.Where(u => u.Email == HttpContext.Session.Get<string>("user")).Single();
                UserSession session = new UserSession
                {
                    Email = userFound.Email,
                    FirstName = userFound.FirstName,
                    LastName = userFound.LastName,
                    Birthday = userFound.Birthday,
                    AccountType = userFound.AccountType,
                    ////////////////////////////////////////////////ProflieImage = userFound.ProfileImage,///////////////////////// still need profile image in database
                    Address1 = userFound.Address1,
                    Address2 = userFound.Address2,
                    City = userFound.City,
                    State = userFound.State,
                    Zip = userFound.Zip,
                    Phone = userFound.Phone
                };
                int userLinkCount = _context.Links.Count(z => z.UserID == userFound.UserId);
                if ( userLinkCount > 0)
                {
                    session.UserLinks = _context.Links.Where(z=>z.UserID == userFound.UserId).ToList();
                }
                ViewData["Message"] = session;
                return View();
            }
            return View("~/Views/Home/Login.cshtml");
        }

        //updates user profile to database from the profile page
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update([Bind("Email,FirstName,LastName,Birthday,Password,ConfirmPassword,AccountType,ProfileImage,Address1,Address2,City,State,Zip,Phone,UserLinks")] UserValidationUpdate testUser)
        {
            bool updateSuccess = false;
            if (ModelState.IsValid)
            {
                if (_context.Users.Count(e => e.Email == testUser.Email) == 0)
                {

                    User users = _context.Users.Where(x => x.Email == testUser.Email).Single();


                    users.Email = testUser.Email;
                    users.FirstName = testUser.FirstName;
                    users.LastName = testUser.LastName;
                    users.Birthday = testUser.Birthday;
                    users.Password = Sha256(testUser.Password);
                    users.AccountType = testUser.AccountType;
                    ///////////////////////////////////////ProfileImage = testUser.ProfileImage;///////////////////need to implement new field in database
                    users.Address1 = testUser.Address1;
                    users.Address2 = testUser.Address2;
                    users.City = testUser.City;
                    users.State = testUser.State;
                    users.Zip = testUser.Zip;
                    users.Phone = testUser.Phone;

                    await _context.SaveChangesAsync();
                    updateSuccess = true;
                }
            }
            User userFound = _context.Users.Where(u => u.Email == HttpContext.Session.Get<string>("user")).Single();
            UserSession session = new UserSession
            {
                Email = userFound.Email,
                FirstName = userFound.FirstName,
                LastName = userFound.LastName,
                Birthday = userFound.Birthday,
                AccountType = userFound.AccountType,
                ////////////////////////////////////////////////ProflieImage = userFound.ProfileImage,///////////////////////// still need profile image in database
                Address1 = userFound.Address1,
                Address2 = userFound.Address2,
                City = userFound.City,
                State = userFound.State,
                Zip = userFound.Zip,
                Phone = userFound.Phone,
                IsUpdate = updateSuccess
            };

            int userLinkCount = _context.Links.Count(z => z.UserID == userFound.UserId);
            if (userLinkCount > 0)
            {
                session.UserLinks = _context.Links.Where(z => z.UserID == userFound.UserId).ToList();
            }

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
