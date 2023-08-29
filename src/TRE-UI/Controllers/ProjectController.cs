using BL.Models;
using BL.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using EasyNetQ.Management.Client.Model;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace TRE_UI.Controllers
{
    [Authorize]
    public class ProjectController : Controller
    {
        
        private readonly ITREClientHelper _treclientHelper;
        public ProjectController(ITREClientHelper treClient)
        {
            
            _treclientHelper = treClient;
        }
        
     
        [HttpGet]
        public IActionResult GetAllProjects()
        
        {
            var projects = _treclientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjects/").Result;

            return View(projects);
        }

        [HttpGet]
        public IActionResult GetAllProjectsForApproval()
        {

            var projects = _treclientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjectsForApproval/").Result;

            return View(projects);
        }

        public IActionResult EditProject(int? projectId)
        {

            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", projectId.ToString());
            var project = _treclientHelper.CallAPIWithoutModel<Project?>(
                "/api/Project/GetProject/", paramlist).Result;
            

            var projectView = new Project()

            {
                Id = project.Id,
                    Name = project.Name, 
                     Users = project.Users,
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    Submissions = project.Submissions

                };
          
            return View(projectView);
        }

        [HttpPost]
        public async Task<IActionResult> EditProjectSubmission(Project model)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", model.Id.ToString());
        
            var result = await _treclientHelper.CallAPI<Project, Project?>("/api/Project/ApproveProjectMembership", model);

            return RedirectToAction("GetAllProjectsForApproval");
        }

        public IActionResult GetUser(int id)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("userId", id.ToString());
            var result = _treclientHelper.CallAPIWithoutModel<BL.Models.User?>(
                "/api/User/GetUser/", paramlist).Result;

            return View(result);
        }
        public IActionResult GetProject(int id)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", id.ToString());
            var result = _treclientHelper.CallAPIWithoutModel<BL.Models.Project?>(
                "/api/Project/GetProject/", paramlist).Result;

            return View(result);
        }

        [HttpGet]
        public IActionResult GetAllUnApprovedMemberships()
        {

            var projects = _treclientHelper.CallAPIWithoutModel<List<ProjectApproval>>("/api/Project/GetAllUnApprovedMemberships/").Result;

            return View(projects);
        }

        [HttpGet]
        public IActionResult GetAllDisabledMemberships()
        {

            var projects = _treclientHelper.CallAPIWithoutModel<List<ProjectApproval>>("/api/Project/GetAllDisabledMemberships/").Result;

            return View(projects);
        }

        [HttpGet]
        public IActionResult GetAllMemberships()
        {

            var projects = _treclientHelper.CallAPIWithoutModel<List<ProjectApproval>>("/api/Project/GetAllMemberships/").Result;

            return View(projects);
        }



    }

}
