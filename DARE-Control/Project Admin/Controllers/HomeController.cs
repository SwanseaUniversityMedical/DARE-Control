using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

using Project_Admin.Models;
using System.Data;

namespace Project_Admin.Controllers
{
    //[Authorize(Policy = "admin")]
    [Authorize]

    public class HomeController : Controller
    {
        //[Authorize]
        //added in mapping and different kind of policy
        public async Task<IActionResult> IndexAsync()
        {
            var test = await HttpContext.GetTokenAsync("access_token");
            return View();
        }

        //this returns a the view testview on https://localhost:7117/home/testview
        //[Authorize]

        public IActionResult testview()
        {
            var step1model = new TestModel();
            step1model.TestID = 10;
            step1model.TestID = 20;
            return View(step1model);
        }

        public IActionResult ReturnAllProjects()
        {
            {
                var project1Users = new List<Users>()
    {
        new Users() { Name = "User 1" },
        new Users() { Name = "User 2" }
    };

                var project2Users = new List<Users>()
    {
        new Users() { Name = "User 3" },
        new Users() { Name = "User 4" }
    };

                var projects = new List<Project>()
    {
        new Project() { Name = "Project 1", Users = project1Users },
        new Project() { Name = "Project 2", Users = project2Users }
    };

                var model = new ProjectListModel()
                {
                    Projects = projects
                };
                return View(model);
            }

        }
        //[Authorize(Policy = "admin")]
        [Route("Home/ReturnProject/{id:int}")]

        public IActionResult ReturnProject(int id) {
            var project1Users = new List<Users>()
    {
        new Users() { Name = "User 1" },
        new Users() { Name = "User 2" }
    };

            var project2Users = new List<Users>()
    {
        new Users() { Name = "User 3" },
        new Users() { Name = "User 4" }
    };

            var projects = new List<Project>()
    {
        new Project() { Id = 1,  Name = "Project 1", Users = project1Users },
        new Project() { Id = 2, Name = "Project 2", Users = project2Users }
    };

            var model = new ProjectListModel()
            {
                Projects = projects
            };

            var project = model.Projects.FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }
    }
}

