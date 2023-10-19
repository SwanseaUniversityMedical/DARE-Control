using BL.Models;
using BL.Models.Tes;
using BL.Models.ViewModels;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;

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

        [Authorize]
        [HttpGet]
        public IActionResult SubmissionWizard(int projectId)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", projectId.ToString());
            var project = _clientHelper.CallAPIWithoutModel<BL.Models.Project?>(
                "/api/Project/GetProject/", paramlist).Result;
            var model = new SubmissionWizard()
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                SelectTresOptions = project.Tres.Select(x => x.Name).ToList()
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmissionWizard(SubmissionWizard model)
        {
            var listOfTre = "";
            var imageUrl = "";
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", model.ProjectId.ToString());
            var project = _clientHelper.CallAPIWithoutModel<BL.Models.Project?>(
                "/api/Project/GetProject/", paramlist).Result;
            if (model.Tres == null)
            {
                var paramList = new Dictionary<string, string>();
                paramList.Add("projectId", model.ProjectId.ToString());
                var tre = _clientHelper.CallAPIWithoutModel<List<Tre>>("/api/Project/GetTresInProject/", paramList).Result;
                List<string> namesList = tre.Select(test => test.Name).ToList();
                listOfTre = string.Join("|", namesList);
            }
            else
            {
                listOfTre = string.Join("|", model.Tres);
            }

            if (model.OriginOption == CrateOrigin.External)
            {
                imageUrl = model.ExternalURL;
            }
            else
            {

                var paramss = new Dictionary<string, string>();

                paramss.Add("bucketName", project.SubmissionBucket);

                var uplodaResultTest = _clientHelper.CallAPIToSendFile<APIReturn>("/api/Project/UploadToMinio", "file", model.File, paramss).Result;
                var minioEndpoint = _clientHelper.CallAPIWithoutModel<MinioEndpoint>("/api/Project/GetMinioEndPoint").Result;

                imageUrl = "http://" + minioEndpoint.Url + "/browser/" + project.SubmissionBucket + "/" + model.File.FileName;



            }

            var test = new TesTask()
            {

                Name = model.TESName,
                Executors = new List<TesExecutor>()
                {
                    new TesExecutor()
                    {
                        Image = imageUrl,

                    }
                },
                Tags = new Dictionary<string, string>()
                {
                    { "project", project.Name },
                    { "tres", listOfTre }
                }

            };

            var result = _clientHelper.CallAPI<TesTask, TesTask?>("/v1/tasks", test).Result;

            return RedirectToAction("GetProject", "Project", new {id = model.ProjectId});
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
            

            var minio = _clientHelper.CallAPIWithoutModel<MinioEndpoint>("/api/Project/GetMinioEndPoint").Result;
            ViewBag.minioendpoint = minio?.Url;

            var test = new SubmissionInfo()
            {
                Submission = res,
                Stages = _clientHelper.CallAPIWithoutModel<Stages>("/api/Submission/StageTypes/").Result
            };
            return View(test);
        }
    }
}
