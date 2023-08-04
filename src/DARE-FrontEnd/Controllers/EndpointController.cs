using BL.Models;
using BL.Models.DTO;
using BL.Services;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Endpoint = BL.Models.Endpoint;
using BL.Models.Settings;

namespace DARE_FrontEnd.Controllers
{
    [Authorize(Roles = "dare-control-admin")]
    public class EndpointController : Controller
    {

        private readonly IDareClientHelper _clientHelper;
        private readonly IConfiguration _configuration;
        private readonly IFormIOSettings _formIOSettings;

        public EndpointController(IDareClientHelper client, IConfiguration configuration)
        {
            _clientHelper = client;
            _configuration = configuration;
            _formIOSettings = new FormIOSettings();
            configuration.Bind(nameof(FormIOSettings), _formIOSettings);
        }

        [HttpGet]
        public IActionResult AddEndpoint()
        {
            return View(new FormData()
            {
                FormIoUrl =  _formIOSettings.EndpointForm
            });
        }

        [HttpPost]
        public IActionResult AddEndpoint(Endpoint model)
        {
            var data = new FormData()
            {
                FormIoUrl = _formIOSettings.EndpointForm,
                FormIoString = JsonConvert.SerializeObject(model)
            };
            var result =  _clientHelper.CallAPI<FormData, Endpoint?>("/api/Endpoint/AddEndpointMVC", data).Result;

            return RedirectToAction("GetAllEndpoints");

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
     
        [HttpPost]
        public async Task<IActionResult> EndpointFormSubmission([FromBody] FormData submissionData)
        {
            var result = await _clientHelper.CallAPI<FormData, Endpoint>("/api/Endpoint/AddEndpoint", submissionData);
            if (result.Id == 0)
            {
                return BadRequest();

            }
            return Ok(result);
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

    }
}
