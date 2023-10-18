using Microsoft.AspNetCore.Mvc;
using BL.Models.APISimpleTypeReturns;
using BL.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using BL.Models;
using System.Collections.Generic;

namespace TRE_UI.Controllers
{
    [Authorize(Roles = "dare-tre-admin")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITREClientHelper _treClientHelper;

        public HomeController(ILogger<HomeController> logger, ITREClientHelper trehelper)
        {
            _logger = logger;
            _treClientHelper = trehelper;
        }

        public async Task<IActionResult> IndexAsync()
        {
           
            var alreadyset =await  _treClientHelper.CallAPIWithoutModel<BoolReturn>("/api/SubmissionCredentials/CheckCredentialsAreValid");
            if (!alreadyset.Result)
            {
               
                return RedirectToAction("UpdateCredentials", "SubmissionCredentials");
            }
            alreadyset = await _treClientHelper.CallAPIWithoutModel<BoolReturn>("/api/DataEgressCredentials/CheckCredentialsAreValid");
            if (!alreadyset.Result)
            {

                return RedirectToAction("UpdateCredentials", "DataEgressCredentials");
            }
           return RedirectToAction("GetAllProjects", "Approval");
            //return View();
        }
     
        public IActionResult LoginAfterTokenExpired()
        {
            return new SignOutResult(new[]
            {
                OpenIdConnectDefaults.AuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme
            }, new AuthenticationProperties
            {
                RedirectUri = Url.Action("Login", "Home")
            });
        }

        public IActionResult Login()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
            }
            return RedirectToAction("Login", "Home");
        }
        [Authorize]
        public IActionResult Logout()
        {
            return new SignOutResult(new[]
            {
                OpenIdConnectDefaults.AuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme
            }, new AuthenticationProperties
            {
                RedirectUri = Url.Action("Login", "Home")
            });
        }
        public async Task<IActionResult> AccessDenied(string ReturnUrl)
        {
            var accessToken = await HttpContext.GetTokenAsync("access_token");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult TermsAndConditions()
        {
            return View();
        }
    }
}