using BL.Models;
using BL.Models.DTO;
using BL.Services;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Endpoint = BL.Models.Endpoint;

namespace DARE_FrontEnd.Controllers
{
    public class EndpointController : Controller
    {

        private readonly IDareClientHelper _clientHelper;
        public EndpointController(IDareClientHelper client)
        {
            _clientHelper = client;
        }

        [HttpGet]
        public IActionResult AddEndpointForm()
        {
            return View(new FormData()
            {
                FormIoUrl = "https://formio.ukserp.ac.uk/dev-sumcldchbogedhw/addendpoint"
            });

        }

        [HttpGet]
        public IActionResult AddEndpoint()
        {
            return View(new FormData()
            {
                FormIoUrl = "https://formio.ukserp.ac.uk/dev-sumcldchbogedhw/addendpoint"
            });
        }

        [HttpPost]
        public IActionResult AddEndpoint(Endpoint model)
        {
            var data = new FormData()
            {
                FormIoUrl = "https://psttpefwlitcuek.form.io/endpoint",
                FormIoString = JsonConvert.SerializeObject(model)
            };
            var result =  _clientHelper.CallAPI<FormData, Endpoint?>("/api/Endpoint/AddEndpointMVC", data).Result;

            return View(result);

        }


        [HttpGet]
        public IActionResult GetAllEndpoints()
        {

            var test = _clientHelper.CallAPIWithoutModel<List<Endpoint>>("/api/Endpoint/GetAllEndpoints/").Result;

            return View(test);
        }




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
    }
}
