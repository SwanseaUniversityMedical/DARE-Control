using BL.Models.ViewModels;
using BL.Services;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using Endpoint = BL.Models.Endpoint;
using BL.Models.Settings;
using Microsoft.CodeAnalysis;

namespace DARE_FrontEnd.Controllers
{
    [Authorize(Roles = "dare-control-admin")]
    public class EndpointController : Controller
    {

        private readonly IDareClientHelper _clientHelper;
        
        private readonly IFormIOSettings _formIOSettings;

        public EndpointController(IDareClientHelper client, IConfiguration configuration)
        {
            _clientHelper = client;
            
            _formIOSettings = new FormIOSettings();
            configuration.Bind(nameof(FormIOSettings), _formIOSettings);
        }

        
        public IActionResult AddEndpoint()
        {
            var formData = new FormData()
            {
                FormIoUrl = _formIOSettings.EndpointForm,
                FormIoString = @"{""id"":0}"
            };

            return View(formData);
        }

        [HttpGet]
        public IActionResult EditEndpoint(int endpointId)
        {
            var formData = new FormData()
            {
                FormIoUrl = _formIOSettings.ProjectForm,
                FormIoString = @"{""id"":0}"
            };

            if (endpointId > 0)
            {
                var paramList = new Dictionary<string, string>();
                paramList.Add("endpointId", endpointId.ToString());
                var endpoint = _clientHelper.CallAPIWithoutModel<BL.Models.Endpoint>("/api/Endpoint/GetEndpoints/", paramList).Result;
                formData.FormIoString = endpoint?.FormData;
                formData.FormIoString = formData.FormIoString?.Replace(@"""id"":0", @"""id"":" + endpointId.ToString());
            }

            return View(formData);
        }

        [HttpPost]
        public async Task<IActionResult> EditEndpoint([FromBody] object arg, int id)
        {
            var str = arg?.ToString();

            if (!string.IsNullOrEmpty(str))
            {
                var theEndpoint = System.Text.Json.JsonSerializer.Deserialize<FormData>(str);
                theEndpoint.FormIoString = str;

                var result = await _clientHelper.CallAPI<FormData, Endpoint>("/api/Endpoint/AddEndpoint", theEndpoint);
                if (result.ErrorMessage != null)
                {

                    return BadRequest(result.ErrorMessage);

                }
                return Ok(result);
            }
            return BadRequest();
        }

        [HttpGet]
        public IActionResult GetAllEndpoints()
        {

            var test = _clientHelper.CallAPIWithoutModel<List<Endpoint>>("/api/Endpoint/GetAllEndpoints/").Result;

            return View(test);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetEndpoints(int projectId)
        {
            var endpointsList = _clientHelper.CallAPIWithoutModel<List<Endpoint>>("/api/Endpoint/GetEndPointsInProject/{projectId}").Result;
            return View(endpointsList);
        }


        [HttpGet]
        public IActionResult GetAnEndpoint(int id)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("endpointId", id.ToString());
            var test = _clientHelper.CallAPIWithoutModel<Endpoint?>(
                "/api/Endpoint/GetAnEndpoint/", paramlist).Result;

            return View(test);
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> EndpointFormSubmission([FromBody] FormData submissionData)
        {
            var result = await _clientHelper.CallAPI<FormData, Endpoint?>("/api/Endpoint/AddEndpoint", submissionData);
            if (result.Id == 0)
            {
                return BadRequest();

            }
            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> EditEndpointFormSubmission([FromBody] object arg, int id)
        {
            var str = arg?.ToString();

            if (!string.IsNullOrEmpty(str))
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<FormData>(str);
                data.FormIoString = str;

                var result = await _clientHelper.CallAPI<FormData, Endpoint?>("/api/Endpoint/AddEndpoint", data);

                if (result.Id == 0)
                    return BadRequest();

                return Ok(result);
            }
            return BadRequest();
        }

    }
}
