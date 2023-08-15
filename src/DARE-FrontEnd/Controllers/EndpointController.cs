using BL.Models.ViewModels;
using BL.Services;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using Endpoint = BL.Models.Endpoint;
using BL.Models.Settings;

namespace DARE_FrontEnd.Controllers
{
    //[Authorize(Roles = "dare-control-admin,dare-tre-admin")]
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

        [HttpGet]
        public IActionResult AddEndpoint()
        {
            return View(new FormData()
            {
                FormIoUrl = _formIOSettings.EndpointForm,
                FormIoString = @"{""id"":0}"

            }); ;
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
        public async Task<IActionResult> EndpointFormSubmission([FromBody] object arg, int id)
        {
            var str = arg?.ToString();

            if (!string.IsNullOrEmpty(str))
            {
                var theEndpoint = System.Text.Json.JsonSerializer.Deserialize<FormData>(str);
                theEndpoint.FormIoString = str;

                var result = await _clientHelper.CallAPI<FormData, Endpoint>("/api/Endpoint/AddEndpoint", theEndpoint);
                if (result.ErrorMessage!= null)
                {
                    
                    return BadRequest(result.ErrorMessage);
                   
                } 
                return Ok(result);
            }
            return BadRequest();
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
