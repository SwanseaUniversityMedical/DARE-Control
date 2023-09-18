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


            return View(res);
        }

        [HttpGet]
        public IActionResult GetASubmission(int id)
        {
            var paramlist = new Dictionary<string, string>();

            paramlist.Add("submissionId", id.ToString());

            var res = _clientHelper.CallAPIWithoutModel<Submission>("/api/Submission/GetASubmission/", paramlist).Result;
            //var res1 = _clientHelper.CallAPIWithoutModel<SubmissionInfo>("/api/Submission/StageTypes/").Result;

            var minio = _clientHelper.CallAPIWithoutModel<MinioEndpoint>("/api/Project/GetMinioEndPoint").Result;
            ViewBag.minioendpoint = minio?.Url;

            var test = new SubmissionInfo()
            {
                Submission = res,
                StageInfo = _clientHelper.CallAPIWithoutModel<List<StageInfo>>("/api/Submission/StageTypes/").Result
            };
            return View(test);
        }
    }
}
