
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

namespace TRE_API.Controllers
{
    [Authorize(Roles = "dare-tre-agent")]
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionController : Controller
    {
        private readonly ISignalRService _signalRService;
        private readonly IDareClientWithoutTokenHelper _dareHelper;

        public SubmissionController(ISignalRService signalRService, IDareClientWithoutTokenHelper helper) 
        { 
            _signalRService = signalRService;
            _dareHelper = helper;
        }

       
        [HttpPost("DAREUpdateSubmission")]
        public async void DAREUpdateSubmission(string trename, string tesId, string submissionStatus) 
        {
            List<string> StringList = new List<string> { trename, tesId, submissionStatus };
            await _signalRService.SendUpdateMessage("TREUpdateStatus", StringList);
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
