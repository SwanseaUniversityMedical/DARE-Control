﻿using BL.Models;
using BL.Models.DTO;
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
        public IActionResult AddEndpointForm()
        {
            return View(new FormData()
            {
                FormIoUrl = "https://psttpefwlitcuek.form.io/endpoint"
            });

        }

        [HttpGet]
        public IActionResult AddEndpoint()
        {
            return View();

        }

        [HttpPost]
        public IActionResult AddEndpoint(Endpoint model)
        {
            var result =  _clientHelper.CallAPI<Endpoint, Endpoint?>("/api/Endpoint/AddEndpointMVC", model).Result;

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

            return Ok(result);
        }
    }
}
