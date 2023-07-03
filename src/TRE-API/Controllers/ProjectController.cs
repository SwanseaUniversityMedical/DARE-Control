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




        [HttpPost("AddProject")]
        public async Task<Project?> AddProject(FormData data)
        {
            try
            {

                Project projects = JsonConvert.DeserializeObject<Project>(data.FormIoString);


                var model = new Project();

                //2023-06-01 14:30:00 use this as the datetime
                model.Name = projects.Name.Trim();
                if (_DbContext.Projects.Any(x => x.Name.ToLower() == model.Name.ToLower().Trim()))
                {
                    return null;
                }
                model.StartDate = projects.StartDate.ToUniversalTime();

                model.EndDate = projects.EndDate.ToUniversalTime();

                model.SubmissionBucket = GenerateRandomName(model.Name);
                model.OutputBucket = GenerateRandomName(model.Name);
                model.FormData = data.FormIoString;


                _DbContext.Projects.Add(model);

                await _DbContext.SaveChangesAsync();

                Log.Information("{Function} Projects added successfully", "CreateProject");
                return model;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "AddProject");
                var errorModel = new Project();
                return errorModel;
                throw;
            }


        }


        //[HttpPost("AddUserMembership")]
        //public async Task<ProjectUser?> AddUserMembership(ProjectUser model)
        //{
        //    try
        //    {
        //        var user = _DbContext.Users.FirstOrDefault(x => x.Id == model.UserId);
        //        if (user == null)
        //        {
        //            Log.Error("{Function} Invalid user id {UserId}", "AddUserMembership", model.UserId);
        //            return null;
        //        }

        //        var project = _DbContext.Projects.FirstOrDefault(x => x.Id == model.ProjectId);
        //        if (project == null)
        //        {
        //            Log.Error("{Function} Invalid project id {UserId}", "AddUserMembership", model.ProjectId);
        //            return null;
        //        }

        //        if (project.Users.Any(x => x == user))
        //        {
        //            Log.Error("{Function} User {UserName} is already on {ProjectName}", "AddUserMembership", user.Name, project.Name);
        //            return null;
        //        }

        //        project.Users.Add(user);

        //        await _DbContext.SaveChangesAsync();
        //        Log.Information("{Function} Added User {UserName} to {ProjectName}", "AddUserMembership", user.Name, project.Name);
        //        return model;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "{Function} Crash", "AddUserMembership");
        //        throw;
        //    }


        //}




        [HttpGet("GetProject")]
        public Project? GetProject(int projectId)
        {
            try
            {
                var returned = _DbContext.Projects.Find(projectId);
                if (returned == null)
                {
                    return null;
                }

                Log.Information("{Function} Project retrieved successfully", "GetProject");
                return returned;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetProject");
                throw;
            }


        }

        [HttpGet("GetAllProjects")]
        public List<Project> GetAllProjects()
        {
            try
            {

                var allProjects = _DbContext.Projects
                    //.Include(x => x.Endpoints)
                    //.Include(x => x.Submissions)
                    //.Include(x => x.Users)
                    .ToList();


                Log.Information("{Function} Projects retrieved successfully", "GetAllProjects");
                return allProjects;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllProjects");
                throw;
            }


        }

       

        private static string GenerateRandomName(string prefix)
        {
            Random random = new Random();
            string randomName = prefix + random.Next(1000, 9999);
            return randomName;
        }

    }
}
