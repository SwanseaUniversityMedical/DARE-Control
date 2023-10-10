
using BL.Models.APISimpleTypeReturns;
using BL.Models;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using BL.Services;
using BL.Models.ViewModels;
using NuGet.Protocol;

namespace DARE_FrontEnd.Controllers
{

    [AllowAnonymous]
    public class HomeController : Controller
    {

        private readonly IDareClientHelper _clientHelper;
        private readonly IConfiguration _configuration;

        public HomeController(IDareClientHelper client, IConfiguration configuration)
        {
            _clientHelper = client;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var getAllProj = _clientHelper.CallAPIWithoutModel< List<Project>>("/api/Project/GetAllProjects").Result;
            ViewBag.getAllProj = getAllProj.Count;
            var getAllSubs = _clientHelper.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetAllSubmissions").Result;
            ViewBag.getAllSubs = getAllSubs.Count;
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

