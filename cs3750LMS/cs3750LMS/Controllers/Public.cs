using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cs3750LMS.Controllers
{
    public class Public : Controller
    {
        public IActionResult Calendar()
        {
            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            /// Will want to add something here that will check to see if there is someone logged in.

            return View();
        }
    }
}
