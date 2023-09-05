
using BL.Models.ViewModels;
using BL.Models;
using BL.Models.Enums;
using BL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using TRE_API.Attributes;
using TRE_API.Services;
using TRE_API.Services.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using BL.Models.APISimpleTypeReturns;
using TRE_API.Repositories.DbContexts;

namespace TRE_API.Controllers
{
    [Authorize(Roles = "dare-tre-agent")]
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionController : Controller
    {
        private readonly ISignalRService _signalRService;
        private readonly IDareClientWithoutTokenHelper _dareHelper;
        private readonly ApplicationDbContext _dbContext;

        public SubmissionController(ISignalRService signalRService, IDareClientWithoutTokenHelper helper, ApplicationDbContext dbContext) 
        { 
            _signalRService = signalRService;
            _dareHelper = helper;
            _dbContext = dbContext;
            
        }

       
        [HttpPost("DAREUpdateSubmission")]
        public async void DAREUpdateSubmission(string trename, string tesId, string submissionStatus) 
        {
            List<string> StringList = new List<string> { trename, tesId, submissionStatus };
            await _signalRService.SendUpdateMessage("TREUpdateStatus", StringList);
        }


        
        [HttpGet("IsUserApprovedOnProject")]
        public BoolReturn IsUserApprovedOnProject(int projectId, int userId)
        {
            
            return new BoolReturn()
            {
                Result = _dbContext.MembershipDecisions.FirstOrDefault(x =>
                    x.Project != null && x.Project.SubmissionProjectId == projectId && x.User != null &&
                    x.User.SubmissionUserId == userId &&
                    !x.Project.Archived && x.Project.Decision == Decision.Approved && !x.Archived &&
                    x.Decision == Decision.Approved) != null
            };
        }

        [HttpGet]
        [Route("GetWaitingSubmissionsForTre")]
        [ValidateModelState]
        [SwaggerOperation("GetWaitingSubmissionsForTre")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<Submission>), description: "")]
        public virtual IActionResult GetWaitingSubmissionsForTre()
        {
            var result =
                _dareHelper.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetWaitingSubmissionsForTre").Result;
            return StatusCode(200, result);
        }


        
        [HttpGet]
        [Route("UpdateStatusForTre")]
        [ValidateModelState]
        [SwaggerOperation("UpdateStatusForTre")]
        [SwaggerResponse(statusCode: 200, type: typeof(APIReturn), description: "")]
        public IActionResult UpdateStatusForTre(string tesId, StatusType statusType, string? description)
        {
            var result = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForTre",
                new Dictionary<string, string>() { { "tesId", tesId }, { "statusType", statusType.ToString() },{"description", description} }).Result;
            return StatusCode(200, result);
        }
    }
}
