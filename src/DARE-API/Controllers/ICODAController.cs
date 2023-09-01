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
using System.Text.Json;
using System.Text;
using System.Reflection.Metadata.Ecma335;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
            try
            {
                var res = await _clientHelper.CallAPIWithoutModelWithAccessCode<List<TesTask>>($"/tasks?view={view}&page_size={page_size}&page_number={page_number}", token, null);
                //var res = HTTPGet<List<TesTask>>(ICODAAddress + $"/tasks?view={view}&page_size={page_size}&page_number={page_number}", token);
                return Ok(new { res });
            }
            catch (Exception e)
            {
                return BadRequest("Failed to get the specific task");
            }

        }

        [HttpGet("/tasks/{task_id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GeTESTask([FromRoute] string task_id, string token, [FromQuery] string view = "MINIMAL")
        {
            try 
            {
                var param = new Dictionary<string, string>();
                param.Add("view", view);
                var content = new FormUrlEncodedContent(param);

                //var res = HTTPGet<TesTask>(ICODAAddress + $"/tasks/{task_id}?view={view}", token);

                //var res = await apiClient.GetAsync($"https://localhost:5005/tasks/{task_id}?view={view}");
                //var res = await client.GetStringAsync($"https://localhost:5005/Job/tasks/{task_id}?view={view}");

                var res = await _clientHelper.CallAPIWithoutModelWithAccessCode<TesTask>($"/tasks/{task_id}", token, param);
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
        public async Task<IActionResult> SubmitTES(TesTask taskVm, string token)
        {
            try
            {
                var res = await _clientHelper.CallAPIWithAccessCode<TesTask, TesTask>($"/tasks", token, taskVm, null);
                if (res != null)
                { 
                    return Ok(new { id = res.Id });
                }
            } 
            catch (Exception e) 
            {
                return BadRequest("Failed to get the specific task");
            }
            return BadRequest("Failed to get the specific task");
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
        public async Task<IActionResult> CancelTask(string task_id, string token)
        {
            try
            {
                var res = await _clientHelper.ClientHelperRequestAsyncICODA($"https://localhost:5005/Job/tasks/tasks/{task_id}/cancel", token, HttpMethod.Get, null, null);
                var data = await res.Content.ReadAsStringAsync();
                //string res = _clientHelper.GenericHTTPRequest($"/tasks/{task_id}/cancel", token).ToString();
                return Ok(data);
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
    }
}
