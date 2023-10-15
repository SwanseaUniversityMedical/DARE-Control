using BL.Models;
using BL.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using BL.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using BL.Models.Settings;
using Microsoft.AspNetCore.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;
using BL.Models.APISimpleTypeReturns;
using BL.Models.Tes;
using Newtonsoft.Json;

namespace DARE_FrontEnd.Controllers
{
    [Authorize(Roles = "dare-control-admin")]
    public class ProjectController : Controller
    {
        private readonly IDareClientHelper _clientHelper;

        private readonly FormIOSettings _formIOSettings;
        public ProjectController(IDareClientHelper client, FormIOSettings formIo)
        {
            _clientHelper = client;

            _formIOSettings = formIo;

        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetProject(int id)
        {
            var users = _clientHelper.CallAPIWithoutModel<List<User>>("/api/User/GetAllUsers/").Result;
            var tres = _clientHelper.CallAPIWithoutModel<List<Tre>>("/api/Tre/GetAllTres/").Result;

            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", id.ToString());
            var project = _clientHelper.CallAPIWithoutModel<Project?>(
                "/api/Project/GetProject/", paramlist).Result;

            var userItems2 = users.Where(p => !project.Users.Select(x => x.Id).Contains(p.Id)).ToList();
            var treItems2 = tres.Where(p => !project.Tres.Select(x => x.Id).Contains(p.Id)).ToList();

            var userItems = userItems2
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();
            var treItems = treItems2
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            var minioEndpoint = _clientHelper.CallAPIWithoutModel<MinioEndpoint>("/api/Project/GetMinioEndPoint").Result;

            var projectView = new ProjectUserTre()
            {
                Id = project.Id,
                FormData = project.FormData,
                Name = project.Name,
                Users = project.Users,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                ProjectDescription = project.ProjectDescription,
                Tres = project.Tres,
                SubmissionBucket = project.SubmissionBucket,
                OutputBucket = project.OutputBucket,
                MinioEndpoint = minioEndpoint.Url,
                Submissions = project.Submissions.Where(x => x.Parent == null).ToList(),
                UserItemList = userItems,
                TreItemList = treItems
            };

            return View(projectView);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAllProjects()
        {


            var projects = _clientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjects/").Result;
            return View(projects);


        }

        [HttpGet]
        public IActionResult AddUserMembership()
        {

            var projmem = GetProjectUserModel();
            return View(projmem);
        }


        private ProjectUser GetProjectUserModel()
        {
            var projs = _clientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjects/").Result;
            var users = _clientHelper.CallAPIWithoutModel<List<User>>("/api/User/GetAllUsers/").Result;

            var projectItems = projs
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            var userItems = users
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            var projmem = new ProjectUser()
            {
                ProjectItemList = projectItems,
                UserItemList = userItems
            };
            return projmem;
        }

        [HttpGet]
        public IActionResult AddTreMembership()
        {

            var projmem = GetProjectTreModel();
            return View(projmem);
        }


        private ProjectTre GetProjectTreModel()
        {
            var projs = _clientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjects/").Result;
            var users = _clientHelper.CallAPIWithoutModel<List<Tre>>("/api/Tre/GetAllTres/").Result;

            var projectItems = projs
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            var treItems = users
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            var projmem = new ProjectTre()
            {
                ProjectItemList = projectItems,
                TreItemList = treItems
            };
            return projmem;
        }

        [HttpPost]
        public async Task<IActionResult> AddUserMembership(ProjectUser model)
        {
            var result =
                await _clientHelper.CallAPI<ProjectUser, ProjectUser?>("/api/Project/AddUserMembership", model);
            result = GetProjectUserModel();
            return View(result);

        }

        [HttpPost]
        public async Task<IActionResult> AddTreMembership(ProjectTre model)
        {
            var result =
                await _clientHelper.CallAPI<ProjectTre, ProjectTre?>("/api/Project/AddTreMembership",
                    model);
            result = GetProjectTreModel();

            return View(result);

        }


        [HttpGet]
        public IActionResult SaveProjectForm(int projectId)
        {
            var formData = new FormData()
            {
                FormIoUrl = _formIOSettings.ProjectForm,
                FormIoString = @"{""id"":0}",

            };

            if (projectId > 0)
            {
                var paramList = new Dictionary<string, string>();
                paramList.Add("projectId", projectId.ToString());
                var project = _clientHelper.CallAPIWithoutModel<BL.Models.Project>("/api/Project/GetProject/", paramList).Result;
                formData.FormIoString = project?.FormData;

                formData.FormIoString = formData.FormIoString?.Replace(@"""id"":0", @"""Id"":" + projectId.ToString(), StringComparison.CurrentCultureIgnoreCase);
            }

            return View(formData);
        }

        [HttpGet]
        public IActionResult GetProjectBuckets(int id)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", id.ToString());
            var project = _clientHelper.CallAPIWithoutModel<Project?>(
                "/api/Project/GetProject/", paramlist).Result;

            var minioEndpoint = _clientHelper.CallAPIWithoutModel<MinioEndpoint>("/api/Project/GetMinioEndPoint").Result;

            var projectView = new ProjectUserTre()
            {
                Id = project.Id,
                FormData = project.FormData,
                Name = project.Name,
                Users = project.Users,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Tres = project.Tres,
                SubmissionBucket = project.SubmissionBucket,
                OutputBucket = project.OutputBucket,
                MinioEndpoint = minioEndpoint.Url
            };

            return View(projectView);
        }

        [HttpPost]
        public async Task<IActionResult> ProjectFormSubmission([FromBody] object arg, int id)
        {
            var str = arg?.ToString();

            if (!string.IsNullOrEmpty(str))
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<FormData>(str);
                data.FormIoString = str;

                var result = await _clientHelper.CallAPI<FormData, Project?>("/api/Project/SaveProject", data);

                if (result.Id == 0)
                    return BadRequest();

                return Ok(result);
            }
            return BadRequest();
        }


        public async Task<IActionResult> RemoveUserFromProject(int projectId, int userId)
        {
            var model = new ProjectUser()
            {
                ProjectId = projectId,
                UserId = userId
            };
            var result =
                await _clientHelper.CallAPI<ProjectUser, ProjectUser?>("/api/Project/RemoveUserMembership", model);

            return RedirectToAction("GetProject", new { id = projectId });
        }

        [HttpGet]
        public async Task<IActionResult> RemoveTreFromProject(int projectId, int treId)
        {
            var model = new ProjectTre()
            {
                ProjectId = projectId,
                TreId = treId
            };
            var result =
                await _clientHelper.CallAPI<ProjectTre, ProjectTre?>("/api/Project/RemoveTreMembership", model);

            return RedirectToAction("GetProject", new { id = projectId });
        }

        [HttpPost]
        public async Task<IActionResult> AddUserList(string ProjectId, string ItemList)
        {
            string[] arr = ItemList.Split(',');
            foreach (string s in arr)
            {
                var model = new ProjectUser()
                {
                    ProjectId = Int32.Parse(ProjectId),
                    UserId = Int32.Parse(s)
                };
                var result =
                await _clientHelper.CallAPI<ProjectUser, ProjectUser?>("/api/Project/AddUserMembership", model);
            }
            return RedirectToAction("GetProject", new { id = ProjectId });
        }

        [HttpPost]
        public async Task<IActionResult> AddTreList(string ProjectId, string ItemList)
        {
            string[] arr = ItemList.Split(',');
            foreach (string s in arr)
            {
                var model = new ProjectTre()
                {
                    ProjectId = Int32.Parse(ProjectId),
                    TreId = Int32.Parse(s)
                };
                var result =
                await _clientHelper.CallAPI<ProjectTre, ProjectTre?>("/api/Project/AddTreMembership",
                    model);
            }
            return RedirectToAction("GetProject", new { id = ProjectId });
        }

        [HttpGet]
        [Authorize(Roles = "dare-control-admin")]
        public void IsUserOnProject(int projectId, int userId)
        {
            var model = new ProjectUser()
            {
                ProjectId = projectId,
                UserId = userId
            };
            var result = _clientHelper.CallAPI<ProjectUser, ProjectUser?>("api/Project/IsUserOnProject", model);
        }

        [HttpPost]
        public async Task<IActionResult> SubmissionWizerd(SubmissionTes model)
        {
            var listOfTre = "";
            var imageUrl = "";

            if (model.Tre == null)
            {
                var paramList = new Dictionary<string, string>();
                paramList.Add("projectId", model.ProjectId.ToString());
                var tre = _clientHelper.CallAPIWithoutModel<List<Tre>>("/api/Project/GetTresInProject/", paramList).Result;
                List<string> namesList = tre.Select(test => test.Name).ToList();
                listOfTre = string.Join("|", namesList);
            }
            else
            {
                listOfTre = string.Join("|", model.Tre);
            }

            if (model.Option == "url")
            {
                imageUrl = model.Url;
            }
            else
            {

                var paramss = new Dictionary<string, string>();

                paramss.Add("bucketName", model.SubmissionBucket);
                
                var uplodaResultTest = _clientHelper.CallAPIToSendFile<APIReturn>("/api/Project/UploadToMinio", "file", model.File, paramss).Result;
                var minioEndpoint = _clientHelper.CallAPIWithoutModel<MinioEndpoint>("/api/Project/GetMinioEndPoint").Result;

                imageUrl = "http://" + minioEndpoint.Url + "/browser/" + model.SubmissionBucket + "/" + model.File.FileName;

               

            }

            var test = new TesTask()
            {

                Name = model.Name,
                Executors = new List<TesExecutor>()
                {
                    new TesExecutor()
                    {
                        Image = imageUrl,

                    }
                },
                Tags = new Dictionary<string, string>()
                {
                    { "project", model.Project },
                    { "tres", listOfTre }
                }

            };

            var result = _clientHelper.CallAPI<TesTask, TesTask?>("/v1/tasks", test).Result;

            return Json(new { success = true, message = "Data received successfully." });
        }

        public string ConvertIFormFileToJson(IFormFile formFile)
        {
            if (formFile == null)
                return null;


            using (var stream = new MemoryStream())
            {
                formFile.CopyTo(stream);
                var bytes = stream.ToArray();

                var base64String = Convert.ToBase64String(bytes);

                var fileData = new IFileData
                {
                    FileName = formFile.FileName,
                    ContentType = formFile.ContentType,
                    Content = base64String
                };

                using (var memoryStream = new MemoryStream())
                {
                    System.Text.Json.JsonSerializer.SerializeAsync(memoryStream, fileData).Wait();
                    return System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                }
            }
        }

    }
}
