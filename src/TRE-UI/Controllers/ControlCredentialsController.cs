using BL.Models;

using BL.Services;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using BL.Models.APISimpleTypeReturns;

namespace TRE_UI.Controllers
{
    [Authorize(Roles = "dare-tre-admin")]
    public class ControlCredentialsController : Controller
    {
        private readonly ITREClientHelper _clientHelper;
        public ControlCredentialsController(ITREClientHelper client)
        {
            _clientHelper = client;
        }



        [HttpGet]

        public async Task<IActionResult> UpdateCredentialsAsync()
        {
            var valid = await _clientHelper.CallAPIWithoutModel<BoolReturn>("/api/ControlCredentials/CheckCredentialsAreValid");


            return View(new ControlCredentials()
                { Valid = valid.Result })
                ;
        }

        [HttpPost]
        
        public async Task<IActionResult> UpdateCredentials(ControlCredentials credentials) {

            if (ModelState.IsValid)
            {


                var result =
                    await _clientHelper.CallAPI<ControlCredentials, ControlCredentials>(
                        "/api/ControlCredentials/UpdateCredentials", credentials);
                if (result.Valid)
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return View(credentials);
                }
            }
            else
            {
                return View(credentials);
            }

        }

       
       

    
     
    }
}
