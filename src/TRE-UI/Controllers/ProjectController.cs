using BL.Models;
using BL.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;



namespace TRE_UI.Controllers
{
    [Authorize(Roles = "dare-tre-admin")]
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
            
              string Approval = Request.Form["foo"].ToString();

            var paramlist = new Dictionary<string, string>();
            paramlist.Add("Approval", Approval);
        
            var result = await _treclientHelper.CallAPI<Project, Project?>("/api/Project/ApproveProjectMembership", model,paramlist);

            return RedirectToAction("GetAllProjectsForApproval");
        }

        public IActionResult GetUser(int id)
        {
            var projects = _treclientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjects/").Result;
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("userId", id.ToString());
            var result = _treclientHelper.CallAPIWithoutModel<BL.Models.User?>(
                "/api/Project/GetUser/", paramlist).Result;

            var projectItems2 = projects.Where(p => !result.Projects.Select(x => x.Id).Contains(p.Id)).ToList();

            var projectItems = projectItems2
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();
           

            ViewBag.ProjectItems = projectItems;

            return View(result);
        }
        public IActionResult GetProject(int id)
        {
            var users = _treclientHelper.CallAPIWithoutModel<List<BL.Models.User>>("/api/Project/GetAllUsers/").Result;
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", id.ToString());
            var project = _treclientHelper.CallAPIWithoutModel<Project?>(
                "/api/Project/GetProject/", paramlist).Result;

            var userItems2 = users.Where(p => !project.Users.Select(x => x.Id).Contains(p.Id)).ToList();
           
            var userItems = userItems2
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();
          
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
                Submissions = project.Submissions,
                UserItemList = userItems,
              
            };

            return View(projectView);
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
