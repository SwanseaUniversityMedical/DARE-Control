using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BL.Repositories.DbContexts;
using BL.Models;
using System.Text.Json.Nodes;
using BL.Models.DTO;
using BL.Rabbit;
using Newtonsoft.Json;
using DARE_API.Controllers;
using static BL.Controllers.UserController;
using Minio.DataModel;
using DARE_API.Services.Contract;
using EasyNetQ;
using Serilog;
using Endpoint = BL.Models.Endpoint;
using BL.Services;

namespace DARE_API.Controllers
{
    //[Authorize]
    //[Authorize(Roles = "dare-control-admin")]
    [ApiController]
    [Route("api/[controller]")]

    public class ProjectController : ControllerBase
    {

        private readonly ApplicationDbContext _DbContext;
        private readonly MinioSettings _minioSettings;
        private readonly IMinioHelper _minioHelper;

        public ProjectController(ApplicationDbContext applicationDbContext, MinioSettings minioSettings, IMinioHelper minioHelper)
        {

            _DbContext = applicationDbContext;
            _minioSettings = minioSettings;
            _minioHelper = minioHelper;

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

                model.SubmissionBucket = GenerateRandomName(model.Name) + "submission";
                model.OutputBucket = GenerateRandomName(model.Name) + "output";
                model.FormData = data.FormIoString;

                var submissionBucket = await _minioHelper.CreateBucket(_minioSettings, model.SubmissionBucket);
                if (!submissionBucket)
                {
                    Log.Error("{Function} S3GetListObjects: Failed to create bucket {name}.", "AddProject", model.SubmissionBucket);
                }
                var outputBucket = await _minioHelper.CreateBucket(_minioSettings, model.OutputBucket);
                if (!outputBucket)
                {
                    Log.Error("{Function} S3GetListObjects: Failed to create bucket {name}.", "AddProject", model.OutputBucket);

                }


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
    
        [HttpPost("AddUserMembership")]
        public async Task<ProjectUser?> AddUserMembership(ProjectUser model)
        {
            try
            {
                var user = _DbContext.Users.FirstOrDefault(x => x.Id == model.UserId);
                if (user == null)
                {
                    Log.Error("{Function} Invalid user id {UserId}", "AddUserMembership", model.UserId);
                    return null;
                }

                var project = _DbContext.Projects.FirstOrDefault(x => x.Id == model.ProjectId);
                if (project == null)
                {
                    Log.Error("{Function} Invalid project id {UserId}", "AddUserMembership", model.ProjectId);
                    return null;
                }

                if (project.Users.Any(x => x == user))
                {
                    Log.Error("{Function} User {UserName} is already on {ProjectName}", "AddUserMembership", user.Name, project.Name);
                    return null;
                }

                project.Users.Add(user);

                await _DbContext.SaveChangesAsync();
                Log.Information("{Function} Added User {UserName} to {ProjectName}", "AddUserMembership", user.Name, project.Name);
                return model;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "AddUserMembership");
                throw;
            }


        }

        [HttpPost("RemoveUserMembership")]
        public async Task<ProjectUser?> RemoveUserMembership(ProjectUser model)
        {
            try
            {
                var user = _DbContext.Users.FirstOrDefault(x => x.Id == model.UserId);
                if (user == null)
                {
                    Log.Error("{Function} Invalid user id {UserId}", "RemoveUserMembership", model.UserId);
                    return null;
                }

                var project = _DbContext.Projects.FirstOrDefault(x => x.Id == model.ProjectId);
                if (project == null)
                {
                    Log.Error("{Function} Invalid project id {UserId}", "RemoveUserMembership", model.ProjectId);
                    return null;
                }

                if (!project.Users.Any(x => x == user))
                {
                    Log.Error("{Function} User {UserName} is not in the {ProjectName}", "RemoveUserMembership", user.Name, project.Name);
                    return null;
                }

                project.Users.Remove(user);

                await _DbContext.SaveChangesAsync();
                Log.Information("{Function} Added User {UserName} to {ProjectName}", "RemoveUserMembership", user.Name, project.Name);
                return model;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "RemoveUserMembership");
                throw;
            }


        }

        [HttpPost("AddEndpointMembership")]
        public async Task<ProjectEndpoint?> AddEndpointMembership(ProjectEndpoint model)
        {
            try
            {
                var endpoint = _DbContext.Endpoints.FirstOrDefault(x => x.Id == model.EndpointId);
                if (endpoint == null)
                {
                    Log.Error("{Function} Invalid endpoint id {UserId}", "AddEndpointMembership", model.EndpointId);
                    return null;
                }

                var project = _DbContext.Projects.FirstOrDefault(x => x.Id == model.ProjectId);
                if (project == null)
                {
                    Log.Error("{Function} Invalid project id {UserId}", "AddEndpointMembership", model.ProjectId);
                    return null;
                }

                if (project.Endpoints.Any(x => x == endpoint))
                {
                    Log.Error("{Function} Endpoint {Endpoint} is already on {ProjectName}", "AddEndpointMembership", endpoint.Name, project.Name);
                    return null;
                }

                project.Endpoints.Add(endpoint);

                await _DbContext.SaveChangesAsync();
                Log.Information("{Function} Added endpoint {Enpoint} to {ProjectName}", "AddEndpointMembership", endpoint.Name, project.Name);
                return model;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "AddEndpointMembership");
                throw;
            }


        }

        [HttpPost("RemoveEndpointMembership")]
        public async Task<ProjectEndpoint?> RemoveEndpointMembership(ProjectEndpoint model)
        {
            try
            {
                var endpoint = _DbContext.Endpoints.FirstOrDefault(x => x.Id == model.EndpointId);
                if (endpoint == null)
                {
                    Log.Error("{Function} Invalid endpoint id {UserId}", "AddEndpointMembership", model.EndpointId);
                    return null;
                }

                var project = _DbContext.Projects.FirstOrDefault(x => x.Id == model.ProjectId);
                if (project == null)
                {
                    Log.Error("{Function} Invalid project id {UserId}", "AddEndpointMembership", model.ProjectId);
                    return null;
                }

                if (!project.Endpoints.Any(x => x == endpoint))
                {
                    Log.Error("{Function} Endpoint {Endpoint} is already on {ProjectName}", "AddEndpointMembership", endpoint.Name, project.Name);
                    return null;
                }

                project.Endpoints.Remove(endpoint);

                await _DbContext.SaveChangesAsync();
                Log.Information("{Function} Added endpoint {Enpoint} to {ProjectName}", "AddEndpointMembership", endpoint.Name, project.Name);
                return model;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "AddEndpointMembership");
                throw;
            }


        }

        [AllowAnonymous]
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
        [AllowAnonymous]
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

        [HttpGet("GetEndPointsInProject")]
        [AllowAnonymous]
        public List<Endpoint> GetEndPointsInProject(int projectId)
        {
            List<Endpoint> endpoints = _DbContext.Projects.Where(p => p.Id == projectId).SelectMany(p => p.Endpoints).ToList();

            return endpoints;
        }

        private static string GenerateRandomName(string prefix)
        {
            Random random = new Random();
            string randomName = prefix + random.Next(1000, 9999);
            return randomName;
        }

        //For testing FetchAndStoreS3Object
        public class testFetch
        {
            public string url { get; set; }
            public string bucketName { get; set; }
            public string key { get; set; }
        }

        [HttpPost("TestFetchAndStoreObject")]
        public async Task<IActionResult> TestFetchAndStoreObject(testFetch testf)
        {
            await _minioHelper.FetchAndStoreObject(testf.url, _minioSettings, testf.bucketName, testf.key);

            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("IsUserOnProject")]
        public bool IsUserOnProject(int projectId, int userId)
        {
            try
            {
                bool isUserOnProject = _DbContext.Projects.Any(p => p.Id == projectId && p.Users.Any(u => u.Id == userId));
                return isUserOnProject;
            }

            catch (Exception ex)
            {
                Log.Error(ex, "{Function} crash", "IsUserOnProject");
                throw;
            }
        }

        //End

    }
}
