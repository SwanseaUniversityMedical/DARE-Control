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
                var project1Users = new List<User>()
    {
        new User() { Name = "User 1" },
        new User() { Name = "User 2" }
    };

                var project2Users = new List<User>()
    {
        new User() { Name = "User 3" },
        new User() { Name = "User 4" }
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
        [Route("Home/ReturnProject/{projectId:int}")]

        public IActionResult ReturnProject(int projectId)
        {
            var project1Users = new List<User>()
    {
        
        new User() { Name = "User 1" },
        new User() { Name = "User 2" }
    };

            var project2Users = new List<User>()
    {
        new User() { Name = "User 3" },
        new User() { Name = "User 4" }
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

            var project = model.Projects.FirstOrDefault(p => p.Id == projectId);

            if (project == null)
            {
                return NotFound();
            }



            return View(project);
        }

        public IActionResult AddUser(int projectId, string userName)
        {
            var project1Users = new List<User>()
    {
        new User() { Name = "User 1" },
        new User() { Name = "User 2" }
    };

            var project2Users = new List<User>()
    {
        new User() { Name = "User 3" },
        new User() { Name = "User 4" }
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

            // Find the project by ID
            var project = model.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return NotFound(); 
            }

            project.Users.Add(new User { Name = userName });

            // Redirect back to the project details page
            return RedirectToAction("ReturnProject", new { id = projectId });
        }




        [Authorize(Policy = "admin")]
        [Route("Home/AdminPanel")]

        public IActionResult AdminPanel()
        {

            return View();
        }

        public IActionResult AddUserToProject()
        {
            AddUser(1, "Luke");
            AddUser(1, "Luke");
            AddUser(1, "Luke");
            AddUser(1, "Luke");
            return View();
        }
    }
}

