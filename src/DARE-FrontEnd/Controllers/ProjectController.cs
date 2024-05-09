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
using Serilog;
using EasyNetQ.Management.Client.Model;
using DARE_FrontEnd.Models;
using System.Diagnostics;

namespace DARE_FrontEnd.Controllers
{
    [Authorize(Roles = "dare-control-admin")]
    public class ProjectController : Controller
    {
        private readonly IDareClientHelper _clientHelper;

        private readonly FormIOSettings _formIOSettings;


        private readonly URLSettingsFrontEnd _URLSettingsFrontEnd;
        public ProjectController(IDareClientHelper client, FormIOSettings formIo, URLSettingsFrontEnd URLSettingsFrontEnd)
        {
            _clientHelper = client;

            _formIOSettings = formIo;

            _URLSettingsFrontEnd = URLSettingsFrontEnd;

        }

        private bool IsUserOnProject(SubmissionGetProjectModel proj)
        {
            if (User.IsInRole("dare-control-admin"))
            {
                return true;
            }

            var usersName = "";
            usersName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(usersName) &&
                (from x in proj.Users where x.Name.ToLower().Trim() == usersName.ToLower().Trim() select x).Any())
            {
                return true;
            }
            return false;
        }

        private bool IsUserOnProject(Project proj)
        {
            if (User.IsInRole("dare-control-admin"))
            {
                return true;
            }

            var usersName = "";
            usersName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(usersName) &&
                (from x in proj.Users where x.Name.ToLower().Trim() == usersName.ToLower().Trim() select x).Any())
            {
                return true;
            }
            return false;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetProject(int id)
        {
            var users = _clientHelper.CallAPIWithoutModel<List<BL.Models.User>>("/api/User/GetAllUsers/");
            var tres = _clientHelper.CallAPIWithoutModel<List<Tre>>("/api/Tre/GetAllTres/");
            var minioEndpoint = _clientHelper.CallAPIWithoutModel<MinioEndpoint>("/api/Project/GetMinioEndPoint");
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", id.ToString());
            var projectawait = _clientHelper.CallAPIWithoutModel<SubmissionGetProjectModel>(
                "/api/Project/GetProjectUI/", paramlist);

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await users;
            Log.Error("users took ElapsedMilliseconds" + stopwatch.ElapsedMilliseconds);
            await tres;
            Log.Error("tres took ElapsedMilliseconds" + stopwatch.ElapsedMilliseconds);
            await projectawait;
            Log.Error("projectawait took ElapsedMilliseconds" + stopwatch.ElapsedMilliseconds);
            await minioEndpoint;
            Log.Error("minioEndpoint took ElapsedMilliseconds" + stopwatch.ElapsedMilliseconds);
            stopwatch.Stop();
            var project = projectawait.Result;

            var userItems2 = users.Result.Where(p => !project.Users.Select(x => x.Id).Contains(p.Id)).ToList();
            var treItems2 = tres.Result.Where(p => !project.Tres.Select(x => x.Id).Contains(p.Id)).ToList();

            
            var userItems = userItems2
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.FullName != "" ? p.FullName : p.Name })
                .ToList();
            var treItems = treItems2
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

           
            ViewBag.UserCanDoSubmissions = IsUserOnProject(project);
            
            ViewBag.minioendpoint = minioEndpoint.Result.Url;
            ViewBag.URLBucket = _URLSettingsFrontEnd.MinioUrl;

            var projectView = new ProjectUserTre()
            {
                Id = project.Id,
                FormData = project.FormData,
                Name = project.Name,
                Users = project.Users,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                ProjectDescription = project.ProjectDescription,
                ProjectContact = project.ProjectContact,
                ProjectOwner = project.ProjectOwner,
                Tres = project.Tres,
                SubmissionBucket = project.SubmissionBucket,
                OutputBucket = project.OutputBucket,
                MinioEndpoint = minioEndpoint.Result.Url,
                Submissions = project.Submissions.Where(x => x.HasParent == false).ToList(),
                UserItemList = userItems,
                TreItemList = treItems
            };
        
            Log.Error("View(projectView) took ElapsedMilliseconds" + stopwatch.ElapsedMilliseconds);
            return View(projectView);
        }

        public IActionResult SubmissionProjectSQL(int id)
        {
           
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", id.ToString());
            var project = _clientHelper.CallAPIWithoutModel<Project?>(
                "/api/Project/GetProject/", paramlist).Result;

            ViewBag.UserCanDoSubmissions = IsUserOnProject(project);

            var projectView = new ProjectUserTre()
            {
                Id = project.Id,
                Name = project.Name
            };

            return View(projectView);
        }


        public IActionResult SubmissionProjectGraphQL(int id)
        {

            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", id.ToString());
            var project = _clientHelper.CallAPIWithoutModel<Project?>(
                "/api/Project/GetProject/", paramlist).Result;

            ViewBag.UserCanDoSubmissions = IsUserOnProject(project);

            var projectView = new ProjectUserTre()
            {
                Id = project.Id,
                Name = project.Name
            };

            return View(projectView);
        }

        public IActionResult SubmissionProjectCrate(int id)
        {

            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", id.ToString());
            var project = _clientHelper.CallAPIWithoutModel<Project?>(
                "/api/Project/GetProject/", paramlist).Result;

            ViewBag.UserCanDoSubmissions = IsUserOnProject(project);

            var projectView = new ProjectUserTre()
            {
                Id = project.Id,
                Name = project.Name
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
            var users = _clientHelper.CallAPIWithoutModel<List<BL.Models.User>>("/api/User/GetAllUsers/").Result;

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
                Id = projectId
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
                {
                    TempData["error"] = "";
                    return BadRequest();
                }
                TempData["success"] = "Project Save Successfully";

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
            TempData["success"] = "User Remove Successfully";
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
            TempData["success"] = "Tre Remove Successfully";
            return RedirectToAction("GetProject", new { id = projectId });
        }

        [HttpPost]
        public async Task<IActionResult> AddUserList(string ProjectId, string ItemList)
        {
            string[] arr = ItemList.Split(',');
            List<string> userList = new List<string>();
            bool addedUser = false;
            foreach (string s in arr)
            {
                var model = new ProjectUser()
                {
                    ProjectId = Int32.Parse(ProjectId),
                    UserId = Int32.Parse(s)
                };
                var user =
                await _clientHelper.CallAPI<ProjectUser, ProjectUser?>("/api/Project/CheckUserExists", model);
                if (user.UserId == 0)
                {
                    var paramList = new Dictionary<string, string>();
                    paramList.Add("userId", s.ToString());
                    var userInfo = _clientHelper.CallAPIWithoutModel<BL.Models.User?>("/api/User/GetUser/", paramList).Result;
                    userList.Add(userInfo.Name);
                }
                else
                {
                    var response =
                await _clientHelper.CallAPI<ProjectUser, ProjectUser?>("/api/Project/AddUserMembership", model);
                    addedUser = true;
                }
            }
            if (userList.Count > 0)
            {
                var listOfNoneExistingUser = string.Join(", ", userList);
                TempData["error"] = listOfNoneExistingUser + "are not exist in keycloak. Need to Register";
            }
            if (addedUser)
            {

                TempData["success"] = "User Added Successfully";
            }
            return Ok();
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
            TempData["success"] = "Tre Added Successfully";
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
