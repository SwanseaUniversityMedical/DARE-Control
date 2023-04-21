using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using Project_Admin.Models;
using System.Data;

namespace Project_Admin.Controllers
{
    //[Authorize(Roles = "Admin")]
    //[Authorize]

    public class HomeController : Controller
    {
        [Authorize]

        public async Task<IActionResult> IndexAsync()
        {
            var test = await HttpContext.GetTokenAsync("access_token");
            return View();
        }

        //this returns a the view testview on https://localhost:7117/home/testview
        [Authorize]

        public IActionResult testview()
        {
            var step1model = new TestModel();
            step1model.TestID = 10;
            step1model.TestID = 20;
            return View(step1model);
        }

        public IActionResult ReturnProject()
        { 
    var projects = new ProjectModel()
    {
        Name = "Test",
    };

       
    return View(projects);

    }
    }
}
