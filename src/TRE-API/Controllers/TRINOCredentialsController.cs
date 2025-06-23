using BL.Models.APISimpleTypeReturns;
using BL.Models;
using BL.Models.Settings;
using Microsoft.AspNetCore.Authorization;
using BL.Services;
using Microsoft.AspNetCore.Mvc;
using TRE_API.Repositories.DbContexts;
using TRE_API.Services;

namespace TRE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TRINOCredentialsController : Controller
    {
        private readonly ApplicationDbContext _DbContext;
        private readonly IEncDecHelper _encDecHelper;
        private readonly KeycloakTokenHelper _keycloakTokenHelper;

        public TRINOCredentialsController (ApplicationDbContext applicationDbContext, IEncDecHelper encDec, TreKeyCloakSettings keycloakSettings)
        {
            _encDecHelper = encDec;
            _DbContext = applicationDbContext;
            _keycloakTokenHelper = new KeycloakTokenHelper(keycloakSettings.BaseUrl, keycloakSettings.ClientId,
                keycloakSettings.ClientSecret, keycloakSettings.Proxy, keycloakSettings.ProxyAddresURL, keycloakSettings.KeycloakDemoMode);
        }

        [Authorize(Roles = "dare-tre-admin")]
        [HttpGet("validate")]
        public async Task<BoolReturn> CheckCredentialsAreValidAsync()
        {
            return await ControllerHelpers.CheckCredentialsAreValid(_keycloakTokenHelper, _encDecHelper, _DbContext, CredentialType.Submission);
        }
    }
}
