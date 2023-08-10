using BL.Models;
using DARE_API.Repositories.DbContexts;
using DARE_API.Attributes;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using BL.Models.ViewModels;
using EasyNetQ;
using Microsoft.AspNetCore.Authorization;


namespace DARE_API.Controllers
{

    [Route("api/[controller]")]
    [Authorize(Roles = "dare-control-admin")]
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

            var results = endpoint.Submissions.Where(x =>x.Status == SubmissionStatus.WaitingForAgentToTransfer).ToList();
            //var results = _DbContext.Submissions.Where(x => x.EndPoint != null && x.EndPoint.Name == endpointname.ToLower() && x.Status == SubmissionStatus.WaitingForAgentToTransfer).ToList();

            return StatusCode(200, results);
        }


        [HttpGet]
        [Route("UpdateStatusForEndpoint")]
        [ValidateModelState]
        [SwaggerOperation("UpdateStatusForEndpoint")]
        [SwaggerResponse(statusCode: 200, type: typeof(APIReturn), description: "")]
        public  IActionResult UpdateStatusForEndpoint(string endpointname, string tesId, SubmissionStatus status)
        {
            //ToDo alter to get endpoint from validated token
            var endpoint = _DbContext.Endpoints.FirstOrDefault(x => x.Name.ToLower() == endpointname.ToLower());
            if (endpoint == null)
            {
                return BadRequest("No access to endpoint " + endpointname + " or does not exist.");
            }

            var sub = _DbContext.Submissions.FirstOrDefault(x => x.TesId == tesId && x.EndPoint == endpoint);
            if (sub == null)
            {
                return BadRequest("Invalid tesid or endpoint not valid for tes");
            }
            sub.Status = status;
            
            _DbContext.SaveChanges();
            

            return StatusCode(200, new APIReturn(){ReturnType = ReturnType.voidReturn});
        }

        [AllowAnonymous]
        [HttpGet("GetAllSubmissions")]
        public List<Submission> GetAllSubmissions()
        {
            try
            {
                var allSubmissions = _DbContext.Submissions.ToList();

                Log.Information("{Function} Endpoints retrieved successfully", "GetAllSubmissions");
                return allSubmissions;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllSubmissions");
                throw;
            }
        }

        [AllowAnonymous]
        [HttpGet("GetASubmission")]
        public Submission GetASubmission(int id)
        {
            try
            {
                var Submission = _DbContext.Submissions.Where(x => x.Id == id).FirstOrDefault();

                Log.Information("{Function} Submission retrieved successfully", "GetASubmission");
                return Submission;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetASubmission");
                throw;
            }
        }
    }
}
