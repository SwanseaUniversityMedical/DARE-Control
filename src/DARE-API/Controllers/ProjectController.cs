using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using DARE_API.Repositories.DbContexts;
using BL.Models;
using BL.Models.ViewModels;

using Newtonsoft.Json;
using Serilog;
using Endpoint = BL.Models.Endpoint;
using BL.Services;


namespace DARE_API.Controllers
{

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
        public async Task<Project?> AddProject([FromBody] FormData data)
        {
            try
            {
           
                Project project = JsonConvert.DeserializeObject<Project>(data.FormIoString);
                //2023-06-01 14:30:00 use this as the datetime
                project.Name = project.Name.Trim();
                project.StartDate = project.StartDate.ToUniversalTime();
                project.EndDate = project.EndDate.ToUniversalTime();
                project.ProjectDescription = project.ProjectDescription.Trim();
                //projects.Id = projects.Id;
                
                
                project.FormData = data.FormIoString;
                project.Display = project.Display;
                //model.Display = projects.Display.Trim();
                if (_DbContext.Projects.Any(x => x.Name.ToLower() == project.Name.ToLower().Trim() && x.Id != project.Id))
                {

                    return new Project() { Error = true, ErrorMessage = "Another project already exists with the same name" };
                }

                if (project.Id == 0)
                {
                    project.SubmissionBucket = GenerateRandomName(project.Name) + "submission";
                    project.OutputBucket = GenerateRandomName(project.Name) + "output";
                    var submissionBucket = await _minioHelper.CreateBucket(_minioSettings, project.SubmissionBucket);
                    if (!submissionBucket)
                    {
                        Log.Error("{Function} S3GetListObjects: Failed to create bucket {name}.", "AddProject", project.SubmissionBucket);
                    }
                    else
                    {
                        var submistionBucketPolicy = await _minioHelper.CreateBucketPolicy(project.SubmissionBucket);
                        if (!submistionBucketPolicy)
                        {
                            Log.Error("{Function} CreateBucketPolicy: Failed to create policy for bucket {name}.", "AddProject", project.SubmissionBucket);
                        }
                    }
                    var outputBucket = await _minioHelper.CreateBucket(_minioSettings, project.OutputBucket);
                    if (!outputBucket)
                    {
                        Log.Error("{Function} S3GetListObjects: Failed to create bucket {name}.", "AddProject", project.OutputBucket);

                    }
                    else
                    {
                        var outputBucketPolicy = await _minioHelper.CreateBucketPolicy(project.OutputBucket);
                        if (!outputBucketPolicy)
                        {
                            Log.Error("{Function} CreateBucketPolicy: Failed to create policy for bucket {name}.", "AddProject", project.OutputBucket);
                        }
                    }
                }
               

                if (project.Id > 0)
                {
                    if (_DbContext.Projects.Select(x => x.Id == project.Id).Any())
                        _DbContext.Projects.Update(project);
                    else
                       _DbContext.Projects.Add(project);
                }
                else { 
                    _DbContext.Projects.Add(project);

                }

                await _DbContext.SaveChangesAsync();

                Log.Information("{Function} Projects added successfully", "CreateProject");
                return project;
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
                //TODO - use User.Identity.IsAuthenticated to alter list returned : embargoed etc

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


        [HttpGet("GetAllProjectsForEndpoint")]
        [AllowAnonymous]
        public List<Project> GetAllProjectsForEndpoint()
        {
            try
            {
                
                var usersName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First();
                var endpoint = _DbContext.Endpoints.FirstOrDefault(x => x.AdminUsername.ToLower() == usersName.ToLower());
                if (endpoint == null)
                {
                    throw new Exception("User " + usersName + " doesn't have an endpoint");
                    
                }

                var allProjects = endpoint.Projects;

                Log.Information("{Function} Projects retrieved successfully", "GetAllProjectsForEndpoint");
                return allProjects;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllProjectsForEndpoint");
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

        [HttpGet("GetMinioEndPoint")]
        public MinioEndpoint? GetMinioEndPoint()
        {

            var minioEndPoint = new MinioEndpoint()
            {
                Url = _minioSettings.Url,
            };  

            return minioEndPoint;
        }


        //End

    }
}
