using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Project_Admin.Models;

namespace Project_Admin.Controllers
{
    [Authorize(Policy = "Admin")]
    //[Authorize]

    public class HomeController : Controller
    {
        [Authorize]

        public IActionResult Index()
        {
            return View();
        }

        //this returns a the view testview on https://localhost:7117/home/testview
        [Authorize]
        
        public IActionResult testview()
        {
            var step1model = new TestModel();
            step1model.TestID = 10;
            return View(step1model);
        }


    }
}
