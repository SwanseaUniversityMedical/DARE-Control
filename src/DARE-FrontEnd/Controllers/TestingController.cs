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


        public async Task<string> NewToken()
        {
            var context = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var result = "";
            //Refresh tokens are used once the access or ID tokens expire
            var currentAccessToken = context.Properties.GetTokenValue("access_token");
            var currentRefreshToken = context.Properties.GetTokenValue("refresh_token");
           
            string keycloakBaseUrl = "https://auth2.ukserp.ac.uk/realms/Dare-Control";
            string clientId = "Dare-Control-UI";
            string clientSecret = "PUykmXOAkyYhIdKzpYz0anPlQ74gUBTz";
            string refreshToken = currentRefreshToken;

            HttpClient httpClient = new HttpClient();

            var tokenEndpoint = $"{keycloakBaseUrl}/protocol/openid-connect/token";
            var tokenRequestBody = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"grant_type", "refresh_token"},
                {"client_id", clientId},
                {"client_secret", clientSecret},
                {"refresh_token", refreshToken},
                {"max_age", "2592000"} // Set a longer max_age in seconds
            });

            var tokenResponse = await httpClient.PostAsync(tokenEndpoint, tokenRequestBody);
            var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();

            if (tokenResponse.IsSuccessStatusCode)
            {
                var tokenJson = JObject.Parse(tokenResponseContent);
                var newAccessToken = tokenJson["access_token"].ToString();
                result = newAccessToken;
                Log.Information("{Function} New Access Token with longer expiry: {newAccessToken}", "NewToken", newAccessToken);
            }
            else
            {
                Log.Error("{Function} Error refreshing token: {tokenResponseContent}", "NewToken", tokenResponseContent);
            }

            return result;
        }

        [HttpGet]
        [Route("Home/NewTokenIssue")]
        [Authorize]
        public async Task<IActionResult> NewTokenIssue()
        {
            
            var oldtoken = HttpContext.User?.Identity?.IsAuthenticated == true
                ? await this.HttpContext.GetTokenAsync("access_token")
                : null;
            //var newtoken = await GetTokenForUser();
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

        

        [Route("Home/TokenRequest")]
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
                    if (roles.roles.Any(gc => gc.Equals("dare-tre-admin")))
                    {
                        expirationTime = DateTime.UtcNow.AddDays(365);
                    }
                    else
                    {
                    expirationTime = DateTime.UtcNow.AddDays(30);
                    }
                    //else if (roles.roles.Any(gc => gc.Equals("dare-control-users")))
                    //{
                    //    expirationTime = DateTime.UtcNow.AddDays(30);
                    //}
                    token.Payload["exp"] = (int)(expirationTime - new DateTime(1970, 1, 1)).TotalSeconds;

                    // Generate a new JWT token with the updated expiration claim
                    var newJwtToken = jwtHandler.WriteToken(token);
                    context.Properties.ExpiresUtc = expirationTime;
                    context.Properties.UpdateTokenValue("access_token", newJwtToken);                             
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, context.Principal, context.Properties);
                    ViewBag.AccessToken = newJwtToken;
                    ViewBag.TokenExpiryDate = expirationTime;
                    //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, context.Principal, context.Properties);
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
