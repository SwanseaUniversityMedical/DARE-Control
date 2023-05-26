using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using IdentityModel.Client;
using Serilog;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using BL.Models;
using System.Data;
using System.Text.Json;
using Newtonsoft.Json;
using BL.Repositories.DbContexts;
using DARE_FrontEnd.Services.Project;
using DARE_FrontEnd.Services.FormIO;

//using API_Project.Repositories.DbContexts;

namespace DARE_FrontEnd.Controllers
{
    //[Authorize(Policy = "admin")]
    [Authorize]

    public class HomeController : Controller
    {
        private readonly IProjectsHandler _projectsHandler;
        //private readonly IAPICaller _apiCaller;

        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration configuration;
        private readonly IFormHandler _formsHandler;
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration,IProjectsHandler IProjectsHandler, IFormHandler IFormHandler/*, IAPICaller IApiCaller*/)
        {
            _logger = logger;
            this.configuration = configuration;
            _projectsHandler = IProjectsHandler;
            _formsHandler = IFormHandler;
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
        [Route("ThisTestForm/{Id}")]
        public async Task<IActionResult> ThisTestForm(int Id)
        {
            var create = await _formsHandler.GetFormDataById(Id);

            return View();
        }
        public async Task<IActionResult> AddProjectForm()
        {
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

        //[HttpPost]
        //[Route("Home/Users/AddUser")]
        //public async Task<IActionResult> AddUserToProject(int userid, int projectId)
        //{
        //    var project = await _projectsHandler.GetProjectSettings(projectId);
        //    //var user = await _projectsHandler.GetUserSettings(userid);

        //    if (project == null)
        //    {
        //        //     project.Users.Add(user);
        //    }
        //    else
        //    {
        //        var model = await _projectsHandler.GetAUser(userid);


        //        var create = await _projectsHandler.AddAUser(model);

        //    }
        //    return View(project);
        //}

        [Route("Home/Projects/AddUser/{userid:int}/{projectId:int}")]
        public async Task<IActionResult> AddUserToProject(int userid, int projectId)
        {
            //var user = GetAUser(userid);
            //var project = GetProject(projectId);

            var project = new Projects();
            //model.Id = 5;
            project.StartDate = DateTime.Now;
            project.EndDate = DateTime.Now;
            project.Users = new List<User>();
            project.Name = "test project";

            var create = await _projectsHandler.CreateProject(project);
            var user = new User();
            //model.Id = 5;
            user.Name = "Luke";
            user.Email = "email@email.com";
            user.Id = userid;


            var create1 = await _projectsHandler.AddAUser(user);
            var membership = new ProjectMembership();
            membership.Projects = project;
            membership.Users = user;
            var userToProject = await _projectsHandler.AddMembership(membership);
            return View(userToProject);

        }

        [HttpGet]
        [Route("Home/NewTokenIssue")]
        [Authorize]
        public async Task<IActionResult> NewTokenIssue()
        {
            var handler = new JwtSecurityTokenHandler();
            var context = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //Access tokens are used to access third party resources
            //string currentAccessToken = await HttpContext.GetTokenAsync("access_token");

            //ID tokens are used for user authentication
            string idToken = await HttpContext.GetTokenAsync("id_token");

            //Refresh tokens are used once the access or ID tokens expire
            var currentAccessToken = context.Properties.GetTokenValue("access_token");
            var currentRefreshToken = context.Properties.GetTokenValue("refresh_token");

            //Convert current refresh token to JWT format to check for expiration
            var tokenRefresh = handler.ReadToken(currentRefreshToken) as JwtSecurityToken;
            var refreshTokenExpiry = tokenRefresh.ValidTo;

            //Check if the refresh token has expired
            if (refreshTokenExpiry < DateTime.UtcNow)
            {
                Log.Warning("Users refresh Token has expired");
                //probably need to log user out
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction("Logout", "Account");
            }
            
            //Send request with valid parameters to get a new access token and an associated new refresh token
            var tokenResponse = await new HttpClient().RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = configuration["KeyCloakSettings:Authority"] + "/protocol/openid-connect/token",
                ClientId = configuration["KeyCloakSettings:ClientId"],
                ClientSecret = configuration["KeyCloakSettings:ClientSecret"],
                RefreshToken = currentRefreshToken,
            });

            if (tokenResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                var newAccessToken = tokenResponse.AccessToken;
                var newRefreshToken = tokenResponse.RefreshToken;
                //context.Properties.UpdateTokenValue("access_token", newAccessToken);
                context.Properties.UpdateTokenValue("refresh_token", newRefreshToken);

                //Read the token in JWT format to check for current expiration time(ValidTo parameter)
                var jwtHandler = new JwtSecurityTokenHandler();
                var token = jwtHandler.ReadJwtToken(newAccessToken);

                // Token belongs to the specified group
                // Update the token's expiration time
                DateTime expirationTime = DateTime.UtcNow;
                var groupClaims = token.Claims.Where(c => c.Type == "groups").Select(c => c.Value);
                if (groupClaims.Any(gc => gc.Equals("dare-control-company")))
                {
                    expirationTime = DateTime.UtcNow.AddDays(365);
                }

                else if (groupClaims.Any(gc => gc.Equals("dare-control-users")))
                {
                    expirationTime = DateTime.UtcNow.AddDays(30);
                }
                //Update the token's expiration claim
                token.Payload["exp"] = (int)(expirationTime - new DateTime(1970, 1, 1)).TotalSeconds;

                // Generate a new JWT token with the updated expiration claim
                var newJwtTokenForCompany = jwtHandler.WriteToken(token);
                context.Properties.UpdateTokenValue("access_token", newJwtTokenForCompany);
                ViewBag.NewAccessToken = newJwtTokenForCompany;

                //To print the new expiration time
                //var tokenNew = handler.ReadToken(newJwtTokenForCompany) as JwtSecurityToken;
                //var newTokenExpiry = tokenNew.ValidTo;
                //ViewBag.TokenExpiryDate = newTokenExpiry;
                //ViewBag.Claims = HttpContext.User.Claims.Where(c => c.Type == "groups").ToList();
            }
            return View();
        }
        [Authorize(Policy = "admin")]
        [Route("Home/AdminPanel")]

        public IActionResult AdminPanel()
        {
            return View();
        }

    }
}

