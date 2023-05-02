using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

using Project_Admin.Models;
using System.Data;
using System.Text.Json;
using Newtonsoft.Json;
using Project_Admin.Repositories.DbContexts;

namespace Project_Admin.Controllers
{
    //[Authorize(Policy = "admin")]
    [Authorize]

    public class HomeController : Controller
    {
        private string path = @"C:\Users\luke.young\Documents\DareJson\projects.json";
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
        [Route("Home/AllProjects")]

        public IActionResult ReturnAllProjects()
        {
            string jsonString = System.IO.File.ReadAllText(path);


            var projectList = System.Text.Json.JsonSerializer.Deserialize<ProjectListModel>(jsonString);

            
            var model = new ProjectListModel()
            {
                Projects = projectList.Projects
            };

            return View(model);
        }


        [Route("Home/ReturnProject/{projectId:int}")]
        public IActionResult ReturnProject(int projectId)
        {
            var projectJson = System.IO.File.ReadAllText(path);
            var projectListModel = JsonConvert.DeserializeObject<ProjectListModel>(projectJson);

            var project = projectListModel.Projects.FirstOrDefault(p => p.Id == projectId);


            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }



        //[Route("Home/ReturnProject/{projectId:int}")]
        //public IActionResult AddUser(int projectId)
        //{
        //    var projectJson = System.IO.File.ReadAllText(path);
        //    var projectListModel = JsonConvert.DeserializeObject<ProjectListModel>(projectJson);
        //    var model = ApplicationDbContext.Users();

        //    var project = projectListModel.Projects.FirstOrDefault(p => p.Id == projectId);


        //    if (project == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(project);


        //}
        [Authorize(Policy = "admin")]
        [Route("Home/AdminPanel")]

        public IActionResult AdminPanel()
        {

            return View();
        }

    }
}

