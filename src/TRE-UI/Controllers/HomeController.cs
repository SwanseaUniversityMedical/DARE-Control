using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BL.Models.APISimpleTypeReturns;
using BL.Services;
using TRE_UI.Models;

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