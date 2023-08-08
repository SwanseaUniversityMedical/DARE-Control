using BL.Models;
using BL.Models.DTO;
using BL.Services;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using System.IdentityModel.Tokens.Jwt;

namespace TRE_UI.Controllers
{
    //[Authorize(Roles = "dare-control-admin")]
    public class UserController : Controller
    {
        private readonly IDareClientHelper _clientHelper;
        public UserController(IDareClientHelper client)
        {
            _clientHelper = client;
        }
    
        [HttpGet]
        public IActionResult GetAllUsers()
        {

            var users = _clientHelper.CallAPIWithoutModel<List<User>>("/api/User/GetAllUsers/").Result;

            return View(users);
        }
        public IActionResult GetUser(int id)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("userId", id.ToString());
            var test = _clientHelper.CallAPIWithoutModel<User?>(
                "/api/User/GetUser/", paramlist).Result;

            return View(test);
        }
        [HttpGet]
        ////[Route("Home/TokenForEndpoints")]
        //[Authorize]

        [Authorize(Roles = "dare-tre-admin")]
        public async Task<IActionResult> RequestTokenForEndpoints()
        {  

            //var handler = new JwtSecurityTokenHandler();
            var context = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //Refresh tokens are used once the access or ID tokens expire
            var currentRefreshToken = context.Properties.GetTokenValue("refresh_token");
            var tokenResponse = await new HttpClient().RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = "https://auth2.ukserp.ac.uk/auth/realms/Dare-Control",
                ClientId = "Dare-TRE-UI",
                ClientSecret = "5da4W2dwgRfYb3Jfp9if7KSPQLiMZ93I",
                RefreshToken = currentRefreshToken,
            });

            // Send request with valid parameters to get a new access token and an associated new refresh token

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
                    expirationTime = DateTime.UtcNow.AddDays(10);
                }
                token.Payload["exp"] = (int)(expirationTime - new DateTime(1970, 1, 1)).TotalSeconds;

                // Generate a new JWT token with the updated expiration claim
                var newJwtToken = jwtHandler.WriteToken(token);
                context.Properties.UpdateTokenValue("access_token", newJwtToken);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, context.Principal, context.Properties);
            }
            return Ok();
        }
        public class TokenRoles
        {
            public List<string> roles { get; set; }
        }

    }
}
