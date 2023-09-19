
using BL.Models.APISimpleTypeReturns;
using BL.Models;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using BL.Services;

namespace DARE_FrontEnd.Controllers
{
    
    [AllowAnonymous]
    public class HomeController : Controller
    {
        

        

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult TermsAndConditions()
        {
            return View();
        }
        public IActionResult PrivacyPolicy()
        {
            return View();
        }

    }
}

