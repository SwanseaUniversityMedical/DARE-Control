using Microsoft.AspNetCore.Mvc;
using TRE_API.Models;
using TRE_API.Services;

namespace TRE_API.Controllers
{
    public class CredentialsMinioController : Controller
    {
        private readonly IZeebeDmnService _zeebeDmnService;
        
        public CredentialsMinioController(IZeebeDmnService zeebeDmnService)
        {
            _zeebeDmnService = zeebeDmnService;
        }

        [HttpPost("evaluateDMN")]
        public async Task<IActionResult> EvaluateDecisionModel([FromBody] DmnRequest dmnRequest)
        {
            try
            {
                var result = await _zeebeDmnService.EvaluateDecisionModelAsync(dmnRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }       

    }
}
