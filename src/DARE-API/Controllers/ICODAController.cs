using BL.Models.Tes;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Amazon.Runtime;
using IdentityModel.Client;
using Serilog;

namespace DARE_API.Controllers
{
    public class ICODAController : Controller
    {
        private readonly IICODAClientHelper _clientHelper;
        protected readonly IHttpClientFactory _httpClientFactory;
        private readonly string ICODAAddress = "https://localhost:5005";
        private static readonly HttpClient client = new HttpClient();

        public ICODAController(IICODAClientHelper clientHelper, IHttpClientFactory httpClientFactory)
        {
            _clientHelper = clientHelper;
            _httpClientFactory = httpClientFactory;
        }


        [HttpGet("/tasks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ListJobs(string token, [FromQuery] string view = "MINIMAL", [FromQuery] int page_size = 256, [FromQuery] int page_number = 1)
        {
            var res = HTTPGet<List<TesTask>>(ICODAAddress + $"/tasks?view={view}&page_size={page_size}&page_number={page_number}", token);
            return Ok(new { res });
        }

        [HttpGet("/tasks/{task_id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GeTESTask([FromRoute] string task_id, string token, [FromQuery] string view = "MINIMAL")
        {
            try 
            {
                //var param = new Dictionary<string, string>();
                //param.Add("view", view);
                //var content = new FormUrlEncodedContent(param);

                var res = HTTPGet<TesTask>(ICODAAddress + $"/tasks/{task_id}?view={view}", token);

                //var res = await apiClient.GetAsync($"https://localhost:5005/tasks/{task_id}?view={view}");
                //var res = await client.GetStringAsync($"https://localhost:5005/Job/tasks/{task_id}?view={view}");

                //var res = await _clientHelper.CallAPIWithoutModel<TesTask>($"/tasks/{task_id}", param);
                if (res != null)
                {
                    return Ok(new { res });
                }
            }
            catch (Exception e)
            {
                return BadRequest("Failed to get the specific task");
            }

            return BadRequest("Failed to get the specific task");
        }

        [HttpPost("/tasks")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "ProjectTaskMembershipRequired")]
        public async Task<IActionResult> SubmitTES(TesTask taskVm)
        {

            return Ok();
        }

        [HttpPost("/tasks/validate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidateTES(TesTask taskVm)
        {
            // validate task
            var validResult = ValidateTask(taskVm);
            if (validResult != null) return validResult;
            return Ok();
        }

        [HttpGet("/tasks/service-info")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetServiceInfo()
        {
            return Ok("TODO: Implement Get Service Info");
        }

        [HttpGet("/tasks/{task_id}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelTask(string task_id)
        {
            try
            {
                return NotFound();
            }
            catch (Exception e)
            {
                return BadRequest($"Failed to stop task {task_id}");
            }
        }

        [AllowAnonymous]
        [HttpGet("/internal/has_task_results/{task_id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<bool> HasTaskResult([FromRoute] string task_id)
        {
            try
            {
                return false;
            }
            catch (Exception e)
            {
              
                return false;
            }
        }

        private IActionResult ValidateTask(TesTask taskVm)
        {
            if (string.IsNullOrEmpty(taskVm.Name)) return BadRequest("Job name is mandatory.");

            if (taskVm.Inputs.Count != 1) return BadRequest("Task is allowed to contain only ONE input.");
            if (string.IsNullOrEmpty(taskVm.Inputs.First().Content)) return BadRequest("content has to be present in the input setting");

            if (taskVm.Executors.Count != 1) return BadRequest("Task is allowed to contain only ONE executor.");
            if (string.IsNullOrEmpty(taskVm.Executors.First().Image)) return BadRequest("image has to be present in the executor setting");

            return null;
        }

        private async Task<T> HTTPGet<T>(string url, string token)
        {
            var client = await CreateClient(token);

            HttpResponseMessage res = new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.BadRequest
            };

            res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                throw new Exception("API Call Failure: " + res.StatusCode + ": " + res.ReasonPhrase);
            }
            Log.Information("{Function} The response {res}", "ClientHelperRequestAsync", res);

            var result = res.Content;

            var data = await result.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(data) ?? throw new InvalidOperationException();
        }

        private async Task<HttpClient> CreateClient(string token)
        {
            var apiClient = _httpClientFactory.CreateClient();
            apiClient.SetBearerToken(token);
            apiClient.DefaultRequestHeaders.Add("Accept", "application/json");
            return apiClient;
        }
    }
}
