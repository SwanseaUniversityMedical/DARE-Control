using BL.Models;
using BL.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;
using System.Data;
using TREAPI.Services;

namespace TRE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DEBUGHasuraController : Controller
    {

        private readonly IHasuraService _iHasuraService;

        private readonly IDoSyncWork _iDoSyncWork;
        private readonly IDoAgentWork _iDoAgentWork;


        public DEBUGHasuraController(IHasuraService iHasuraService, IDoSyncWork iDoSyncWork,
           IDoAgentWork iDoAgentWork)
        {
            _iHasuraService = iHasuraService;
            _iDoSyncWork = iDoSyncWork;
            _iDoAgentWork = iDoAgentWork;
        }


        [HttpPost]
        [Route("RunHasuraSetup")]
        public async Task<IActionResult> RunHasuraSetup()
        {
            Log.Information("RunHasuraSetup");
            try
            {
                await _iHasuraService.Run();
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "RunHasuraSetup");
                throw;
            }
        }
        [HttpPost]
        [Route("DoSyncWork")]
        public async Task<IActionResult> DoSyncWork()
        {
            Log.Information("DoSyncWork");
            try
            {
                _iDoSyncWork.Execute();
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "DoSyncWork");
                throw;
            }
        }

        [HttpPost]
        [Route("DoAgentWork")]
        public async Task<IActionResult> DoAgentWork()
        {

            Log.Information("DoAgentWork");
            try
            {
                _iDoAgentWork.Execute();
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "DoAgentWork");
                throw;
            }
        }


        public class Cooldat{
            public string role { get; set; }
            public string Torun { get; set; }
        }

        [HttpPost]
        [Route("RunThniny")]
        public async Task<IActionResult> RunThniny(Cooldat cooldat)
        {

            Log.Information("RunThniny");
            try
            {
                await _iDoAgentWork.testing(cooldat.Torun, cooldat.role);
                return StatusCode(200);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "RunThniny");
                throw;
            }
        }

    }
}
