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

        public DEBUGHasuraController(IHasuraService iHasuraService)
        {
            _iHasuraService = iHasuraService;
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

    }
}
