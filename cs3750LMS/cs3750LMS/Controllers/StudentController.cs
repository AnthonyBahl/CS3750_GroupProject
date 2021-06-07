using cs3750LMS.DataAccess;
using cs3750LMS.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public IActionResult Register()
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
                if (userFound.AccountType == 0)
                {
                    ViewData["Message"] = session;
                    return View();
                }
            }
            return View("~/Views/Home/Login.cshtml");
        }
    }
}
