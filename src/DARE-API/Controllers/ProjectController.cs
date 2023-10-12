using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using DARE_API.Repositories.DbContexts;
using BL.Models;
using BL.Models.ViewModels;

using Newtonsoft.Json;
using Serilog;
using BL.Services;
using DARE_API.Services.Contract;
using Microsoft.AspNetCore.Authentication;
using BL.Models.Tes;
using EasyNetQ.Management.Client.Model;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;
using BL.Models.APISimpleTypeReturns;
using Amazon.Util.Internal;

namespace DARE_API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    
    
    public class ProjectController : Controller
    {

        private readonly ApplicationDbContext _DbContext;
        private readonly MinioSettings _minioSettings;
        private readonly IMinioHelper _minioHelper;
        private readonly IKeycloakMinioUserService _keycloakMinioUserService;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectController(ApplicationDbContext applicationDbContext, MinioSettings minioSettings, IMinioHelper minioHelper, IKeycloakMinioUserService keycloakMinioUserService, IHttpContextAccessor httpContextAccessor)
        {

            _DbContext = applicationDbContext;
            _minioSettings = minioSettings;
            _minioHelper = minioHelper;
            _keycloakMinioUserService = keycloakMinioUserService;
            _httpContextAccessor = httpContextAccessor;
        }

        [Authorize(Roles = "dare-control-admin")]
        [HttpPost("SaveProject")]
        public async Task<Project?> SaveProject([FromBody] FormData data)
        {
            try
            {
           
                Project project = JsonConvert.DeserializeObject<Project>(data.FormIoString);
                //2023-06-01 14:30:00 use this as the datetime
                project.Name = project.Name.Trim();
                project.StartDate = project.StartDate.ToUniversalTime();
                project.EndDate = project.EndDate.ToUniversalTime();

                project.ProjectDescription = project.ProjectDescription.Trim();
                project.MarkAsEmbargoed = project.MarkAsEmbargoed;
                
                
                

                project.FormData = data.FormIoString;
                project.Display = project.Display;
                
                if (_DbContext.Projects.Any(x => x.Name.ToLower() == project.Name.ToLower().Trim() && x.Id != project.Id))
                {

                    return new Project() { Error = true, ErrorMessage = "Another project already exists with the same name" };
                }

                if (project.Id == 0)
                {
                    project.SubmissionBucket = GenerateRandomName(project.Name.ToLower()) + "submission";
                    project.OutputBucket = GenerateRandomName(project.Name.ToLower()) + "output";
                    var submissionBucket = await _minioHelper.CreateBucket(_minioSettings, project.SubmissionBucket);
                    if (!submissionBucket)
                    {
                        Log.Error("{Function} S3GetListObjects: Failed to create bucket {name}.", "SaveProject", project.SubmissionBucket);
                    }
                    else
                    {
                        var submistionBucketPolicy = await _minioHelper.CreateBucketPolicy(project.SubmissionBucket);
                        if (!submistionBucketPolicy)
                        {
                            Log.Error("{Function} CreateBucketPolicy: Failed to create policy for bucket {name}.", "SaveProject", project.SubmissionBucket);
                        }
                    }
                    var outputBucket = await _minioHelper.CreateBucket(_minioSettings, project.OutputBucket);
                    if (!outputBucket)
                    {
                        Log.Error("{Function} S3GetListObjects: Failed to create bucket {name}.", "SaveProject", project.OutputBucket);

                    }
                    else
                    {
                        var outputBucketPolicy = await _minioHelper.CreateBucketPolicy(project.OutputBucket);
                        if (!outputBucketPolicy)
                        {
                            Log.Error("{Function} CreateBucketPolicy: Failed to create policy for bucket {name}.", "SaveProject", project.OutputBucket);
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
                var audit = new AuditLog()
                {
                    FormData = data.FormIoString,
                    IPaddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                    UserName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First(),
                    ProjectId = project.Id,           
                    Date = DateTime.Now.ToUniversalTime()
                };

                _DbContext.AuditLogs.Add(audit);
                Log.Information("{Function}:", "AuditLogs","SaveProject", "FormData: " + data.FormIoString + "ProjectId:" + project.Id  + @User?.FindFirst("name")?.Value);              
                await _DbContext.SaveChangesAsync();
                Log.Information("{Function} Projects added successfully", "CreateProject");
                return project;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "SaveProject");
                var errorModel = new Project();
                return errorModel;
                throw;
            }


        }

        [Authorize(Roles = "dare-control-admin")]
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

                var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
                var attributeName = _minioSettings.AttributeName;

                await _keycloakMinioUserService.SetMinioUserAttribute(accessToken, user.Name.ToString(), attributeName, project.SubmissionBucket.ToLower() + "_policy");

                await _keycloakMinioUserService.SetMinioUserAttribute(accessToken, user.Name.ToString(), attributeName, project.OutputBucket.ToLower() + "_policy");
                var audit = new AuditLog()
                {
                    IPaddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                    UserName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First(),
                    ProjectId = project.Id,
                    UserId = user.Id,
                    Date = DateTime.Now.ToUniversalTime()
                };
                _DbContext.AuditLogs.Add(audit);
                Log.Information("{Function}:", "AuditLogs","AddUserMembership", "UserId: " + user.Id + "ProjectId:" + project.Id + @User?.FindFirst("name")?.Value);

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

        [Authorize(Roles = "dare-control-admin")]
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

                var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
                var attributeName = _minioSettings.AttributeName;
                var attributeValue = project.Name.ToLower() + "_policy";

                await _keycloakMinioUserService.RemoveMinioUserAttribute(accessToken, user.Name.ToString(), attributeName, project.SubmissionBucket.ToLower() + "_policy");
                await _keycloakMinioUserService.RemoveMinioUserAttribute(accessToken, user.Name.ToString(), attributeName, project.OutputBucket.ToLower() + "_policy");
                var audit = new AuditLog()
                {
                    IPaddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                    UserName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First(),
                    ProjectId = project.Id,
                    UserId = user.Id,
                    Date = DateTime.Now.ToUniversalTime()
                };
                _DbContext.AuditLogs.Add(audit);
                Log.Information("{Function}:", "AuditLogs", "RemoveUserMembership", "ProjectId:" + project.Id + " UserId:" + user.Id  + @User?.FindFirst("name")?.Value);

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

        [Authorize(Roles = "dare-control-admin")]
        [HttpPost("AddTreMembership")]
        public async Task<ProjectTre?> AddTreMembership(ProjectTre model)
        {
            try
            {
                var tre = _DbContext.Tres.FirstOrDefault(x => x.Id == model.TreId);
                if (tre == null)
                {
                    Log.Error("{Function} Invalid tre id {UserId}", "AddTreMembership", model.TreId);
                    return null;
                }

                var project = _DbContext.Projects.FirstOrDefault(x => x.Id == model.ProjectId);
                if (project == null)
                {
                    Log.Error("{Function} Invalid project id {UserId}", "AddTreMembership", model.ProjectId);
                    return null;
                }

                if (project.Tres.Any(x => x == tre))
                {
                    Log.Error("{Function} Tre {Tre} is already on {ProjectName}", "AddTreMembership", tre.Name, project.Name);
                    return null;
                  }

                project.Tres.Add(tre);
                var audit = new AuditLog()
                {
                    IPaddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                    UserName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First(),
                    ProjectId = project.Id,
                    TreId = tre.Id,
                    Date = DateTime.Now.ToUniversalTime()
                };
                _DbContext.AuditLogs.Add(audit);
                Log.Information("{Function}:", "AuditLogs", "AddTreMembership", "ProjectId:" + project.Id + " TreId:" + tre.Id + @User?.FindFirst("name")?.Value);
                await _DbContext.SaveChangesAsync();
                Log.Information("{Function} Added Tre {Tre} to {ProjectName}", "AddTreMembership", tre.Name, project.Name);
                return model;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "AddTreMembership");
                throw;
            }


        }

        [Authorize(Roles = "dare-control-admin")]
        [HttpPost("RemoveTreMembership")]
        public async Task<ProjectTre?> RemoveTreMembership(ProjectTre model)
        {
            try
            {
                var tre = _DbContext.Tres.FirstOrDefault(x => x.Id == model.TreId);
                if (tre == null)
                {
                    Log.Error("{Function} Invalid tre id {UserId}", "AddTreMembership", model.TreId);
                    return null;
                }

                var project = _DbContext.Projects.FirstOrDefault(x => x.Id == model.ProjectId);
                if (project == null)
                {
                    Log.Error("{Function} Invalid project id {UserId}", "AddTreMembership", model.ProjectId);
                    return null;
                }

                if (!project.Tres.Any(x => x == tre))
                {
                    Log.Error("{Function} Tre {Tre} is already on {ProjectName}", "AddTreMembership", tre.Name, project.Name);
                    return null;
                }

                project.Tres.Remove(tre);
                var audit = new AuditLog()
                {
                    IPaddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                    UserName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First(),
                    ProjectId = project.Id,
                    TreId = tre.Id,
                    Date = DateTime.Now.ToUniversalTime()
                };
                _DbContext.AuditLogs.Add(audit);
                Log.Information("{Function}:", "AuditLogs", "RemoveTreMembership", "ProjectId:" + project.Id + " TreId:" + tre.Id + @User?.FindFirst("name")?.Value);

                await _DbContext.SaveChangesAsync();
                Log.Information("{Function} Added Tre {Tre} to {ProjectName}", "AddTreMembership", tre.Name, project.Name);
                return model;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "AddTreMembership");
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
                    //.Include(x => x.Tres)
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


        [HttpGet("GetAllProjectsForTre")]
        [Authorize(Roles = "dare-tre-admin")]
        public List<Project> GetAllProjectsForTre()
        {
            try
            {
                
                var usersName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First();
                var tre = _DbContext.Tres.FirstOrDefault(x => x.AdminUsername.ToLower() == usersName.ToLower());
                if (tre == null)
                {
                    throw new Exception("User " + usersName + " doesn't have a tre");
                    
                }

                var allProjects = tre.Projects;

                Log.Information("{Function} Projects retrieved successfully", "GetAllProjectsForTre");
                return allProjects;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllProjectsForTre");
                throw;
            }


        }

        [HttpGet("GetTresInProject")]
        [AllowAnonymous]
        public List<Tre> GetTresInProject(int projectId)
        {
            List<Tre> tres = _DbContext.Projects.Where(p => p.Id == projectId).SelectMany(p => p.Tres).ToList();

            return tres;
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

        [Authorize(Roles = "dare-control-admin")]
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
        [AllowAnonymous]
        public MinioEndpoint? GetMinioEndPoint()
        {

            var minioEndPoint = new MinioEndpoint()
            {
                Url = _minioSettings.AdminConsole,
            };  

            return minioEndPoint;
        }

        //[HttpGet("UploadToMinioOld")]
        //[AllowAnonymous]
        //public async Task<BoolReturn> UploadToMinioOld(string bucketName, string fileJson)
        //{
        //    IFormFile iFile = ConvertJsonToIFormFile(fileJson);

        //    var submissionBucket = await _minioHelper.UploadFileAsync(_minioSettings, iFile, bucketName, iFile.Name);

        //    return new BoolReturn();
        //}

        [HttpPost("UploadToMinio")]
        public async Task<BoolReturn> UploadToMinio(string bucketName, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return new BoolReturn() { Result = false };

            try
            {
                var submissionBucket = await _minioHelper.UploadFileAsync(_minioSettings, file, bucketName, file.Name);
                

                return new BoolReturn() { Result = true };
            }
            catch (Exception ex)
            {
                return new BoolReturn() { Result = false };
            }
        }


        private IFormFile ConvertJsonToIFormFile(string fileJson)
        {
            if (string.IsNullOrEmpty(fileJson))
                return null;
            var fileData = System.Text.Json.JsonSerializer.Deserialize<IFileData>(fileJson);

            var bytes = Convert.FromBase64String(fileData.Content);

            var fileName = fileData.FileName;
            var contentType = fileData.ContentType;

            var formFile = new FormFile(new MemoryStream(bytes), 0, bytes.Length, null, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };

            return formFile;
        }


        //End

    }
}
