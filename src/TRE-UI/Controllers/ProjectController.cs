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
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;
using Amazon.S3;

namespace TRE_UI.Controllers
{ 
    //[Authorize(Roles = "dare-control-admin")]
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
        public IActionResult RequestProjectMembership()
        {

            var projmem = GetProjectUserModel();
            return View(projmem);
        }
        private ProjectUserTre GetProjectUserModelSubmit(int projectId,int userId,string Localprojectname)
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
                Username = userItems.Where(p => p.Value == "1").First().Text,
                Projectname = projectItems.Where(p => p.Value == "1").First().Text,
                UserId = userId,
                LocalProjectName = Localprojectname,
                ProjectId = projectId
               
            };

            return projmem;
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
     
                ProjectItemList = projectItems,
                UserItemList = userItems
            };

            return projmem;
        }
        [HttpPost]
        public async Task<IActionResult> RequestProjectMembership(ProjectUserTre model)
        {

            model = GetProjectUserModelSubmit(model.ProjectId,model.UserId,model.LocalProjectName);
          
            var result = await _treclientHelper.CallAPI<ProjectUserTre, ProjectUserTre?>("/api/Project/RequestMembership", model);
            ViewBag.ApprovalResult= "Project Sent for Approval!";
            return View(result);


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
                await _dareclientHelper.CallAPI<ProjectUser, ProjectUser?>("/api/Project/AddUserMembership", model);
            }
            return RedirectToAction("EditProject", new { projectId = ProjectId });
        }
        [HttpPost]
        public async Task<IActionResult> EditMembership(ProjectApproval model)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", model.Id.ToString());
            var project = _treclientHelper.CallAPIWithoutModel<ProjectApproval?>(
                "/api/Project/GetProjectApproval/", paramlist).Result;

            var resultTre =
                await _treclientHelper.CallAPI<ProjectApproval, ProjectApproval?>("/api/Project/EditProjectApproval", model);

            var result =
                await _treclientHelper.CallAPI < ProjectApproval, ProjectApproval?>("/api/Project/AddUserMembership", model);
            
            //result = GetProjectUserModel();


            //return View(result);

            return RedirectToAction("GetAllProjectsForApproval");
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
            return RedirectToAction("GetAllProjectsForApproval");
        }

        [HttpGet]
        public async Task<IActionResult> EditProject(int? projectId)
        {
          
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", projectId.ToString());
            var project = _treclientHelper.CallAPIWithoutModel<ProjectApproval?>(
                "/api/Project/GetProjectApproval/", paramlist).Result;


            return View(project);
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
