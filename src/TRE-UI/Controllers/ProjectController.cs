using BL.Models;
using BL.Models.DTO;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestSharp;
using System.Text.Json.Nodes;
using BL.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TRE_UI.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IDareClientHelper _dareclientHelper;
        private readonly ITREClientHelper _treclientHelper;
        public ProjectController(IDareClientHelper dareclient, ITREClientHelper treclient)
        {
            _dareclientHelper = dareclient;
            _treclientHelper = treclient;
        }
    
        [HttpGet]
        public IActionResult AddProject()
        {
            return View();

        }

        [HttpGet]
        public IActionResult GetProject(int id)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", id.ToString());
            var projects = _dareclientHelper.CallAPIWithoutModel<Project?>(
                "/api/Project/GetProject/", paramlist).Result;

            return View(projects);
        }
     
        [HttpGet]
        public IActionResult GetAllProjects()
        {

            var projects = _dareclientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjects/").Result;

            return View(projects);
        }
        [HttpGet]
        public IActionResult ApproveProjects()
        {
            return View();

        }
     

        [HttpGet]
        public IActionResult RequestProjectMembership()
        {

            var projmem = GetProjectUserModel();
            return View(projmem);
        }

        private ProjectUserTre GetProjectUserModel()
        {
            var projs = _dareclientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjects/").Result;
            var users = _dareclientHelper.CallAPIWithoutModel<List<User>>("/api/User/GetAllUsers/").Result;

            var projectItems = projs
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            var userItems = users
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            var projmem = new ProjectUserTre()
            {
                Username = userItems.Where(p => p.Value == "2").First().Text,
                Projectname = projectItems.Where(p => p.Value == "2").First().Text,
                ProjectItemList = projectItems,
                UserItemList = userItems
            };

            return projmem;
        }
        [HttpPost]
        public async Task<IActionResult> RequestProjectMembership(ProjectUserTre model)
        {
            var projmem = GetProjectUserModel();
            var result = await _treclientHelper.CallAPI<ProjectUserTre, ProjectUserTre?>("/api/Project/RequestMembership", model);

            return View(result);


        }
        public async Task<IActionResult> AddUserMembership(ProjectUserTre model)
        {
            var result =
                await _dareclientHelper.CallAPI<ProjectUserTre, ProjectUserTre?>("/api/Project/AddUserMembership", model);
            result = GetProjectUserModel();


            return View(result);


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
                await _dareclientHelper.CallAPI<ProjectUser, ProjectUser?>("/api/Project/RemoveUserMembership", model);
            return RedirectToAction("EditProject", new { projectId = projectId });
        }

        [HttpGet]
        public async Task<IActionResult> EditProject(int? projectId)
        {
            var users = _dareclientHelper.CallAPIWithoutModel<List<User>>("/api/User/GetAllUsers/").Result;
            
            var userItems = users
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

        
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", projectId.ToString());
            var project = _dareclientHelper.CallAPIWithoutModel<Project?>(
                "/api/Project/GetProject/", paramlist).Result;

            var projectView = new ProjectUserEndpoint()
            {
                Id = project.Id,
                Name = project.Name,
                Users = project.Users,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                UserItemList = userItems,
               
            };

            return View(projectView);
        }
        [HttpGet]
        public IActionResult GetAllProjectsForApproval()
        {
            var projmem = GetProjectUserModel();
     
            var projects = _treclientHelper.CallAPIWithoutModel<List<ProjectApproval>>("/api/Project/GetAllProjectsForApproval/").Result;

            return View(projects);
        }

    }
}
