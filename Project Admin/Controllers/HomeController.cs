using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using BL.Models;
using System.Data;
using System.Text.Json;
using Newtonsoft.Json;
using BL.Repositories.DbContexts;
using BL.Services.Project;
using Newtonsoft.Json.Linq;
//using API_Project.Repositories.DbContexts;

namespace Project_Admin.Controllers
{
    //[Authorize(Policy = "admin")]
    [Authorize]

    public class HomeController : Controller
    {
        private readonly IProjectsHandler _projectsHandler;
        //private readonly IAPICaller _apiCaller;


        public HomeController(IProjectsHandler IProjectsHandler/*, IAPICaller IApiCaller*/)
        {
            _projectsHandler = IProjectsHandler;
            //_apiCaller = IApiCaller;
        }

        //private readonly IProjectsHandler _dataSetService;
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
            ////var step1model = new TestModel();
            //step1model.TestID = 10;
            //step1model.TestID = 20;
            //return View(step1model);
            return View();
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
        public async Task<IActionResult> GetProject(int projectId)
        {
            var project = await _projectsHandler.GetProjectSettings(projectId);
               project.Id = projectId;
               return View(project);
        }

        [HttpGet]
        [Route("Home/CreateProject/{projectId:int}")]
        public async Task<IActionResult> CreateProject(int projectId)
        {
            //var create = await _dataSetService.CreateProjectSettings(model);
            var model = new Projects();
            //model.Id = 5;
            model.StartDate = DateTime.Now;
            model.EndDate = DateTime.Now;
            model.Users = new List<User>();
            model.Name = "test project";

            var create = await _projectsHandler.CreateProject(model);

            return View(model);
        }


        [HttpGet]
        [Route("Home/ReturnUser/{userId:int}")]
        public async Task<IActionResult> GetAUser(int userId)
        {
            var user = await _projectsHandler.GetAUser(userId);
            user.Id = userId;
            return View(user);


        }
        [HttpGet]
        [Route("Home/AddUser/{userid:int}")]
        public async Task<IActionResult> AddUser(int userid)
        {
            
            var model = new User();
            //model.Id = 5;
            model.Name = "Luke";
            model.Email = "email@email.com";
            model.Id = userid;
            var create = await _projectsHandler.AddAUser(model);


            //var create = await _projectsHandler.AddAUser(userid);

            return View(userid);
        }


        [Route("Home/Projects/AddUser/{userid:int}/{projectId:int}")]
        public async Task<IActionResult> AddUserToProject(int userId, int projectId)
        {
            var userToProject = await _projectsHandler.AddMembership(userId,projectId);
            return View(userToProject);

        }
        [Authorize(Policy = "admin")]
        [Route("Home/AdminPanel")]

        public IActionResult AdminPanel()
        {
            return View();
        }

    }
}

