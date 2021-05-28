using cs3750LMS.DataAccess;
using cs3750LMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        public async Task<IActionResult> SignUp([Bind("Email,Fname,Lname,Birthday,Password,AccountType")] User users)
        {
            if(_context.Users.Count(e => e.Email == users.Email) == 0)
            {
                if (ModelState.IsValid)
                {
                    _context.Add(users);
                    await _context.SaveChangesAsync();
                    return View("Index");
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
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
            if(userFound.Password == password)
            {
                return View("Index");
            }
            return View();
        } 

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
