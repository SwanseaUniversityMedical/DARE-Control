using BL.Models;
using BL.Models.DTO;


using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using System.Text.Json.Nodes;
using BL.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using static System.Net.Mime.MediaTypeNames;
using Endpoint = BL.Models.Endpoint;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using BL.Models.Settings;

namespace DARE_FrontEnd.Controllers
{
    [Authorize(Roles = "dare-control-admin")]
    public class ProjectController : Controller
    {
        private readonly IDareClientHelper _clientHelper;
        private readonly IFormIOSettings _formSettings;

        public ProjectController(IDareClientHelper client, IFormIOSettings formSettings)
        {
            _clientHelper = client;
            _formSettings = formSettings;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetProject(int id)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", id.ToString());
            var test = _clientHelper.CallAPIWithoutModel<Project?>(
                "/api/Project/GetProject/", paramlist).Result;

            return View(test);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAllProjects()
        {

            var test = _clientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjects/").Result;

            return View(test);
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


        [HttpGet]
        public Task<IActionResult> AddProjectForm()
        {


            return Task.FromResult<IActionResult>(View(new FormData() { FormIoUrl = _formSettings.ProjectForm }));
        }

        [HttpPost]
        public async Task<IActionResult> ProjectFormSubmission([FromBody] FormData model)
        {
            
            var result =
                await _clientHelper.CallAPI<FormData, Project?>("/api/Project/AddProject", model);

            if (result.Id == 0)
            {
                return BadRequest();
            }

            return Redirect("/home");
        }

        [HttpGet]
        public async Task<IActionResult> EditProject(int? projectId)
        {
            var users = _clientHelper.CallAPIWithoutModel<List<User>>("/api/User/GetAllUsers/").Result;
            var endpoints = _clientHelper.CallAPIWithoutModel<List<Endpoint>>("/api/Endpoint/GetAllEndpoints/").Result;

            var userItems = users
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            var endpointItems = endpoints
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", projectId.ToString());
            var project = _clientHelper.CallAPIWithoutModel<Project?>(
                "/api/Project/GetProject/", paramlist).Result;

            var projectView = new ProjectUserEndpoint()
            {
                Id = project.Id,
                Name=project.Name,
                Users = project.Users,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Endpoints = project.Endpoints,
                UserItemList = userItems,
                EndpointItemList = endpointItems
            };

            return View(projectView);
        }

        [HttpGet]
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
            string[] arr= ItemList.Split(',');
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
