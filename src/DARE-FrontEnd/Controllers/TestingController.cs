using BL.Models;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json.Linq;
using System.Net;

namespace DARE_FrontEnd.Controllers
{
    [Authorize(Roles = "dare-control-admin")]
    public class TestingController : Controller
    {

        private readonly IConfiguration configuration;

        public TestingController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        private string path = @"C:\Users\luke.young\Documents\DareJson\projects.json";
        //added in mapping and different kind of policy
        public async Task<IActionResult> IndexAsync()
        {
            var test = await HttpContext.GetTokenAsync("access_token");
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
        [Route("Home/AddUser/{userid:int}")]
        public async Task<IActionResult> AddUser(int userid)
        {

            var model = new User();
            //model.Id = 5;
            model.Name = "Luke";
            model.Email = "email@email.com";
            model.Id = userid;
            // var create = await _projectsHandler.AddAUser(model);


            //var create = await _projectsHandler.AddAUser(userid);

            return View(userid);
        }



        

        [Authorize(Policy = "admin")]
        [Route("Home/AdminPanel")]

        public IActionResult AdminPanel()
        {
            return View();
        }
    }
}
