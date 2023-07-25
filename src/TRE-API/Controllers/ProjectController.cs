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
    [Authorize]
    [ApiController]
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

                //2023-06-01 14:30:00 use this as the datetime
                proj.ProjectId = model.ProjectId;
                proj.UserId = model.UserId;
                proj.Projectname = model.Projectname;
                proj.Username = model.Username;
                 proj.LocalProjectName =model.LocalProjectName;

              
               


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
