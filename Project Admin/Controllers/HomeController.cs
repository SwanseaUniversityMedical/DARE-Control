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
//using API_Project.Repositories.DbContexts;

namespace Project_Admin.Controllers
{
    //[Authorize(Policy = "admin")]
    [Authorize]

    public class HomeController : Controller
    {
        private readonly IProjectsHandler _projectsHandler;
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration configuration;
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            this.configuration = configuration;
        }
        //private readonly IProjectsHandler _dataSetService;
        private string path = @"C:\Users\luke.young\Documents\DareJson\projects.json";

        //added in mapping and different kind of policy
        [Authorize]
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

            var create = await _projectsHandler.CreateProject(model);

            return View(model);
        }

        [HttpPost]
        [Route("Home/Users/AddUser")]


        public async Task<IActionResult> AddUser(int userid)
        {
            //might need to add more stuff here that will fill out additional user info
            var create = await _projectsHandler.AddUser(userid);

            return View(userid);
        }

        [HttpPost]
        [Route("Home/Users/AddUser")]


        public async Task<IActionResult> AddUserToProject(int userid, int projectId)
        {
            var project = await _projectsHandler.GetProjectSettings(projectId);
            var user = await _projectsHandler.GetUserSettings(userid);

            if (project == null)
            {
                project.Users.Add(user);
            }

            return View(project);
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
                var tokenNew = handler.ReadToken(newJwtTokenForCompany) as JwtSecurityToken;
                var newTokenExpiry = tokenNew.ValidTo;
                ViewBag.TokenExpiryDate = newTokenExpiry;
                ViewBag.Claims = HttpContext.User.Claims.Where(c => c.Type == "groups").ToList();
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

