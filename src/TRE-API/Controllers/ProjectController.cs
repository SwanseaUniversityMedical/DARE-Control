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

        [HttpPost("MapUserToProject")]
        public async Task<ProjectUser?> MapUserToProject(ProjectUser model)
        {
            try
            {
                var user = _DbContext.Users.FirstOrDefault(x => x.Id == model.UserId);
                if (user == null)

                {
                    Log.Error("{Function} Invalid user id {UserId}", "MapUserToProject", model.UserId);
                    return null;
                }

                var project = _DbContext.Projects.FirstOrDefault(x => x.Id == model.ProjectId);
              

                if (project.Users.Any(x => x == user))
                {
                    Log.Error("{Function} User {UserName} is already on {ProjectName}", "MapUserToProject", user.Name, project.Name);
                    return null;
                }

                project.Users.Add(user);

                await _DbContext.SaveChangesAsync();
                Log.Information("{Function} Added User {UserName} to {ProjectName}", "MapUserToProject", user.Name, project.Name);
                return model;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "AddUserToProject");
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

        [HttpGet("ListAllProjects")]
        public List<Project> ListAllProjects()
        {
            try
            {

                var allProjects = _DbContext.Projects
                    .ToList();


                Log.Information("{Function} Projects retrieved successfully", "ListAllProjects");
                return allProjects;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "ListAllProjects");
                throw;
            }


        }
    }
}
