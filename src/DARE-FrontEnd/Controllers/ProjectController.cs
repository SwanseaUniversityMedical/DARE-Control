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

namespace DARE_FrontEnd.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IDareClientHelper _clientHelper;

        public ProjectController(IDareClientHelper client)
        {
            _clientHelper = client;
        }

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

            
            return Task.FromResult<IActionResult>(View(new FormData()
            {
                FormIoUrl = "https://feidldzemrnfcva.form.io/createnewproject"
            }));


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
    }
}
