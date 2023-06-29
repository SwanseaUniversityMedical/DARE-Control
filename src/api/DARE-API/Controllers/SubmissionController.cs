using BL.Models;
using BL.Repositories.DbContexts;
using DARE_API.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using BL.Models;
using EasyNetQ;

namespace DARE_API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// API endpoints for <see cref="Submission"/>s.
    /// </summary>
    public class SubmissionController : Controller
    {
        private readonly ApplicationDbContext _DbContext;

        

        public SubmissionController(ApplicationDbContext repository, IBus rabbit)
        {
            _DbContext = repository;
            

        }


        
        [HttpGet]
        [Route("GetWaitingSubmissionsForEndpoint")]
        [ValidateModelState]
        [SwaggerOperation("GetWaitingSubmissionsForEndpoint")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<Submission>), description: "")]
        public virtual IActionResult GetWaitingSubmissionsForEndpoint(string endpointname)
        {
            //ToDo alter to get endpoint from validated token
            var endpoint = _DbContext.Endpoints.FirstOrDefault(x => x.Name.ToLower() == endpointname.ToLower());
            if (endpoint == null)
            {
                return BadRequest("No access to endpoint " + endpointname + " or does not exist.");
            }

            var results = endpoint.Submissions.Where(x => x.Status == SubmissionStatus.WaitingForAgentToTransfer).ToList();

            return StatusCode(200, results);
        }
    }
}
