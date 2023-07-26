using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BL.Repositories.DbContexts;
using BL.Models;
using System.Text.Json.Nodes;
using BL.Models.DTO;
using BL.Rabbit;
using Newtonsoft.Json;
using TRE_API.Controllers;
using TRE_API.Models;
using EasyNetQ;
using Serilog;

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
                proj.ProjectId = 3;
                proj.UserId = 3;
                proj.Projectname = "Project3";
                proj.Username = "User3";
                proj.LocalProjectName = "testb";

                //proj.ProjectId = model.ProjectId;
                //proj.UserId = model.UserId;
                //proj.Projectname = model.Projectname;
                //proj.Username = model.Username;
                // proj.LocalProjectName =model.LocalProjectName;


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
