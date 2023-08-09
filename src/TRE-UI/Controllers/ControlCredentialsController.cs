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


        [HttpGet("UpdateCredentials")]

        public async Task<IActionResult> UpdateCredentialsAsync()
        {
            var alreadyset = await _clientHelper.CallAPIWithoutModel<BoolReturn>("/api/ControlCredentials/AreCredentialsSet");


            return View(new ControlCredentials()
                { AlreadySet = alreadyset.Result });
        }

        [HttpPost("UpdateCredentials")]
        
        public async Task<IActionResult> UpdateCredentials(ControlCredentials credentials) {


            var result = await _clientHelper.CallAPI<ControlCredentials, ControlCredentials>("/api/ControlCredential/UpdateCredentials", credentials);
            return RedirectToAction("Index", "Home");
        }

       
        public async Task<string> GetTokenForUser()
        {

            //string clientId = "Dare-TRE-UI";
            //string clientSecret = "5da4W2dwgRfYb3Jfp9if7KSPQLiMZ93I";
            string clientId = "Dare-Control-UI";
            string clientSecret = "PUykmXOAkyYhIdKzpYz0anPlQ74gUBTz";
            var authority = "https://auth2.ukserp.ac.uk/realms/Dare-Control";

            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = authority,
                Policy = new DiscoveryPolicy
                {
                    ValidateIssuerName = false, // Keycloak may have a different issuer name format
                }
            });

            if (disco.IsError)
            {
                Log.Error("{Function} {Error}", "GetTokenForUser", disco.Error);
                return "";
            }

            var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                UserName = "testingtreapi",
                Password = "statuszero0!"
            });


            if (tokenResponse.IsError)
            {
                Log.Error("{Function} {Error}", "GetTokenForUser", tokenResponse.Error);
                return "";
            }

            Log.Information("{Function} {AccessToken}", "GetTokenForUser", tokenResponse.AccessToken);
            Log.Information(tokenResponse.RefreshToken);
            return tokenResponse.AccessToken;
        }

    
     
    }
}
