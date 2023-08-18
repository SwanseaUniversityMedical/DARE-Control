
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
    //[Authorize]
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

        [Authorize(Roles = "dare-tre-agent")]
        [HttpPost("DAREUpdateSubmission")]
        public async void DAREUpdateSubmission(string endpointname, string tesId, string submissionStatus) 
        {
            List<string> StringList = new List<string> { endpointname, tesId, submissionStatus };
            await _signalRService.SendUpdateMessage("TREUpdateStatus", StringList);
        }

        [Authorize(Roles = "dare-tre-agent")]
        [HttpGet]
        [Route("GetWaitingSubmissionsForEndpoint")]
        [ValidateModelState]
        [SwaggerOperation("GetWaitingSubmissionsForEndpoint")]
        [SwaggerResponse(statusCode: 200, type: typeof(List<Submission>), description: "")]
        public virtual IActionResult GetWaitingSubmissionsForEndpoint()
        {
            var result =
                _dareHelper.CallAPIWithoutModel<List<Submission>>("/api/Submission/GetWaitingSubmissionsForEndpoint").Result;
            return StatusCode(200, result);
        }


        [HttpGet]
        [Route("UpdateStatusForEndpoint")]
        [ValidateModelState]
        [SwaggerOperation("UpdateStatusForEndpoint")]
        [SwaggerResponse(statusCode: 200, type: typeof(APIReturn), description: "")]
        public IActionResult UpdateStatusForEndpoint(string tesId, StatusType statusType, string description)
        {
            var result = _dareHelper.CallAPIWithoutModel<APIReturn>("/api/Submission/UpdateStatusForEndpoint",
                new Dictionary<string, string>() { { "tesId", tesId }, { "statusType", statusType.ToString() },{"description", description} }).Result;
            return StatusCode(200, result);
        }
    }
}
