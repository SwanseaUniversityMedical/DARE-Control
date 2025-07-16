using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using BL.Models.Settings;
using Newtonsoft.Json.Linq;
using System.Net;
using Microsoft.Extensions.Options;

namespace DARE_FrontEnd.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly SubmissionKeyCloakSettings _submissionKeyCloakSettings;

        public AccountController(IOptions<SubmissionKeyCloakSettings> submissionKeyCloakSettings)
        {
            _submissionKeyCloakSettings = submissionKeyCloakSettings.Value;
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> NewTokenIssue()
        {
            var context = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var result = "";
            //Refresh tokens are used once the access or ID tokens expire
            var currentAccessToken = context.Properties.GetTokenValue("access_token");
            var currentRefreshToken = context.Properties.GetTokenValue("refresh_token");

            string keycloakBaseUrl = _submissionKeyCloakSettings.BaseUrl;
            string clientId = _submissionKeyCloakSettings.ClientId;
            string clientSecret = _submissionKeyCloakSettings.ClientSecret;
            string refreshToken = currentRefreshToken;

            HttpClientHandler handler = new HttpClientHandler();

            if (_submissionKeyCloakSettings.Proxy)
            {
                handler = new HttpClientHandler
                {
                    Proxy = new WebProxy(_submissionKeyCloakSettings.ProxyAddressUrl,
                        true), // Replace with your proxy server URL
                    UseProxy = _submissionKeyCloakSettings.Proxy,
                };
            }

            HttpClient httpClient = new HttpClient(handler);

            var tokenEndpoint = $"{keycloakBaseUrl}/protocol/openid-connect/token";
            var tokenRequestBody = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "refresh_token", refreshToken },
                { "max_age", _submissionKeyCloakSettings.TokenRefreshSeconds } // Set a longer max_age in seconds
            });


            var tokenResponse = await httpClient.PostAsync(tokenEndpoint, tokenRequestBody);
            var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();

            if (tokenResponse.IsSuccessStatusCode)
            {
                var tokenJson = JObject.Parse(tokenResponseContent);
                var newAccessToken = tokenJson["access_token"].ToString();
                result = newAccessToken;
                Log.Information("{Function} New Access Token with longer expiry: {newAccessToken}", "NewToken",
                    newAccessToken);
            }
            else
            {
                Log.Error("{Function} Error refreshing token: {tokenResponseContent}", "NewToken",
                    tokenResponseContent);
            }

            var expirationDate = DateTime.Now.AddSeconds(int.Parse(_submissionKeyCloakSettings.TokenRefreshSeconds));
            ViewBag.AccessToken = result;
            ViewBag.TokenExpiryDate = expirationDate;
            return View();
        }


        public class TokenRoles
        {
            public List<string> roles { get; set; }
        }

        public IActionResult Login()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
            }

            return RedirectToAction("LoggedInUser", "Home");
        }

        public IActionResult LoginAfterTokenExpired()
        {
            return new SignOutResult(new[]
            {
                OpenIdConnectDefaults.AuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme
            }, new AuthenticationProperties
            {
                RedirectUri = Url.Action("Login", "Account")
            });
        }

        public IActionResult Logout()
        {
            return new SignOutResult(new[]
            {
                OpenIdConnectDefaults.AuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme
            }, new AuthenticationProperties
            {
                RedirectUri = Url.Action("Login", "Account")
            });
        }

        public async Task<IActionResult> AccessDenied(string ReturnUrl)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return View();
        }
    }
}