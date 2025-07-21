using Microsoft.AspNetCore.Mvc;
using Tre_Camunda.Models;
using Tre_Camunda.Services;

namespace Tre_Camunda.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IZeebeDmnService _zeebeDmnService;

        public WeatherForecastController(IZeebeDmnService zeebeDmnService)
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
