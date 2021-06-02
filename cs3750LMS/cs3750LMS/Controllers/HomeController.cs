using cs3750LMS.DataAccess;
using cs3750LMS.Models;
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
        public HomeController(ILogger<HomeController> logger,cs3750Context context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            if(HttpContext.Session.Get<string>("user") != null)
            {
                ViewData["Message"] = _context.UserCache.Where(y => y.CacheId == HttpContext.Session.Get<string>("user")).Single();
                return View();
            }
            return View("Login");
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignUp([Bind("Email,FirstName,LastName,Birthday,Password,AccountType")] User users)
        {
            if(_context.Users.Count(e => e.Email == users.Email) == 0)
            {
                if (ModelState.IsValid)
                {
                    users.Password = Sha256(users.Password);
                    _context.Add(users);
                    await _context.SaveChangesAsync();

                    if (_context.UserCache.Count(y => y.CacheId == users.Email) == 0)
                    {
                        UserCache create = new UserCache();
                        create.CacheId = users.Email;
                        create.UserEmail = users.Email;
                        create.CacheFirstName = users.FirstName;
                        create.CacheLastName = users.LastName;
                        create.ExpiresAtTime = new DateTimeOffset(DateTime.Now.Year, DateTime.Now.Month,
                                                            DateTime.Now.Day, DateTime.Now.Hour,
                                                            DateTime.Now.Minute, DateTime.Now.Second,
                                                            new TimeSpan(3, 0, 0));
                        _context.Add(create);
                        await _context.SaveChangesAsync();
                    }

                    HttpContext.Session.Set<string>("user", users.Email);
                    ViewData["Message"] = _context.UserCache.Where(y => y.CacheId == HttpContext.Session.Get<string>("user")).Single();

                    return View("Index");
                }
            }
           
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            User userFound;
            if(_context.Users.Count(y=>y.Email == email) == 1)
            {
                userFound = _context.Users.Where(e => e.Email == email).Single();
            }
            else
            {
                return View();
            }
            if(userFound.Password == Sha256(password))
            {

                if(_context.UserCache.Count(y=>y.CacheId == userFound.Email) == 0){
                    UserCache create = new UserCache();
                    create.CacheId = userFound.Email;
                    create.UserEmail = userFound.Email;
                    create.CacheFirstName = userFound.FirstName;
                    create.CacheLastName = userFound.LastName;
                    create.ExpiresAtTime = new DateTimeOffset(DateTime.Now.Year,DateTime.Now.Month,
                                                        DateTime.Now.Day,DateTime.Now.Hour,
                                                        DateTime.Now.Minute,DateTime.Now.Second,
                                                        new TimeSpan(3, 0, 0));
                    _context.Add(create);
                    await _context.SaveChangesAsync();
                }

                HttpContext.Session.Set<string>("user",userFound.Email);
                ViewData["Message"] = _context.UserCache.Where(y=>y.CacheId == HttpContext.Session.Get<string>("user")).Single();
                return View("Index");
            }
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
