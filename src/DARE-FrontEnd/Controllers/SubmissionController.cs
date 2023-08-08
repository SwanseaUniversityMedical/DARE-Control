using BL.Models;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DARE_FrontEnd.Controllers
{
    [AllowAnonymous]
    public class SubmissionController : Controller
    {
        private readonly IDareClientHelper _clientHelper;
        private readonly IConfiguration _configuration;

        public SubmissionController(IDareClientHelper client, IConfiguration configuration)
        {
            _clientHelper = client;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Instructions()
        {
            var url = _configuration["DareAPISettings:HelpAddress"];
            return View(model:url);
        }

        [HttpGet]
        public IActionResult GetAllSubmissions()
        {

            var test = _clientHelper.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetAllSubmissions/").Result;

            return View(test);
        }


    }
}
