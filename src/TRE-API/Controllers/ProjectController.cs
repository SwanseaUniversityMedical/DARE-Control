using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BL.Models;
using BL.Models.ViewModels;
using Serilog;
using Microsoft.Extensions.Options;
using BL.Services;
using TRE_API.Repositories.DbContexts;
using TRE_API.Services;

namespace TRE_API.Controllers
{
    
    [Authorize(Roles = "dare-tre-admin")]
    [Route("api/[controller]")]
    [ApiController]

    public class ProjectController : Controller
    {

        private readonly ApplicationDbContext _DbContext;

        
        private readonly IDareClientWithoutTokenHelper _dareclientHelper;
  
        public ProjectController(IDareClientWithoutTokenHelper dareclient, ApplicationDbContext applicationDbContext)
        {
            _dareclientHelper = dareclient;
            _DbContext = applicationDbContext;

        }  

        [HttpGet("GetAllProjects")]
        [Authorize(Roles = "dare-tre-admin")]
        public List<Project> GetAllProjects()
        {
            
            var allProjects =  _dareclientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjectsForTre/").Result;
            return allProjects;
        }

        [HttpGet("GetProject")]
        public Project? GetProject(int? projectId)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", projectId.ToString());
            var Projects = _dareclientHelper.CallAPIWithoutModel<Project?>("/api/Project/GetProject/", paramlist).Result;
            return Projects;

        }
        [HttpGet("GetUser")]
        public User? GetUser(int? userId)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("userId", userId.ToString());
            var User = _dareclientHelper.CallAPIWithoutModel<User?>("/api/User/GetUser/", paramlist).Result;
            return User;

        }
        [HttpGet("GetAllProjectsForApproval")]
        public List<Project> GetAllProjectsForApproval()
        {
            try
            {
                var allProjects = _dareclientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjectsForTre/").Result;
                return allProjects;
                
                 }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllProjectsForApproval");
                throw;
            }


        }
        [HttpGet("GetAllUsers")]
        public List<User> GetAllUsers()
        {

            var allUsers = _dareclientHelper.CallAPIWithoutModel<List<User>>("/api/User/GetAllUsers/").Result;
            return allUsers;
        }

        [HttpGet("IsUserApprovedonProject")]
        public bool IsUserApprovedonProject(int projectId, int userId)
        {
            try
            {
                bool IsUserApprovedonProject = _DbContext.ProjectApprovals.Any(p => p.Approved == "Approved" && p.Users.Any(u => u.Id == userId));
                return IsUserApprovedonProject;
            }

            catch (Exception ex)
            {
                Log.Error(ex, "{Function} crash", "IsUserApprovedonProject");
                throw;
            }
        }


        [HttpPost("ApproveProjectMembership")]
        public async Task<ProjectApproval?> ApproveProjectMembership(Project model)
        {
            try

            {
                var approved = IsUserApprovedonProject(model.Id, model.Id);
                 //edit userId above  
                var paramlist = new Dictionary<string, string>();
                paramlist.Add("projectId", model.Id.ToString());
                var Projects = _dareclientHelper.CallAPIWithoutModel<Project?>("/api/Project/GetProject/", paramlist).Result;

                
                var projectapproval = new ProjectApproval();

                if (approved == true && Projects == null)
                {
                    projectapproval.Approved = "Archive";
                   if(_DbContext.ProjectApprovals.Select(x => x.Id == model.Id).Any())
                    _DbContext.ProjectApprovals.Update(projectapproval);
                    else
                        _DbContext.ProjectApprovals.Add(projectapproval);
                }
                else
                {
                    projectapproval.Date = DateTime.Now.ToUniversalTime();
                    projectapproval.ProjectId = model.Id;
                    //pass username below
                    //proj.UserId = model.Users.;
                    projectapproval.Projectname = model.Name;
                    //proj.Username = model.Username;
                    projectapproval.LocalProjectName = model.FormData;
                    projectapproval.Approved = model.OutputBucket;


                    _DbContext.ProjectApprovals.Add(projectapproval);
                }
                await _DbContext.SaveChangesAsync();

                Log.Information("{Function} Membership Request added successfully", "MembershipRequest");
                return projectapproval;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "ApproveProjectMembership");
                var errorModel = new ProjectApproval();
                return errorModel;
                throw;
            }

        }

     
        [AllowAnonymous]
        [HttpGet("GetAllMemberships")]
        public List<ProjectApproval> GetAllMemberships()
        {
            try 
            {     
                var generalmemberships = _DbContext.ProjectApprovals.ToList(); ;
                if (generalmemberships == null)
                {
                    return null;
                }

                Log.Information("{Function} Projects retrieved successfully", "GetProject");
                return generalmemberships;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllMemberships");
                throw;
            }


        }

        [HttpGet("GetAllDisabledMemberships")]
        public List<ProjectApproval> GetAllDisabledMemberships()
        {
            try
            {
                var returned = _DbContext.ProjectApprovals.Where(p => p.Approved == "Archived").ToList();
                if (returned == null)
                {
                    return null;
                }

                Log.Information("{Function} Projects retrieved successfully", "GetProject");
                return returned;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllDisabledMemberships");
                throw;
            }

        }

        [HttpGet("GetAllUnApprovedMemberships")]
        public List<ProjectApproval> GetAllUnApprovedMemberships()
        {
            try
            {
                var returned = _DbContext.ProjectApprovals.Where(p => p.Approved != "Approved").ToList();
                if (returned == null)
                {
                    return null;
                }

                Log.Information("{Function} Projects retrieved successfully", "GetProject");
                return returned;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllUnApprovedMembership");
                throw;
            }


        }



    }
}
