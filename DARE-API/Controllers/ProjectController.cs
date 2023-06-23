using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BL.Repositories.DbContexts;
using BL.Models;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using DARE_API.Controllers;
using static BL.Controllers.UserController;
using Minio.DataModel;
using DARE_API.Services.Contract;
using DARE_API.Models;
using Serilog;

namespace DARE_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]

    public class ProjectController : ControllerBase
    {

        private readonly ApplicationDbContext _DbContext;
        private readonly MinioSettings _minioSettings;
        private readonly IMinioService _minioService;

        private readonly ILogger<ProjectController> _logger;

    

        public ProjectController(ApplicationDbContext applicationDbContext, MinioSettings minioSettings, IMinioService minioService)
        {

            _DbContext = applicationDbContext;
            _minioSettings = minioSettings;
            _minioService = minioService;
        }


        [HttpGet("HelloWorld")]

        public IActionResult HelloWorld()
        {
            return Ok();
        }


        //[HttpPost("Save_Project")]

        //public async Task<Projects> CreateProject([FromBody] JsonObject project)
        //{
        //    try
        //    {
        //        string jsonString = project.ToString();
        //        Projects projects = JsonConvert.DeserializeObject<Projects>(jsonString);

        //        //Projects projects = JsonConvert.DeserializeObject<Projects>(project);
        //        var model = new Projects();
        //        //2023-06-01 14:30:00 use this as the datetime
        //        model.Name = projects.Name;
        //        model.StartDate = projects.StartDate.ToUniversalTime();
        //        //model.Users = projects.Users.ToList();
        //        model.EndDate = projects.EndDate.ToUniversalTime();

        //        _DbContext.Projects.Add(model);

        //        await _DbContext.SaveChangesAsync();


        //        return model;
        //    }
        //    catch (Exception ex) { }

        //    return null;
        //}

        [HttpPost("Save_Project")]

        public async Task<Projects> CreateProject(data data)
        {
            try
            {
                Projects projects = JsonConvert.DeserializeObject<Projects>(data.FormIoString);

                //Projects projects = JsonConvert.DeserializeObject<Projects>(project);
                var model = new Projects();
                //2023-06-01 14:30:00 use this as the datetime
                model.Name = projects.Name;
                model.StartDate = projects.StartDate.ToUniversalTime();
                //model.Users = projects.Users.ToList();
                model.EndDate = projects.EndDate.ToUniversalTime();

                model.SubmissionBucket = GenerateRandomName(model.Name);
                model.OutputBucket = GenerateRandomName(model.Name);

                var submissionBucket = await _minioService.CreateBucket(_minioSettings, model.SubmissionBucket);
                if (!submissionBucket)
                {
                    Log.Error("S3GetListObjects: Failed to create bucket {name}.", model.SubmissionBucket);
                }
                var outputBucket = await _minioService.CreateBucket(_minioSettings, model.OutputBucket);
                if (!outputBucket)
                {
                    Log.Error("S3GetListObjects: Failed to create bucket {name}.", model.OutputBucket);
                }

                _DbContext.Projects.Add(model);

                await _DbContext.SaveChangesAsync();

                _logger.LogInformation("Projects added successfully");
                return model;
            }
            catch (Exception ex) {
                _logger.LogError(ex.Message.ToString());
            }
           
            return null;
        }

        [HttpPost("Add_Membership")]

        public async Task<ProjectMembership> AddMembership(int userid, int projectid)
        {
            try
            { 
            var membership = new ProjectMembership();
            //var theuser =

            membership.Users = await _DbContext.Users.SingleAsync(x => x.Id == userid);
            membership.Projects = await _DbContext.Projects.SingleAsync(x => x.Id == projectid);

            //membership.Id = 1;
            _DbContext.ProjectMemberships.Add(membership);
            await _DbContext.SaveChangesAsync();
                _logger.LogInformation("Memberships added successfully");
                return membership;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
            }

            return null;
        }

        [HttpGet("Get_AllMemberships")]

        public List<ProjectMembership> GetAllProjectMemberships()
        {
            try { 
            var allMemberships = _DbContext.ProjectMemberships.ToList();

            foreach (var memberships in allMemberships)
            {
                var Users = memberships.Users;
                var Projects = memberships.Projects;
            }
                //return returned.FirstOrDefault();
                _logger.LogInformation("Memberships retrieved successfully");
                return allMemberships;
        }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString()
                    );
            }

            return null;
        }

[HttpGet("Get_Membership/{userid}")]

        public ProjectMembership GetMembership(int userid)
        {
            try {
            var membership = _DbContext.ProjectMemberships.Find(userid);
            if (membership == null)
            {
                return null;
            }
                //return returned.FirstOrDefault();
                _logger.LogInformation("Membership retrieved successfully");
                return membership;
        }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
            }

            return null;
        }


[HttpGet("Get_Project/{projectId}")]

        public Projects GetProject(int projectId)
        {
            try
            {
                var returned = _DbContext.Projects.Find(projectId);
                if (returned == null)
                {
                    return null;
                }
                //return returned.FirstOrDefault();
                _logger.LogInformation("Project retrieved successfully");
                return returned;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
            }

            return null;
        }


        [HttpGet("Get_AllProjects")]

        public List<Projects> GetAllProjects()
        {
            try
            {

                var allProjects = _DbContext.Projects.ToList();

                foreach (var project in allProjects)
                {
                    var id = project.Id;
                    var name = project.Name;
                }
                //return returned.FirstOrDefault();
                _logger.LogInformation("Projects retrieved successfully");
                return allProjects;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
            }

            return null;
        }

        [HttpGet("Get_AllEndPoints/{projectId}")]

        public List<Endpoints> GetEndPointsInProject(int projectId)
        {
            List<Endpoints> endpoints = _DbContext.Projects.Where(p => p.Id == projectId).SelectMany(p => p.Endpoints).ToList();

            //var returned = _DbContext.Projects.Find(projectId);
            //if (returned == null)
            //{
            //    return null;
            //}           
            return endpoints;
        }

        public static string GenerateRandomName(string prefix)
        {
            Random random = new Random();
            string randomName = prefix + random.Next(1000, 9999);
            return randomName;
        }

        //this is going to be used but commented out until I can fix it
        //[HttpPost("Add_EndpointsToProject")]

        //public async Task<Endpoints> AddEndpointsToProject(data data)
        //{
        //    try
        //    {
        //        Endpoints endpoints = JsonConvert.DeserializeObject<Endpoints>(data.FormIoString);

        //        var model = new Endpoints();
        //        model.Name = endpoints.Name;
        //        //model.Projects = endpoints.Projects.ToList();

        //        _DbContext.Endpoints.Add(model);

        //        await _DbContext.SaveChangesAsync();


        //        return model;
        //    }
        //    catch (Exception ex) { }

        //    return null;


        //}

    }
}
