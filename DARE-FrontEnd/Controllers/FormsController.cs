using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using DARE_FrontEnd.Models;
using System.Data;
using System.Text.Json;
using Newtonsoft.Json;
using DARE_FrontEnd.Services.Project;

namespace DARE_FrontEnd.Controllers
{
    [Authorize]
    public class FormsController : Controller
    {
        private readonly IProjectsHandler _projectsHandler;

        public FormsController(IProjectsHandler projectsHandler)
        {
            _projectsHandler = projectsHandler;
        }

        [Route("Forms/Index")]
        public IActionResult Index()
        {
            return View(new data()
            {
                FormIoUrl = "https://bthbspqizezypsb.form.io/dareuser/dareuserregistration"
            });
        }

        public class data
        {
            public string? FormIoString { get; set; }
            public string? FormIoUrl { get; set; }
            

        }


        [HttpPost]
        public async Task<IActionResult> FormSubmission([FromBody] data submissionData)
        {
            var result = await _projectsHandler.CreateProject(submissionData);
            // IActionResult result = await HomeController.CreateProject(submissionData);

            return (IActionResult)result;
        }

        [HttpPost]
        public async Task<IActionResult> UserFormSubmission([FromBody] data submissionData)
        {
            //save session id against it

            var result = await _projectsHandler.AddAUser(submissionData);
            return (IActionResult)result;
        }

        [HttpPost]
        public IActionResult FormSubmissionIndex([FromBody] JsonObject submissionData)
        {
            //save session id against it
            return View();
        }

        [HttpPost]
        public IActionResult GetAllProjects()
        {
           var result = _projectsHandler.GetAllProjects();

            JsonConvert.SerializeObject(result);
            return View(result);
        }

        [HttpPost]
        public IActionResult Membership()
        {
            var result = _projectsHandler.GetAllProjects();
            var result1 = _projectsHandler.GetAllUsers();
    
            JsonConvert.SerializeObject(result);
            JsonConvert.SerializeObject(result1);
            return View(result1);

        }
    }
}
