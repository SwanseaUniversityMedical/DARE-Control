﻿using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BL.Models.APISimpleTypeReturns;
using BL.Services;
using TRE_UI.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace TRE_UI.Controllers
{
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
             var alreadyset =await  _treClientHelper.CallAPIWithoutModel<BoolReturn>("/api/ControlCredentials/CheckCredentialsAreValid");
            if (!alreadyset.Result)
            {
               
                return RedirectToAction("UpdateCredentials", "ControlCredentials");
            }
            return View();
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