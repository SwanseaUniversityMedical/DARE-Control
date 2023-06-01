using BL.Repositories.DbContexts;
using Microsoft.AspNetCore.Mvc;
using BL.Models;
using System.Text.Json.Nodes;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using IdentityModel.Client;


namespace BL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {

        private readonly ILogger<UserController> _logger;
        private readonly IConfiguration configuration;
        private readonly ApplicationDbContext _DbContext;

        public UserController(ApplicationDbContext applicationDbContext, ILogger<UserController> logger, IConfiguration configuration)
        {

            _DbContext = applicationDbContext;
            _logger = logger;
            this.configuration = configuration;
        }
        
        //[HttpPost("Add_User")]

        //public async Task<User> AddUser([FromBody] User Users)
        //{
        //    _DbContext.Users.Add(Users);
        //    await _DbContext.SaveChangesAsync();

        //    return Users;
        //}


        [HttpPost("Add_User1")]
        public async Task<User> AddUser(JsonObject submissionData)
        {
            //save session id against it
            User users = JsonConvert.DeserializeObject<User>(submissionData.ToString());
            //user = JsonConvert.DeserializeObject<User>(submissionData.ToString());

            var Name = users.Name;
            var Email = users.Email;
            _DbContext.Users.Add(users);
            await _DbContext.SaveChangesAsync();
            //_DbContext.SaveChangesAsync();

            return users;
        }


        [HttpGet("Get_User/{userId}")]

        public User GetUser(int userId)
        {
            var returned = _DbContext.Users.Find(userId);
            if (returned == null)
            {
                return null;
            }
            
            return returned;
        }

        [HttpGet("GetNewToken/{userId}")]
        public async Task<IActionResult> GetNewToken(int userId)
        {
            var handler = new JwtSecurityTokenHandler();
            var context = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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
                //ViewBag.NewAccessToken = newJwtTokenForCompany;
            }
            return Ok();
        }
    }
}
