using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BL.Models;
using BL.Models.ViewModels;
using Serilog;
using TRE_API.Repositories.DbContexts;

namespace TRE_API.Controllers
{
    //[Authorize]
    //[ApiController]
    [Route("api/[controller]")]

    public class ProjectController : ControllerBase
    {

        private readonly ApplicationDbContext _DbContext;

        public ProjectController(ApplicationDbContext applicationDbContext)
        {
            _DbContext = applicationDbContext;
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


                _DbContext.ProjectApprovals.Add(proj);

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


                var returned = _DbContext.ProjectApprovals.Find(model.Id);
                if( returned != null)
                    _DbContext.ProjectApprovals.Update(proj); ;
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

        
                var returned = _DbContext.ProjectApprovals.Find(projectId);
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

                var allApprovedProjects = _DbContext.ProjectApprovals
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
