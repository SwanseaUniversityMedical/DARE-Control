using Data_Egress_UI.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using BL.Services;
using BL.Models.APISimpleTypeReturns;

namespace Data_Egress_UI.Controllers
{
    //[Authorize(Roles = "data-egress-admin")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IDataEgressClientHelper _dataClientHelper;
        //private readonly ITREClientHelper _treClientHelper;
        public HomeController(ILogger<HomeController> logger, IDataEgressClientHelper datahelper)
        {
            _logger = logger;
            _dataClientHelper = datahelper;
            //_treClientHelper = trehelper;
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

        //public async Task<IActionResult> Index()
        //{

        //    var alreadyset = await _treClientHelper.CallAPIWithoutModel<BoolReturn>("/api/SubmissionCredentials/CheckCredentialsAreValid");
        //    if (!alreadyset.Result)
        //    {

        //        return RedirectToAction("UpdateCredentials", "SubmissionCredentials");
        //    }
        //    return View();
        //}
        public IActionResult Index()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                return Challenge(OpenIdConnectDefaults.AuthenticationScheme);
            }
            return View();

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

    }
}