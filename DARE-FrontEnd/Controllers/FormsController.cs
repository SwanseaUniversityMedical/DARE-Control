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
    public class FormsController : Controller
    {
        [Authorize]
        [Route("Forms/Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult FormSubmission([FromBody] JsonObject submissionData)
        {
            //save session id against it

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
