
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
        private readonly IDareClientHelper _clientHelper;

        public HomeController(IDareClientHelper client)
        {
            _clientHelper = client;

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

