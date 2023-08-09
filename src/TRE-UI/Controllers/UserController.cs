using BL.Models;
using BL.Services;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TRE_UI.Controllers
{
    //[Authorize(Roles = "dare-TRE-admin")]
    public class UserController : Controller
    {
        private readonly ITREClientHelper _clientHelper;
        public UserController(ITREClientHelper client)
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
        [Authorize]
        public async Task<IActionResult> RequestTokenForEndpoints()
        {

            var oldtoken = HttpContext.User?.Identity?.IsAuthenticated == true
                ? await this.HttpContext.GetTokenAsync("access_token")
                : null;
            var newtoken = await GetTokenForUser();
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

       
        public async Task<string> GetTokenForUser()
        {

            //string clientId = "Dare-TRE-UI";
            //string clientSecret = "5da4W2dwgRfYb3Jfp9if7KSPQLiMZ93I";
            string clientId = "Dare-Control-UI";
            string clientSecret = "PUykmXOAkyYhIdKzpYz0anPlQ74gUBTz";
            var authority = "https://auth2.ukserp.ac.uk/realms/Dare-Control";

            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = authority,
                Policy = new DiscoveryPolicy
                {
                    ValidateIssuerName = false, // Keycloak may have a different issuer name format
                }
            });

            if (disco.IsError)
            {
                Log.Error("{Function} {Error}", "GetTokenForUser", disco.Error);
                return "";
            }

            var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                UserName = "testingtreapi",
                Password = "statuszero0!"
            });


            if (tokenResponse.IsError)
            {
                Log.Error("{Function} {Error}", "GetTokenForUser", tokenResponse.Error);
                return "";
            }

            Log.Information("{Function} {AccessToken}", "GetTokenForUser", tokenResponse.AccessToken);
            Log.Information(tokenResponse.RefreshToken);
            return tokenResponse.AccessToken;
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


    }
}
