using BL.Models;
using DARE_FrontEnd.Services;
using Microsoft.AspNetCore.Mvc;
using Endpoint = BL.Models.Endpoint;

namespace DARE_FrontEnd.Controllers
{
    public class EndpointController : Controller
    {

        private readonly IClientHelper _clientHelper;
        public EndpointController(IClientHelper client)
        {
            _clientHelper = client;
        }

        [HttpGet]
        public IActionResult AddEndpoint()
        {
            return View(new FormData()
            {
                FormIoUrl = "https://psttpefwlitcuek.form.io/endpoint"
            });

        }
        






        [HttpPost]
        public async Task<IActionResult> EndpointFormSubmission([FromBody] FormData submissionData)
        {
            var result = await _clientHelper.CallAPI<FormData, Endpoint?>("/api/Endpoint/AddEndpoint", submissionData);

            return Ok(result);
        }
    }
}
