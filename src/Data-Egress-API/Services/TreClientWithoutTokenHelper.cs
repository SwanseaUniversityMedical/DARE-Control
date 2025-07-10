using BL.Models;
using BL.Models.Settings;
using BL.Services;
using Data_Egress_API.Repositories.DbContexts;


namespace Data_Egress_API.Services
{
    public class TreClientWithoutTokenHelper : BaseClientHelper, ITreClientWithoutTokenHelper
    {
        public ApplicationDbContext CredDb { get; set; }

        public TreClientWithoutTokenHelper(IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor, IConfiguration config, ApplicationDbContext db,
             IEncDecHelper encDec, TreKeyCloakSettings settings) : base(httpClientFactory, httpContextAccessor,
            config["TreAPISettings:Address"], false)
        {
            CredDb = db;
            _keycloakTokenHelper = new KeycloakTokenHelper(settings.BaseUrl, settings.ClientId, settings.ClientSecret, settings.Proxy, settings.ProxyAddressUrl, settings.KeycloakDemoMode);
            var creds = db.KeycloakCredentials.FirstOrDefault(x => x.CredentialType == CredentialType.Tre);
            if (creds != null)
            {
                _username = creds.UserName;
                _password = encDec.Decrypt(creds.PasswordEnc);
                _requiredRole = "data-egress-admin";
            }


        }

        public bool CheckCredsAreAvailable()
        {
            return CredDb.KeycloakCredentials.Any(x => x.CredentialType == CredentialType.Tre);
        }
    }
}
