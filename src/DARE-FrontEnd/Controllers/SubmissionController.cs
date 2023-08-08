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

        public SubmissionController(IDareClientHelper client)
        {
            _clientHelper = client;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Instructions()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAllSubmissions()
        {

            var test = _clientHelper.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetAllSubmissions/").Result;

            return View(test);
        }


    }
}
