using BL.Models;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BL.Models.APISimpleTypeReturns;

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
            var valid = await _clientHelper.CallAPIWithoutModel<BoolReturn>("/api/KeycloakCredentials/CheckCredentialsAreValid");


            return View(new KeycloakCredentials()
                { Valid = valid.Result })
                ;
        }

        [HttpPost]
        
        public async Task<IActionResult> UpdateCredentials(KeycloakCredentials credentials) {

            if (ModelState.IsValid)
            {


                var result =
                    await _clientHelper.CallAPI<KeycloakCredentials, KeycloakCredentials>(
                        "/api/KeycloakCredentials/UpdateCredentials", credentials);
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
