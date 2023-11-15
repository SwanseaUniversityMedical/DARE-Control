using BL.Models;
using BL.Models.Tes;
using BL.Models.ViewModels;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using NuGet.Common;
using Serilog;

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

      

        public IActionResult Instructions()
        {
            var url = _configuration["DareAPISettings:HelpAddress"];
            return View(model:url);
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubmissionWizard(SubmissionWizard model)
        {
            try
            {
                var listOfTre = "";
                var imageUrl = "";
                var paramlist = new Dictionary<string, string>();
                paramlist.Add("projectId", model.ProjectId.ToString());
                var project = await _clientHelper.CallAPIWithoutModel<BL.Models.Project?>(
                    "/api/Project/GetProject/", paramlist);
                if (model.TreRadios == null)
                {
                    var paramList = new Dictionary<string, string>();
                    paramList.Add("projectId", model.ProjectId.ToString());
                    var tre = await _clientHelper.CallAPIWithoutModel<List<Tre>>("/api/Project/GetTresInProject/", paramList);
                    List<string> namesList = tre.Select(test => test.Name).ToList();
                    listOfTre = string.Join("|", namesList);
                }
                else
                {
                    listOfTre = string.Join("|", model.TreRadios.Where(info => info.IsSelected).Select(info => info.Name));
                }

                if (model.OriginOption == CrateOrigin.External)
                {
                    imageUrl = model.ExternalURL;
                }
                else
                {

                    var paramss = new Dictionary<string, string>();

                    paramss.Add("bucketName", project.SubmissionBucket);
                    if (model.File != null)
                    {
                        var uplodaResultTest = await _clientHelper.CallAPIToSendFile<APIReturn>("/api/Project/UploadToMinio", "file", model.File, paramss);
                    }
                    var minioEndpoint = await _clientHelper.CallAPIWithoutModel<MinioEndpoint>("/api/Project/GetMinioEndPoint");

                    imageUrl = "http://" + minioEndpoint.Url + "/browser/" + project.SubmissionBucket + "/" + model.File.FileName;
                }
                var test = new TesTask();
                if (string.IsNullOrEmpty(model.TesRun))
                {
                    test = new TesTask()
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
                }
                else
                {
                    test = JsonConvert.DeserializeObject<TesTask>(model.TesRun);
                }

                var result = await _clientHelper.CallAPI<TesTask, TesTask?>("/v1/tasks", test);

                return RedirectToAction("GetProject", "Project", new { id = model.ProjectId });
            }
            catch (Exception ex)
            {
                Log.Error("SubmissionWizard > " + ex.ToString());
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadFileAsync(int subId)
        {


            var paramlist = new Dictionary<string, string>
            {
                { "submissionId", subId.ToString() }
            };

            var submission = _clientHelper.CallAPIWithoutModel<Submission>("/api/Submission/GetASubmission/", paramlist).Result;
            var file = await _clientHelper.CallAPIToGetFile(
                "/api/Submission/DownloadFile", paramlist);
            
            return File(file, GetContentType(submission.FinalOutputFile), submission.FinalOutputFile);
        }

        public static string GetContentType(string fileName)
        {
            // Create a new FileExtensionContentTypeProvider
            var provider = new FileExtensionContentTypeProvider();

            // Try to get the content type based on the file name's extension
            if (provider.TryGetContentType(fileName, out var contentType))
            {
                return contentType;
            }

            // If the content type cannot be determined, provide a default value
            return "application/octet-stream"; // This is a common default for unknown file types
        }

        [HttpGet] 
        public IActionResult GetAllSubmissions()
        {
            List<Submission> displaySubmissionsList = new List<Submission>();
            var res = _clientHelper.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetAllSubmissions/").Result.Where(x => x.Parent == null).ToList();

            res = res.Where(x => x.Parent == null).ToList();


            return View(res);
        }

        [HttpGet]
        public IActionResult GetASubmission(int id)
        {
            var res = _clientHelper.CallAPIWithoutModel<Submission>($"/api/Submission/GetASubmission/{id}").Result;
            

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
