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
using DARE_FrontEnd.Services.Project;
using DARE_FrontEnd.Services.FormIO;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Xml.Linq;

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

        //[HttpGet]
        //[Route("Home/CreateProject/{projectId:int}")]
        //public async Task<IActionResult> CreateProject(int projectId)
        //{
        //    //var create = await _dataSetService.CreateProjectSettings(model);
        //    var model = new Projects();
        //    //model.Id = 5;
        //    model.StartDate = DateTime.Now;
        //    model.EndDate = DateTime.Now;
        //    model.Users = new List<User>();
        //    model.Name = "test project";

        //    var create = await _projectsHandler.CreateProject(model);

        //    return View(model);
        //}

        //[HttpGet]
        //[Route("Home/CreateProject/{projectId:int}")]
        //public async Task<IActionResult> CreateProject(JsonObject project)
        //{
        //    string jsonString = project.ToString();
        //    JObject jsonObject = JObject.Parse(jsonString);

        //    var projectName = (string)jsonObject["ProjectName"];
        //    var startDate = (int)jsonObject["StartDate"];
        //    var endDate = (string)jsonObject["EndDate"];
        //    //var users = new List<User>();
        //    //IEnumerable<string> keys = jsonObject.Properties().Select(p => p.Name);
        //    //IEnumerable<string> keys = jsonObject.Properties().Select(p => p.);

        //    //var create = await _dataSetService.CreateProjectSettings(model);
        //    var model = new Projects();
        //    //model.Id = 5;
        //    //model.StartDate = project;
        //    model.EndDate = DateTime.Now;
        //    model.Users = new List<User>();
        //   // model.Name = keys.Name;

        //    var create = await _projectsHandler.CreateProject(model);

        //    return View(model);
        //}

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
           // var create = await _projectsHandler.AddAUser(model);


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

        //[Route("Home/Projects/AddUser/{userid:int}/{projectId:int}")]
        //public async Task<IActionResult> AddUserToProject(int userid, int projectId)
        //{
        //    //var user = GetAUser(userid);
        //    //var project = GetProject(projectId);

        //    var project = new Projects();
        //    //model.Id = 5;
        //    project.StartDate = DateTime.Now;
        //    project.EndDate = DateTime.Now;
        //    project.Users = new List<User>();
        //    project.Name = "test project";

        //    //var create = await _projectsHandler.CreateProject(project);
        //    var user = new User();
        //    //model.Id = 5;
        //    user.Name = "Luke";
        //    user.Email = "email@email.com";
        //    user.Id = userid;


        //    //var create1 = await _projectsHandler.AddAUser(user);
        //    var membership = new ProjectMembership();
        //    membership.Projects = project;
        //    membership.Users = user;
        //    var userToProject = await _projectsHandler.AddMembership(membership);
        //    return View(userToProject);

        //}

        [HttpGet]
        [Route("Home/GetEndpointsList/{projectId:int}")]
        public async Task<IActionResult> GetEndpoints(int projectId)
        {
            var endpoints = await _projectsHandler.GetAllEndPoints(projectId);
            return View(endpoints);
        }

        [HttpGet]
        [Route("Home/NewTokenIssue")]
        [Authorize]
        public async Task<IActionResult> NewTokenIssue()
        {
            ViewBag.Claims = HttpContext.User.Claims.Where(c => c.Type == "groups").ToList();
            var accessToken = HttpContext.User?.Identity?.IsAuthenticated == true
                ? await this.HttpContext.GetTokenAsync("access_token")
                : null;
            ViewBag.AccessToken = accessToken;
            var handler = new JwtSecurityTokenHandler();
            var tokenS = handler.ReadToken(accessToken) as JwtSecurityToken;
            var tokenExpiryDate = tokenS.ValidTo;
            ViewBag.TokenExpiryDate = tokenExpiryDate;
            return View();
        }
        public async Task<IActionResult> TokenRequest()
        {
            var handler = new JwtSecurityTokenHandler();
            var context = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

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

            var tokenResponse = await new HttpClient().RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = "https://auth2.ukserp.ac.uk/realms/Dare-Control/protocol/openid-connect/token",
                ClientId = "Dare-Control-UI",
                ClientSecret = "PUykmXOAkyYhIdKzpYz0anPlQ74gUBTz",
                RefreshToken = currentRefreshToken,
            });

            //Send request with valid parameters to get a new access token and an associated new refresh token

            if (tokenResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                var newAccessToken = tokenResponse.AccessToken;              
                var jwtHandler = new JwtSecurityTokenHandler();
                var token = jwtHandler.ReadJwtToken(newAccessToken);

                DateTime expirationTime = DateTime.UtcNow;
                var groupClaims = token.Claims.Where(c => c.Type == "realm_access").Select(c => c.Value);
                var roles = new TokenRoles()
                {
                    roles = new List<string>()
                };
                if (groupClaims.Any())
                {
                    roles = JsonConvert.DeserializeObject<TokenRoles>(groupClaims.First());
                }
                if (roles.roles.Any(gc => gc.Equals("dare-control-company")))
                {
                    expirationTime = DateTime.UtcNow.AddDays(365);
                }
                else if (roles.roles.Any(gc => gc.Equals("dare-control-users")))
                {
                    expirationTime = DateTime.UtcNow.AddDays(30);
                }               
                token.Payload["exp"] = (int)(expirationTime - new DateTime(1970, 1, 1)).TotalSeconds;

                // Generate a new JWT token with the updated expiration claim
                var newJwtToken = jwtHandler.WriteToken(token);
                context.Properties.UpdateTokenValue("access_token", newJwtToken);
                ViewBag.AccessToken = newJwtToken;
                ViewBag.TokenExpiryDate = expirationTime;
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, context.Principal, context.Properties);
                return RedirectToAction("NewTokenIssue");
            }
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); //, context.Principal, context.Properties);
            return RedirectToAction("Logout", "Account");
        }
        public class TokenRoles
        {
            public List<string> roles { get; set; }
        }

        [Authorize(Policy = "admin")]
        [Route("Home/AdminPanel")]

        public IActionResult AdminPanel()
        {
            return View();
        }

    }
}

