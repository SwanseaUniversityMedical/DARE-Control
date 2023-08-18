using BL.Models;
using BL.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using BL.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Endpoint = BL.Models.Endpoint;
using Microsoft.AspNetCore.Authorization;
using BL.Models.Settings;

namespace DARE_FrontEnd.Controllers
{
    [Authorize(Roles = "dare-control-admin")]
    public class ProjectController : Controller
    {
        private readonly IDareClientHelper _clientHelper;
        private readonly IConfiguration _configuration;
        private readonly IFormIOSettings _formIOSettings;

        public ProjectController(IDareClientHelper client, IConfiguration configuration)
        {
            _clientHelper = client;
            _configuration = configuration;
            _formIOSettings = new FormIOSettings();
            configuration.Bind(nameof(FormIOSettings), _formIOSettings);

        }

        //[AllowAnonymous]
        [HttpGet]
        public IActionResult GetProject(int id)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", id.ToString());
            var project = _clientHelper.CallAPIWithoutModel<Project?>(
                "/api/Project/GetProject/", paramlist).Result;

            var minioEndpoint = _clientHelper.CallAPIWithoutModel<MinioEndpoint>("/api/Project/GetMinioEndPoint").Result;

            var projectView = new ProjectUserEndpoint()
            {
                Id = project.Id,
                FormData = project.FormData,
                Name = project.Name,
                Users = project.Users,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                ProjectDescription = project.ProjectDescription,
                Endpoints = project.Endpoints,
                SubmissionBucket = project.SubmissionBucket,
                OutputBucket = project.OutputBucket,
                MinioEndpoint = minioEndpoint.Url,
                Submissions=project.Submissions
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
        public IActionResult AddEndpointMembership()
        {

            var projmem = GetProjectEndpointModel();
            return View(projmem);
        }


        private ProjectEndpoint GetProjectEndpointModel()
        {
            var projs = _clientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjects/").Result;
            var users = _clientHelper.CallAPIWithoutModel<List<Endpoint>>("/api/Endpoint/GetAllEndpoints/").Result;

            var projectItems = projs
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            var endpointItems = users
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            var projmem = new ProjectEndpoint()
            {
                ProjectItemList = projectItems,
                EndpointItemList = endpointItems
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
        public async Task<IActionResult> AddEndpointMembership(ProjectEndpoint model)
        {
            var result =
                await _clientHelper.CallAPI<ProjectEndpoint, ProjectEndpoint?>("/api/Project/AddEndpointMembership",
                    model);
            result = GetProjectEndpointModel();

            return View(result);


        }



        public IActionResult AddProjectForm(int projectId)
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
         
                formData.FormIoString = formData.FormIoString?.Replace(@"""id"":0", @"""id"":" + projectId.ToString());
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

            var projectView = new ProjectUserEndpoint()
            {
                Id = project.Id,
                FormData = project.FormData,
                Name = project.Name,
                Users = project.Users,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Endpoints = project.Endpoints,
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

                var result = await _clientHelper.CallAPI<FormData, Project?>("/api/Project/AddProject", data);

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
            return RedirectToAction("EditProject", new { projectId = projectId });
        }

        [HttpGet]
        public async Task<IActionResult> RemoveEndFromProject(int projectId, int endpointId)
        {
            var model = new ProjectEndpoint()
            {
                ProjectId = projectId,
                EndpointId = endpointId
            };
            var result =
                await _clientHelper.CallAPI<ProjectEndpoint, ProjectEndpoint?>("/api/Project/RemoveEndpointMembership", model);
            return RedirectToAction("EditProject", new { projectId = projectId });
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
            return RedirectToAction("EditProject", new { projectId = ProjectId });
        }

        [HttpPost]
        public async Task<IActionResult> AddEndpintList(string ProjectId, string ItemList)
        {
            string[] arr = ItemList.Split(',');
            foreach (string s in arr)
            {
                var model = new ProjectEndpoint()
                {
                    ProjectId = Int32.Parse(ProjectId),
                    EndpointId = Int32.Parse(s)
                };
                var result =
                await _clientHelper.CallAPI<ProjectEndpoint, ProjectEndpoint?>("/api/Project/AddEndpointMembership",
                    model);
            }
            return RedirectToAction("EditProject", new { projectId = ProjectId });
        }

        [HttpGet]
        [Authorize(Roles = "dare-control-admin")]
        public void IsUSerOnProject(int projectId, int userId)
        {
            var model = new ProjectUser()
            {
                ProjectId = projectId,
                UserId = userId
            };
            var result = _clientHelper.CallAPI<ProjectUser, ProjectUser?>("api/Project/IsUserOnProject", model);
        }

    }
}
