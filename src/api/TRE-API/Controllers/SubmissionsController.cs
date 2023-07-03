using BL.Models;
using BL.Repositories.DbContexts;
using TRE_API.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using BL.Models;
using BL.Models.DTO;
using BL.Models.Tes;
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
        [Route("GetWaitingProjectApprovals")]
        [ValidateModelState]
        [SwaggerOperation("GetWaitingProjectApprovals")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<Submission>), description: "")]
        public virtual IActionResult GetWaitingProjectApprovals(string endpointname)
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


        [HttpGet]
        [Route("UpdateStatusForApprovals")]
        [ValidateModelState]
        [SwaggerOperation("UpdateStatusForApprovals")]
        [SwaggerResponse(statusCode: 200, type: typeof(APIReturn), description: "")]
        public IActionResult UpdateStatusForApprovals(string endpointname, string tesId, SubmissionStatus status)
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


            return StatusCode(200, new APIReturn() { ReturnType = ReturnType.voidReturn });
        }
    }
}
