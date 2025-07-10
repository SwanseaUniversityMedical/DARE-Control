using BL.Models.Settings;
using BL.Services;
using Microsoft.AspNetCore.Mvc;
using TRE_API.Repositories.DbContexts;

namespace TRE_API.Controllers
{
    [Route("api/tre-credentials/trino")]
    [ApiController]
    public class TRINOCredentialsController : Controller
    {
        private readonly ApplicationDbContext _DbContext;
        private readonly IEncDecHelper _encDecHelper;
        private readonly KeycloakTokenHelper _keycloakTokenHelper;

        public TRINOCredentialsController(ApplicationDbContext applicationDbContext, IEncDecHelper encDec,
            TreKeyCloakSettings keycloakSettings)
        {
            _encDecHelper = encDec;
            _DbContext = applicationDbContext;
            _keycloakTokenHelper = new KeycloakTokenHelper(keycloakSettings.BaseUrl, keycloakSettings.ClientId,
                keycloakSettings.ClientSecret, keycloakSettings.Proxy, keycloakSettings.ProxyAddressUrl,
                keycloakSettings.KeycloakDemoMode);
        }
    }
}