using BL.Models;
using BL.Models.APISimpleTypeReturns;
using BL.Models.Settings;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TRE_API.Repositories.DbContexts;
using TRE_API.Services;
namespace TRE_API.Controllers
{
    [Route("api/[controller]")]

    [ApiController]
    public class TRECredentialsController : Controller
    {

        private readonly ApplicationDbContext _DbContext;
        private readonly IEncDecHelper _encDecHelper;
        private readonly KeycloakTokenHelper _keycloakTokenHelper;

        public TRECredentialsController(ApplicationDbContext applicationDbContext, IEncDecHelper encDec, TreKeyCloakSettings keycloakSettings)
        {
            _encDecHelper = encDec;
            _DbContext = applicationDbContext;
            _keycloakTokenHelper = new KeycloakTokenHelper(keycloakSettings.BaseUrl, keycloakSettings.ClientId,
                keycloakSettings.ClientSecret, keycloakSettings.Proxy, keycloakSettings.ProxyAddresURL, keycloakSettings.KeycloakDemoMode);
        }

        [Authorize(Roles = "dare-tre-admin")]
        [HttpGet("CheckCredentialsAreValid")]
        public async Task<BoolReturn> CheckCredentialsAreValidAsync()
        {
            return await ControllerHelpers.CheckCredentialsAreValid(_keycloakTokenHelper, _encDecHelper, _DbContext, CredentialType.Tre);
        }
        [Authorize(Roles = "dare-tre-admin")]
        [HttpPost("UpdateCredentials")]

        public async Task<KeycloakCredentials> UpdateCredentials(KeycloakCredentials creds)
        {
            creds = await ControllerHelpers.UpdateCredentials(creds, _keycloakTokenHelper, _DbContext, _encDecHelper, CredentialType.Tre, "dare-tre-admin");
            return creds;
        }
    }
}