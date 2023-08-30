using BL.Models;
using BL.Models.ViewModels;
using BL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BL.Models.Settings;
using Microsoft.CodeAnalysis;

namespace DARE_FrontEnd.Controllers
{
    [Authorize(Roles = "dare-control-admin")]
    public class EndpointController : Controller
    {

        private readonly IDareClientHelper _clientHelper;
        
        private readonly FormIOSettings _formIOSettings;

        public EndpointController(IDareClientHelper client, FormIOSettings formIo)
        {
            _clientHelper = client;

            _formIOSettings = formIo;
            
        }

        
        public IActionResult AddEndpoint(int endpointId)
        {
            var formData = new FormData()
            {
                
                FormIoUrl = _formIOSettings.EndpointForm,
                FormIoString = @"{""id"":0}"
            };

            if (endpointId > 0)
            {
                var paramList = new Dictionary<string, string>();
                paramList.Add("endpointId", endpointId.ToString());
                var endpoint = _clientHelper.CallAPIWithoutModel<BL.Models.Tre>("/api/Endpoint/GetanEndpoint/", paramList).Result;
                //TempData["end"] = endpoint.ErrorMessage.ToString();
                formData.FormIoString = endpoint?.FormData;
                formData.FormIoString = formData.FormIoString?.Replace(@"""id"":0", @"""Id"":" + endpointId.ToString(), StringComparison.CurrentCultureIgnoreCase);
            }

            return View(formData);
        }

     

        [HttpGet]
        public IActionResult GetAllEndpoints()
        {

            var endpoints = _clientHelper.CallAPIWithoutModel<List<Tre>>("/api/Endpoint/GetAllEndpoints/").Result;

            return View(endpoints);
        }

     

        [HttpGet]
        public IActionResult GetAnEndpoint(int id)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("endpointId", id.ToString());
            var test = _clientHelper.CallAPIWithoutModel<Tre?>(
                "/api/Endpoint/GetAnEndpoint/", paramlist).Result;

            return View(test);
        }
 
        [HttpPost]
        public async Task<IActionResult> EndpointFormSubmission([FromBody] object arg, int id)
        {
            var str = arg?.ToString();

            if (!string.IsNullOrEmpty(str))
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<FormData>(str);
                data.FormIoString = str;

                var result = await _clientHelper.CallAPI<FormData, Tre?>("/api/Endpoint/AddEndpoint", data);

                if (result.Error)
                    return BadRequest();

                return Ok(result);
            }
            return BadRequest();
        }



   

    }
}
