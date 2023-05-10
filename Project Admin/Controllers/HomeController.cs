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
using Project_Admin.Services.Project;

namespace Project_Admin.Controllers
{
    //[Authorize(Policy = "admin")]
    [Authorize]

    public class HomeController : Controller
    {
        private readonly IProjectsHandler _dataSetService;
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

        [HttpGet]
        [Route("Home/ReturnProject/{projectId:int}")]
        public IActionResult ReturnProject(int projectId)
        {
            var projectJson = System.IO.File.ReadAllText(path);
            var projectListModel = JsonConvert.DeserializeObject<ProjectListModel>(projectJson);

            var project = projectListModel.Projects.FirstOrDefault(p => p.Id == projectId);
            //getting from the database will look something like this
            //var project = await _dataSetService.GetUserSettings(projectId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpPost]
        [Route("Home/ReturnProjecttest/{projectId:int}")]


        public async Task<IActionResult> CreateProject(int projectId, Projects model)
        {
            //var create = await _dataSetService.CreateProjectSettings(model);
            model.Id = 5;
            model.StartDate = DateTime.Now;
            model.EndDate = DateTime.Now;
            model.Users = new List<User>();
            model.Name = "test project";
            var create = await _dataSetService.CreateProjectSettings(model);

            return View(model);
        }

        [HttpPost]
        [Route("Home/Users/AddUser")]


        public async Task<IActionResult> AddUser(int userid)
        {
            //might need to add more stuff here that will fill out additional user info
            var create = await _dataSetService.AddUser(userid);

            return View(userid);
        }

        [HttpPost]
        [Route("Home/Users/AddUser")]


        public async Task<IActionResult> AddUserToProject(int userid, int projectId)
        {
            var project = await _dataSetService.GetProjectSettings(projectId);
            var user = await _dataSetService.GetUserSettings(userid);

            if (project == null)
            {
                project.Users.Add(user);
            }

            return View(project);
        }

        [Authorize(Policy = "admin")]
        [Route("Home/AdminPanel")]

        public IActionResult AdminPanel()
        {

            return View();
        }

    }
}

