using Amazon.Runtime.Internal.Transform;
using BL.Models;
using BL.Models.Tes;
using BL.Models.ViewModels;
using BL.Services;
using DARE_FrontEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.CodeAnalysis;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NuGet.Common;
using Serilog;
using System;
using static System.Net.Mime.MediaTypeNames;
using DARE_FrontEnd.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace DARE_FrontEnd.Controllers
{
    [AllowAnonymous]
    public class SubmissionController : Controller
    {
        private readonly IDareClientHelper _clientHelper;
        private readonly IConfiguration _configuration;
        private readonly URLSettingsFrontEnd _URLSettingsFrontEnd; 
        private readonly IKeyCloakService _IKeyCloakService;


        public SubmissionController(IDareClientHelper client, IConfiguration configuration, URLSettingsFrontEnd URLSettingsFrontEnd, IKeyCloakService IKeyCloakService)
        {
            _clientHelper = client;
            _configuration = configuration;
            _URLSettingsFrontEnd = URLSettingsFrontEnd;
            _IKeyCloakService = IKeyCloakService;
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

                var TesTask = new TesTask()
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


                var result = await _clientHelper.CallAPI<TesTask, TesTask?>("/v1/tasks", TesTask);

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

            var submission = _clientHelper.CallAPIWithoutModel<Submission>($"/api/Submission/GetASubmission/{subId}").Result;
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
            ViewBag.URLBucket = _URLSettingsFrontEnd.MinioUrl;
            var test = new SubmissionInfo()
            {
                Submission = res,
                Stages = _clientHelper.CallAPIWithoutModel<Stages>("/api/Submission/StageTypes/").Result
            };
            return View(test);
        }
        [HttpPost]
        public IActionResult AddExecutors(string image, string command)
        {
            //var model = HttpContext.Session.GetString("AddiSubmissionWizard");
            //var addiSubmissionWizard = string.IsNullOrEmpty(model)
            //    ? new AddiSubmissionWizard()
            //    : JsonConvert.DeserializeObject<AddiSubmissionWizard>(model);


            //addiSubmissionWizard.Executors ??= new List<Executors>();
            //addiSubmissionWizard.Executors.Add(new Executors { Image = image, Command = command });

            //HttpContext.Session.SetString("AddiSubmissionWizard", JsonConvert.SerializeObject(addiSubmissionWizard));

            //return Json(new { success = true });

            var modelJson = HttpContext.Request.Cookies["AddiSubmissionWizard"];
            var model = string.IsNullOrEmpty(modelJson)
                ? new AddiSubmissionWizard()
                : JsonConvert.DeserializeObject<AddiSubmissionWizard>(modelJson);

            // Add the executor to your data source (e.g., a list)
            model.Executors ??= new List<Executors>();
            model.Executors.Add(new Executors { Image = image, Env = command });

            var serializedModel = JsonConvert.SerializeObject(model);
            HttpContext.Response.Cookies.Append("AddiSubmissionWizard", serializedModel);

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<ActionResult> AddiSubmissionWizard(AddiSubmissionWizard model, string Executors, string TreData,string selectedTre)
        {
            var listOfTre = "";

            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", model.ProjectId.ToString());
            var project = await _clientHelper.CallAPIWithoutModel<BL.Models.Project?>(
                "/api/Project/GetProject/", paramlist);

            var test = new TesTask();
            var tesExecutors = new List<TesExecutor>();

            if (string.IsNullOrEmpty(Executors) == false && Executors != "null")
            {
                bool First = true;
                List<Executors> executorsList = JsonConvert.DeserializeObject<List<Executors>>(Executors);
                foreach (var ex in executorsList)
                {
                    if (First)
                    {
                        First = false;
                        continue;
                    }
                    List<string> EnvList = ex.Env.Split(',').ToList();
                    var exet = new TesExecutor()
                    {
                        Image = ex.Image
                    };
                    foreach (var ENV in EnvList)
                    {
                        var vales = ENV.Split("=");
                        exet.Env[vales[0]] = vales[1];
                    }
                    
                    tesExecutors.Add(exet);
                }
            }


            if (selectedTre == "null")
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



            test = new TesTask();

            if (string.IsNullOrEmpty(model.RawInput) == false)
            {
                test = JsonConvert.DeserializeObject<TesTask>(model.RawInput);
            }



        

            if (string.IsNullOrEmpty(model.TESName) == false)
            {
                test.Name = model.TESName; 
            }

            if (string.IsNullOrEmpty(model.TESDescription) == false)
            {
                test.Description = model.TESDescription;
            }

            if (tesExecutors.Count > 0)
            {
                if (test.Executors == null || test.Executors.Count == 0)
                {
                    test.Executors = tesExecutors;
                }
                else
                {
                    test.Executors.AddRange(tesExecutors);
                }
            }

            if (string.IsNullOrEmpty(model.Query) == false)
            {
                var QueryExecutor = new TesExecutor()
                {
                    Image = _URLSettingsFrontEnd.QueryImage,
                    Env = new Dictionary<string, string>()
                    {
                        { "SQL_STATEMENT", model.Query }
                    }
                };


                if (test.Executors == null)
                {
                    test.Executors = new List<TesExecutor>();
                    test.Executors.Add(QueryExecutor);
                }
                else
                {
                    test.Executors.Insert(0, QueryExecutor);
                }
            }

            if (test.Outputs == null || test.Outputs.Count == 0)
            {
                test.Outputs = new List<TesOutput>()
                {
                    new TesOutput()
                    {
                        Url = "",
                        Name = "aName",
                        Description = "ADescription",
                        Path = "/app/data",
                        Type = TesFileType.DIRECTORYEnum,

                    }
                };
            }

            if (test.Tags == null || test.Tags.Count == 0)
            {
                test.Tags = new Dictionary<string, string>()
                        {
                            { "project", project.Name },
                            { "tres", listOfTre }
                        };
            }

            var context = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var Token = await _IKeyCloakService.RefreshUserToken(context);

            var result = await _clientHelper.CallAPI<TesTask, TesTask?>($"/v1/tasks/{Token}", test);

            return RedirectToAction("GetProject", "Project", new { id = model.ProjectId });
        }
    }
}
