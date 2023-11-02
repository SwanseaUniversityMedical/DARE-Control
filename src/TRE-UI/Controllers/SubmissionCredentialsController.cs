﻿using BL.Models;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BL.Models.APISimpleTypeReturns;
using TRE_UI.Services;

namespace TRE_UI.Controllers
{
    [Authorize(Roles = "dare-tre-admin")]
    public class SubmissionCredentialsController : Controller
    {
        private readonly ITREClientHelper _clientHelper;
        public SubmissionCredentialsController(ITREClientHelper client)
        {
            _clientHelper = client;
        }



        [HttpGet]

        public async Task<IActionResult> UpdateCredentialsAsync()
        {
            return View(await ControllerHelpers.UpdateCredentials("SubmissionCredentials", _clientHelper));
            
        }

        [HttpPost]
        
        public async Task<IActionResult> UpdateCredentials(KeycloakCredentials credentials) {

            var result =
                await ControllerHelpers.UpdateCredentials("SubmissionCredentials", _clientHelper, ModelState,
                    credentials);

            if (result)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View(credentials);
            }
            

        }

       
       

    
     
    }
}
