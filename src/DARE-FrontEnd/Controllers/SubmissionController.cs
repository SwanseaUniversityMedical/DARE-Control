using BL.Models;
using BL.Models.ViewModels;
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
            List<Submission> displaySubmissionsList = new List<Submission>();
            var res = _clientHelper.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetAllSubmissions/").Result;

            res = res.Where(x => x.Parent == null).ToList();
            //foreach (var submission in res) {
            //    ViewBag.endpoints = submission.Project.Endpoints.ToString();
            //}
            

            return View(res);
        }

        [HttpGet]
        public IActionResult GetASubmission(int id)
        {
            var res = _clientHelper.CallAPIWithoutModel<Submission>("/api/Submission/GetASubmission/").Result;
            var test = new ProjectUserEndpoint();
            var minio = _clientHelper.CallAPIWithoutModel<MinioEndpoint>("/api/Project/GetMinioEndPoint").Result;
            ViewBag.minioendpoint = minio?.Url;

            return View(res);
        }
    }
}
