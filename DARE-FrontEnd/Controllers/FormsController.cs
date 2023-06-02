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

        [Authorize]
        [Route("Forms/Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> FormSubmission([FromBody] JsonObject submissionData)
        {
            var result = _projectsHandler.CreateProject(submissionData);
            //IActionResult result = await HomeController.CreateProject(submissionData);

            return (IActionResult)result;
        }

        [HttpPost]
        public IActionResult UserFormSubmission([FromBody] JsonObject submissionData)
        {
            //save session id against it
            _projectsHandler.AddAUser1(submissionData);
            return View();
        }

        [HttpPost]
        public IActionResult FormSubmissionIndex([FromBody] JsonObject submissionData)
        {
            //save session id against it
            return View();
        }
    }
}
