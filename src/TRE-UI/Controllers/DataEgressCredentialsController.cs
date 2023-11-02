using BL.Models;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BL.Models.APISimpleTypeReturns;
using TRE_UI.Services;
using System.Net;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TRE_UI.Controllers
{
    [Authorize(Roles = "dare-tre-admin")]
    public class DataEgressCredentialsController : Controller
    {
        private readonly ITREClientHelper _clientHelper;
        public DataEgressCredentialsController(ITREClientHelper client)
        {
            _clientHelper = client;
        }



        [HttpGet]

        public async Task<IActionResult> UpdateCredentialsAsync()
        {
            return View(await ControllerHelpers.UpdateCredentials("DataEgressCredentials", _clientHelper));
        }



        [HttpPost]

        public async Task<IActionResult> UpdateCredentials(KeycloakCredentials credentials)
        {
            if (await ControllerHelpers.UpdateCredentials("DataEgressCredentials", _clientHelper, ModelState,
                    credentials))
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
