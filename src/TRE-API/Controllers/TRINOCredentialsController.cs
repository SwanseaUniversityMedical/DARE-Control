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
    [Route("api/tre-credentials/trino")]
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

    }
}
