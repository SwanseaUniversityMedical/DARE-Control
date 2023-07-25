using BL.Models.DTO;
using BL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TRE_API.Services.SignalR;
using BL.Services;
using System.Text.Json;
using System.Text;

namespace TRE_API.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SubmissionController : Controller
    {
        private readonly ISignalRService _signalRService;

        public SubmissionController(ISignalRService signalRService) 
        { 
            _signalRService = signalRService;
        }


        [HttpPost("DAREUpdateSubmission")]
        public async void DAREUpdateSubmission(string endpointname, string tesId, string submissionStatus) 
        {
            await _signalRService.SendUpdateMessage("TREUpdateStatus", endpointname, tesId, submissionStatus);
        }
    }
}
