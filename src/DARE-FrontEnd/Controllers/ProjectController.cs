using BL.Models;
using BL.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using BL.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using BL.Models.Settings;
using Microsoft.AspNetCore.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace DARE_FrontEnd.Controllers
{
    [Authorize(Roles = "dare-control-admin")]
    public class ProjectController : Controller
    {
        private readonly IDareClientHelper _clientHelper;
        
        private readonly FormIOSettings _formIOSettings;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        public ProjectController(IDareClientHelper client, FormIOSettings formIo, IHttpContextAccessor httpContextAccessor)
        {
            _clientHelper = client;
            
            _formIOSettings = formIo;
            _httpContextAccessor = httpContextAccessor;

        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetProject(int id)
        {
            var users = _clientHelper.CallAPIWithoutModel<List<User>>("/api/User/GetAllUsers/").Result;
            var tres = _clientHelper.CallAPIWithoutModel<List<Tre>>("/api/Tre/GetAllTres/").Result;

            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", id.ToString());
            var project = _clientHelper.CallAPIWithoutModel<Project?>(
                "/api/Project/GetProject/", paramlist).Result;

            var userItems2 = users.Where(p => !project.Users.Select(x => x.Id).Contains(p.Id)).ToList();
            var treItems2 = tres.Where(p => !project.Tres.Select(x => x.Id).Contains(p.Id)).ToList();

            var userItems = userItems2
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();
            var treItems = treItems2
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            var minioEndpoint = _clientHelper.CallAPIWithoutModel<MinioEndpoint>("/api/Project/GetMinioEndPoint").Result;

            var projectView = new ProjectUserTre()
            {
                Id = project.Id,
                FormData = project.FormData,
                Name = project.Name,
                Users = project.Users,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                ProjectDescription = project.ProjectDescription,
                Tres = project.Tres,
                SubmissionBucket = project.SubmissionBucket,
                OutputBucket = project.OutputBucket,
                MinioEndpoint = minioEndpoint.Url,
                Submissions=project.Submissions,
                UserItemList = userItems,
                TreItemList = treItems
            };

            return View(projectView);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAllProjects()
        {
           
            
            var projects = _clientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjects/").Result;
            return View(projects);


        }

        [HttpGet]
        public IActionResult AddUserMembership()
        {

            var projmem = GetProjectUserModel();
            return View(projmem);
        }


        private ProjectUser GetProjectUserModel()
        {
            var projs = _clientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjects/").Result;
            var users = _clientHelper.CallAPIWithoutModel<List<User>>("/api/User/GetAllUsers/").Result;

            var projectItems = projs
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            var userItems = users
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            var projmem = new ProjectUser()
            {
                ProjectItemList = projectItems,
                UserItemList = userItems
            };
            return projmem;
        }

        [HttpGet]
        public IActionResult AddTreMembership()
        {

            var projmem = GetProjectTreModel();
            return View(projmem);
        }


        private ProjectTre GetProjectTreModel()
        {
            var projs = _clientHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjects/").Result;
            var users = _clientHelper.CallAPIWithoutModel<List<Tre>>("/api/Tre/GetAllTres/").Result;

            var projectItems = projs
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            var treItems = users
                .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
                .ToList();

            var projmem = new ProjectTre()
            {
                ProjectItemList = projectItems,
                TreItemList = treItems
            };
            return projmem;
        }

        [HttpPost]
        public async Task<IActionResult> AddUserMembership(ProjectUser model)
        {
            var result =
                await _clientHelper.CallAPI<ProjectUser, ProjectUser?>("/api/Project/AddUserMembership", model);
            result = GetProjectUserModel();
            var audit = new AuditLog()
            {
                FormData = "ProjectId: " + model.ProjectId.ToString() + " /User Id: " + model.UserId.ToString(),
                IPaddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                UserName = @User?.FindFirst("name")?.Value,
                Module = "ProjectUser",
                AuditValues = "Added UserMembership- " + "ProjectId: " + model.ProjectId.ToString() + " /User Id: " + model.UserId.ToString(),
                Action = "AddUserMembership",
                Date = DateTime.Now.ToUniversalTime()
            };
            var log = await _clientHelper.CallAPI<AuditLog, AuditLog?>("/api/Audit/SaveAuditLogs", audit);

            return View(result);


        }

        [HttpPost]
        public async Task<IActionResult> AddTreMembership(ProjectTre model)
        {
            var result =
                await _clientHelper.CallAPI<ProjectTre, ProjectTre?>("/api/Project/AddTreMembership",
                    model);
            result = GetProjectTreModel();
            var audit = new AuditLog()
            {
                FormData = "ProjectId: "+ model.ProjectId.ToString() +" /Tre Id: "+ model.TreId.ToString(),
                IPaddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                UserName = @User?.FindFirst("name")?.Value,
                Module = "ProjectTre",
                AuditValues = "Added TreMembership- " + "ProjectId: " + model.ProjectId.ToString() + " /Tre Id: " + model.TreId.ToString(),
                Action = "AddTreMembership",
                Date = DateTime.Now.ToUniversalTime()
            };
            var log = await _clientHelper.CallAPI<AuditLog, AuditLog?>("/api/Audit/SaveAuditLogs", audit);

            return View(result);


        }


        [HttpGet]
        public IActionResult SaveProjectForm(int projectId)
        {
            var formData = new FormData()
            {
                FormIoUrl = _formIOSettings.ProjectForm,
                FormIoString = @"{""id"":0}",
            
            };

            if (projectId > 0)
            {
                var paramList = new Dictionary<string, string>();
                paramList.Add("projectId", projectId.ToString());
                var project = _clientHelper.CallAPIWithoutModel<BL.Models.Project>("/api/Project/GetProject/", paramList).Result;
                formData.FormIoString = project?.FormData;

                formData.FormIoString = formData.FormIoString?.Replace(@"""id"":0", @"""Id"":" + projectId.ToString(), StringComparison.CurrentCultureIgnoreCase);
            }

            return View(formData);
        }

        [HttpGet]
        public IActionResult GetProjectBuckets(int id)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("projectId", id.ToString());
            var project = _clientHelper.CallAPIWithoutModel<Project?>(
                "/api/Project/GetProject/", paramlist).Result;

            var minioEndpoint = _clientHelper.CallAPIWithoutModel<MinioEndpoint>("/api/Project/GetMinioEndPoint").Result;

            var projectView = new ProjectUserTre()
            {
                Id = project.Id,
                FormData = project.FormData,
                Name = project.Name,
                Users = project.Users,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Tres = project.Tres,
                SubmissionBucket = project.SubmissionBucket,
                OutputBucket = project.OutputBucket,
                MinioEndpoint = minioEndpoint.Url
            };

            return View(projectView);
        }

        [HttpPost]
        public async Task<IActionResult> ProjectFormSubmission([FromBody] object arg, int id)
        {
            var str = arg?.ToString();

            if (!string.IsNullOrEmpty(str))
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<FormData>(str);
                data.FormIoString = str;
       
                var result = await _clientHelper.CallAPI<FormData, Project?>("/api/Project/SaveProject", data);
                var audit = new AuditLog()
                {
                    FormData = data.FormIoString,
                    IPaddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                    UserName = @User?.FindFirst("name")?.Value,
                    Module = "Projects",
                    AuditValues = "Added Project/"+" "+ result.Id.ToString()+" "+result.ErrorMessage,
                    Action = "ProjectFormSubmission",
                    Date = DateTime.Now.ToUniversalTime()
                };
                var log = await _clientHelper.CallAPI<AuditLog, AuditLog?>("/api/Audit/SaveAuditLogs", audit);

                if (result.Id == 0)
                    return BadRequest();

                return Ok(result);
            }
            return BadRequest();
        }

       
        public async Task<IActionResult> RemoveUserFromProject(int projectId, int userId)
        {
            var model = new ProjectUser()
            {
                ProjectId = projectId,
                UserId = userId
            };
            var result =
                await _clientHelper.CallAPI<ProjectUser, ProjectUser?>("/api/Project/RemoveUserMembership", model);

            var audit = new AuditLog()
            {
                FormData = "ProjectId: " + model.ProjectId.ToString() + " /User Id: " + model.UserId.ToString(),
                IPaddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                UserName = @User?.FindFirst("name")?.Value,
                Module = "ProjectUser",
                AuditValues = "Removed Project User- " + "ProjectId: " + model.ProjectId.ToString() + " /User Id: " + model.UserId.ToString(),
                Action = "RemoveUserFromProject",
                Date = DateTime.Now.ToUniversalTime()
            };
            var log = await _clientHelper.CallAPI<AuditLog, AuditLog?>("/api/Audit/SaveAuditLogs", audit);
            
            return RedirectToAction("GetProject", new { id = projectId });
        }

        [HttpGet]
        public async Task<IActionResult> RemoveTreFromProject(int projectId, int treId)
        {
            var model = new ProjectTre()
            {
                ProjectId = projectId,
                TreId = treId
            };
            var result =
                await _clientHelper.CallAPI<ProjectTre, ProjectTre?>("/api/Project/RemoveTreMembership", model);
            var audit = new AuditLog()
            {
                FormData = "ProjectId: " + model.ProjectId.ToString() + " /Tre Id: " + model.TreId.ToString(),
                IPaddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                UserName = @User?.FindFirst("name")?.Value,
                Module = "ProjectTre",
                AuditValues = "Removed Project Tre- " + "ProjectId: " + model.ProjectId.ToString() + " /Tre Id: " + model.TreId.ToString(),
                Action = "RemoveTreFromProject",
                Date = DateTime.Now.ToUniversalTime()
            };
            var log = await _clientHelper.CallAPI<AuditLog, AuditLog?>("/api/Audit/SaveAuditLogs", audit);

            return RedirectToAction("GetProject", new { id = projectId });
        }

        [HttpPost]
        public async Task<IActionResult> AddUserList(string ProjectId, string ItemList)
        {
            string[] arr = ItemList.Split(',');
            foreach (string s in arr)
            {
                var model = new ProjectUser()
                {
                    ProjectId = Int32.Parse(ProjectId),
                    UserId = Int32.Parse(s)
                };
                var result =
                await _clientHelper.CallAPI<ProjectUser, ProjectUser?>("/api/Project/AddUserMembership", model);
            }
            return RedirectToAction("GetProject", new {  id = ProjectId });
        }

        [HttpPost]
        public async Task<IActionResult> AddTreList(string ProjectId, string ItemList)
        {
            string[] arr = ItemList.Split(',');
            foreach (string s in arr)
            {
                var model = new ProjectTre()
                {
                    ProjectId = Int32.Parse(ProjectId),
                    TreId = Int32.Parse(s)
                };
                var result =
                await _clientHelper.CallAPI<ProjectTre, ProjectTre?>("/api/Project/AddTreMembership",
                    model);
            }
            return RedirectToAction("GetProject", new { id = ProjectId });
        }

        [HttpGet]
        [Authorize(Roles = "dare-control-admin")]
        public void IsUserOnProject(int projectId, int userId)
        {
            var model = new ProjectUser()
            {
                ProjectId = projectId,
                UserId = userId
            };
            var result = _clientHelper.CallAPI<ProjectUser, ProjectUser?>("api/Project/IsUserOnProject", model);
        }

    }
}
