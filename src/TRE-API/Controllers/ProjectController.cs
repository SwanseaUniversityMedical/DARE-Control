using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BL.Repositories.DbContexts;
using BL.Models;
using BL.Models.DTO;
using Serilog;
using Microsoft.Extensions.Options;
using BL.Services;

namespace TRE_API.Controllers
{
    //[Authorize]
    //[ApiController]
    [Authorize(Roles = "dare-tre,dare-control-admin")]
    [Route("api/[controller]")]

    public class ProjectController : ControllerBase
    {

        private readonly ApplicationDbContext _DbContext;

        private readonly DAREAPISettings _dareAPISettings;

        private readonly IDareClientHelper _dareclientHelper;


        public ProjectController(ApplicationDbContext applicationDbContext, IOptions<DAREAPISettings> APISettings, IDareClientHelper client)
        {
            _DbContext = applicationDbContext;
            _dareAPISettings = APISettings.Value;
            _dareclientHelper = client;
        }


        [HttpGet("GetAllProjects")]
        public List<Project> GetAllProjects()
        {
            try
            {
                var allProjects = _dareclientHelper.CallAPIWithoutModel<List<Project>>(_dareAPISettings.Address + "/api/Project/GetAllProjects/").Result;

                Log.Information("{Function} Projects retrieved successfully", "GetAllProjects");

                return allProjects;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllProjects");
                throw;
            }
        }

        [HttpGet("GetProject")]
        public Project? GetProject(int projectId)
        {
            try
            {
                var paramlist = new Dictionary<string, string>();
                paramlist.Add("projectId", projectId.ToString());
                var projects = _dareclientHelper.CallAPIWithoutModel<Project?>(_dareAPISettings.Address + "/api/Project/GetProject/", paramlist).Result;

                Log.Information("{Function} Projects retrieved successfully", "GetAllProjects");
                return projects;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetProject");
                throw;
            }
        }


        [HttpGet("GetAllUsers")]
        public List<User> GetAllUsers()
        {
            try
            {
                var allUsers = _dareclientHelper.CallAPIWithoutModel<List<User>>(_dareAPISettings.Address + "/api/User/GetAllUsers/").Result;
               
                Log.Information("{Function} Users retrieved successfully", "GetAllUsers");

                return allUsers;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllUsers");
                throw;
            }
        }

        [HttpPost("AddUserMembership")]
        public async Task<ProjectUser?> AddUserMembership(ProjectUser model)
        {
            try
            {
                var result = await _dareclientHelper.CallAPI<ProjectUser, ProjectUser?>(_dareAPISettings.Address + "/api/Project/AddUserMembership", model);
                Log.Information("{Function} Membership successfully", "AddUserMembership");

                return result;
            }

            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "AddUserMembership");
                throw;
            }
        }

        [HttpPost("RemoveUserMembership")]
        public async Task<ProjectUser?> RemoveUserMembership(int projectId, int userId)
        {
            try
            {
                var model = new ProjectUser()
                {
                    ProjectId = projectId,
                    UserId = userId
                };
                var result =
                    await _dareclientHelper.CallAPI<ProjectUser, ProjectUser?>(_dareAPISettings.Address + "/api/Project/RemoveUserMembership", model);
                Log.Information("{Function} Membership removed successfully", "RemoveUserMembership");

                return result;
            }

            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "RemoveUserMembership");
                throw;
            }
        }


        [HttpPost("RequestMembership")]
        public async Task<ProjectApproval?> RequestMembership(ProjectUserTre model)
        {
            try
            {

                var proj = new ProjectApproval();

                proj.Date = DateTime.Now.ToUniversalTime();
                //proj.ProjectId = 2;
                //proj.UserId = 2;
                //proj.Projectname = "Project2";
                //proj.Username = "User2";
                //proj.LocalProjectName = "testa";

                proj.ProjectId = model.ProjectId;
                proj.UserId = model.UserId;
                proj.Projectname = model.Projectname;
                proj.Username = model.Username;
                proj.LocalProjectName = model.LocalProjectName;


                _DbContext.ProjectApproval.Add(proj);

                await _DbContext.SaveChangesAsync();

                Log.Information("{Function} Membership Request added successfully", "MembershipRequest");
                return proj;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "RequestMembership");
                var errorModel = new ProjectApproval();
                return errorModel;
                throw;
            }


        }

        [HttpPost("EditProjectApproval")]
        public async Task<ProjectApproval?> EditProjectApproval(ProjectApproval model)
        {
            try
            {

                var proj = new ProjectApproval();

                proj.Id = model.Id;
                proj.ProjectId = model.ProjectId;
                proj.UserId = model.UserId;
                proj.Projectname = model.Projectname;
                proj.Username = model.Username;
                proj.LocalProjectName = model.LocalProjectName;
                proj.Approved = model.Approved;
                proj.Approved = model.ApprovedBy;
                proj.Date = DateTime.Now.ToUniversalTime(); 


                var returned = _DbContext.ProjectApproval.Find(model.Id);
                if( returned != null)
                    _DbContext.ProjectApproval.Update(proj); ;
                await _DbContext.SaveChangesAsync();

                Log.Information("{Function} Membership Request added successfully", "MembershipRequest");
                return proj;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "EditMembership");
                var errorModel = new ProjectApproval();
                return errorModel;
                throw;
            }


        }

        [AllowAnonymous]
        [HttpGet("GetProjectApproval")]
        public ProjectApproval? GetProjectApproval(int projectId)
        {
            try
            {

        
                var returned = _DbContext.ProjectApproval.Find(projectId);
                if (returned == null)
                {
                    return null;
                }

                Log.Information("{Function} Project retrieved successfully", "GetProject");
                return returned;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetProjectApproval");
                throw;
            }


        }
        [HttpGet("GetAllProjectsForApproval")]
        public List<ProjectApproval> GetAllProjectsForApproval()
        {
            try
            {

                var allApprovedProjects = _DbContext.ProjectApproval
                    //.Include(x => x.Approved)
                    .ToList();

                Log.Information("{Function} Projects retrieved successfully", "GetAllProjectForApproval");
                return allApprovedProjects;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllProjectForApproval");
                throw;
            }


        }

       
    }
}
