using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml;
using System;
using System.Collections.Generic;
using DARE_API.Attributes;
using Microsoft.AspNetCore.WebUtilities;
using Swashbuckle.AspNetCore.Annotations;
using BL.Models;

using DARE_API.ContractResolvers;
using Serilog;


namespace DARE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// API endpoints for <see cref="TesTask"/>s.
    /// </summary>







    public class TaskServiceApiController : ControllerBase
    {





        private static readonly Dictionary<TesView, JsonSerializerSettings> TesJsonSerializerSettings = new()
        {
            {
                TesView.MINIMAL,
                new JsonSerializerSettings { ContractResolver = MinimalTesTaskContractResolver.Instance }
            },
            { TesView.BASIC, new JsonSerializerSettings { ContractResolver = BasicTesTaskContractResolver.Instance } },
            { TesView.FULL, new JsonSerializerSettings { ContractResolver = FullTesTaskContractResolver.Instance } }
        };

        /// <summary>
        /// Contruct a <see cref="TaskServiceApiController"/>
        /// </summary>

        public TaskServiceApiController()
        {
            ;

        }

        /// <summary>
        /// Cancel a task
        /// </summary>
        /// <param name="id">The id of the <see cref="TesTask"/> to cancel</param>
        /// <param name="cancellationToken">A<see cref="CancellationToken"/> for controlling the lifetime of the asynchronous operation.</param>
        /// <response code="200"></response>
        [HttpPost]
        [Route("/v1/tasks/{id}:cancel")]
        [ValidateModelState]
        [SwaggerOperation("CancelTask")]
        [SwaggerResponse(statusCode: 200, type: typeof(object), description: "")]
        public virtual async Task<IActionResult> CancelTask([FromRoute] [Required] string id,
            CancellationToken cancellationToken)
        {
            TesTask tesTask = null;




            return StatusCode(200, new object());
        }

        /// <summary>
        /// Create a new task                               
        /// </summary>
        /// <param name="tesTask">The <see cref="TesTask"/> to add to the repository</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> for controlling the lifetime of the asynchronous operation.</param>
        /// <response code="200"></response>
        [HttpPost]
        [Route("/v1/tasks")]
        [ValidateModelState]
        [SwaggerOperation("CreateTask")]
        [SwaggerResponse(statusCode: 200, type: typeof(TesCreateTaskResponse), description: "")]
        public virtual async Task<IActionResult> CreateTaskAsync([FromBody] TesTask tesTask,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(tesTask.Id))
            {
                return BadRequest(
                    "Id should not be included by the client in the request; the server is responsible for generating a unique Id.");
            }

            if (string.IsNullOrWhiteSpace(tesTask.Executors?.FirstOrDefault()?.Image))
            {
                return BadRequest("Docker container image name is required.");
            }

            foreach (var input in tesTask.Inputs ?? Enumerable.Empty<TesInput>())
            {
                if (!input.Path.StartsWith('/'))
                {
                    return BadRequest("Input paths in the container must be absolute paths.");
                }
            }

            foreach (var output in tesTask.Outputs ?? Enumerable.Empty<TesOutput>())
            {
                if (!output.Path.StartsWith('/'))
                {
                    return BadRequest("Output paths in the container must be absolute paths.");
                }
            }



            if (tesTask?.Resources?.BackendParameters is not null)
            {
                var keys = tesTask.Resources.BackendParameters.Keys.Select(k => k).ToList();

                if (keys.Count > 1 && keys.Select(k => k?.ToLowerInvariant()).Distinct().Count() != keys.Count)
                {
                    return BadRequest("Duplicate backend_parameters were specified");
                }



                keys = tesTask.Resources.BackendParameters.Keys.Select(k => k).ToList();

                // Backends shall log system warnings if a key is passed that is unsupported.
                var unsupportedKeys = keys.Except(Enum.GetNames(typeof(TesResources.SupportedBackendParameters)))
                    .ToList();

                if (unsupportedKeys.Count > 0)
                {
                    Log.Warning("{Function }Unsupported keys were passed to TesResources.backend_parameters: {Keys}",
                        "CreateTaskAsync", string.Join(",", unsupportedKeys));
                }

                // If backend_parameters_strict equals true, backends should fail the task if any key / values are unsupported
                if (tesTask.Resources?.BackendParametersStrict == true
                    && unsupportedKeys.Count > 0)
                {
                    return BadRequest(
                        $"backend_parameters_strict is set to true and unsupported backend_parameters were specified: {string.Join(",", unsupportedKeys)}");
                }


            }

            Log.Debug("{Function} Creating task with id {Id} state {State}", "CreateTaskAsync", tesTask.Id,
                tesTask.State);

            return StatusCode(200, new TesCreateTaskResponse { Id = tesTask.Id });
        }

        /// <summary>
        /// GetServiceInfo provides information about the service, such as storage details, resource availability, and  other documentation.
        /// </summary>
        /// <response code="200"></response>
        [HttpGet]
        [Route("/v1/service-info")]
        [ValidateModelState]
        [SwaggerOperation("GetServiceInfo")]
        [SwaggerResponse(statusCode: 200, type: typeof(TesServiceInfo), description: "")]
        public virtual IActionResult GetServiceInfo()
        {
            var serviceInfo = new TesServiceInfo
            {
                Name = "GA4GH Task Execution Service",
                Doc = string.Empty,
                Storage = new List<string>(),
                TesResourcesSupportedBackendParameters =
                    Enum.GetNames(typeof(TesResources.SupportedBackendParameters)).ToList()
            };

            Log.Information(
                "{Function} Name: {Name} Doc: {Doc} Storage: {Storage} TesResourcesSupportedBackendParameters: {Params}",
                "GetServiceInfo", serviceInfo.Name, serviceInfo.Doc, serviceInfo.Storage,
                string.Join(",", serviceInfo.TesResourcesSupportedBackendParameters));
            return StatusCode(200, serviceInfo);
        }

        /// <summary>
        /// Get a task. TaskView is requested as such: \&quot;v1/tasks/{id}?view&#x3D;FULL\&quot;
        /// </summary>
        /// <param name="id">The id of the <see cref="TesTask"/> to get</param>
        /// <param name="view">OPTIONAL. Affects the fields included in the returned Task messages. See TaskView below.   - MINIMAL: Task message will include ONLY the fields:   Task.Id   Task.State  - BASIC: Task message will include all fields EXCEPT:   Task.ExecutorLog.stdout   Task.ExecutorLog.stderr   Input.content   TaskLog.system_logs  - FULL: Task message includes all fields.</param>
        /// <param name="cancellationToken">A<see cref="CancellationToken"/> for controlling the lifetime of the asynchronous operation.</param>
        /// <response code="200"></response>
        [HttpGet]
        [Route("/v1/tasks/{id}")]
        [ValidateModelState]
        [SwaggerOperation("GetTask")]
        [SwaggerResponse(statusCode: 200, type: typeof(TesTask), description: "")]
        public virtual async Task<IActionResult> GetTaskAsync([FromRoute] [Required] string id, [FromQuery] string view,
            CancellationToken cancellationToken)
        {
            TesTask tesTask = null;


            return TesJsonResult(tesTask, view);
        }

        /// <summary>
        /// List tasks. TaskView is requested as such: \&quot;v1/tasks?view&#x3D;BASIC\&quot;
        /// </summary>
        /// <param name="namePrefix">OPTIONAL. Filter the list to include tasks where the name matches this prefix. If unspecified, no task name filtering is done.</param>
        /// <param name="pageSize">OPTIONAL. Number of tasks to return in one page. Must be less than 2048. Defaults to 256.</param>
        /// <param name="pageToken">OPTIONAL. Page token is used to retrieve the next page of results. If unspecified, returns the first page of results. See ListTasksResponse.next_page_token</param>
        /// <param name="view">OPTIONAL. Affects the fields included in the returned Task messages. See TaskView below.   - MINIMAL: Task message will include ONLY the fields:   Task.Id   Task.State  - BASIC: Task message will include all fields EXCEPT:   Task.ExecutorLog.stdout   Task.ExecutorLog.stderr   Input.content   TaskLog.system_logs  - FULL: Task message includes all fields.</param>
        /// <param name="cancellationToken">A<see cref="CancellationToken"/> for controlling the lifetime of the asynchronous operation.</param>
        /// <response code="200"></response>
        [HttpGet]
        [Route("/v1/tasks")]
        [ValidateModelState]
        [SwaggerOperation("ListTasks")]
        [SwaggerResponse(statusCode: 200, type: typeof(TesListTasksResponse), description: "")]
        public virtual async Task<IActionResult> ListTasks([FromQuery] string namePrefix, [FromQuery] long? pageSize,
            [FromQuery] string pageToken, [FromQuery] string view, CancellationToken cancellationToken)
        {
            var decodedPageToken =
                pageToken is not null ? Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(pageToken)) : null;

            if (pageSize < 1 || pageSize > 2047)
            {
                Log.Error("{Function} pageSize invalid {pageSize}", "ListTasks", pageSize);
                return BadRequest("If provided, pageSize must be greater than 0 and less than 2048. Defaults to 256.");
            }

            List<TesTask> tasks = null;

            var encodedNextPageToken = "";
            var response = new TesListTasksResponse { Tasks = tasks.ToList(), NextPageToken = encodedNextPageToken };

            return TesJsonResult(response, view);
        }



        private IActionResult TesJsonResult(object value, string view)
        {
            TesView viewEnum;

            try
            {
                viewEnum = string.IsNullOrEmpty(view) ? TesView.MINIMAL : Enum.Parse<TesView>(view, true);
            }
            catch
            {
                Log.Error("{Function }Invalid view parameter value. If provided, it must be one of: {Names}",
                    "TesJsonResult", string.Join(", ", Enum.GetNames(typeof(TesView))));
                return BadRequest(
                    $"Invalid view parameter value. If provided, it must be one of: {string.Join(", ", Enum.GetNames(typeof(TesView)))}");
            }

            var jsonResult = new JsonResult(value, TesJsonSerializerSettings[viewEnum]) { StatusCode = 200 };

            return jsonResult;
        }

        private enum TesView
        {
            MINIMAL,
            BASIC,
            FULL
        }
    }
}


