using BL.Models;
using BL.Models.Settings;
using BL.Services;
using Serilog;
using TRE_API.Repositories.DbContexts;

namespace TRE_API.Services
{
    public class DareClientWithoutTokenHelper : BaseClientHelper, IDareClientWithoutTokenHelper
    {
        public ApplicationDbContext CredDb { get; set; }

        public DareClientWithoutTokenHelper(IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor, IConfiguration config, ApplicationDbContext db,
            IEncDecHelper encDec, SubmissionKeyCloakSettings settings) : base(httpClientFactory, httpContextAccessor,
            config["DareAPISettings:Address"], false)
        {
            CredDb = db;
            _keycloakTokenHelper = new KeycloakTokenHelper(settings.BaseUrl, settings.ClientId, settings.ClientSecret, settings.Proxy, settings.ProxyAddresURL, settings.IgnoreHttps);

            var creds = db.KeycloakCredentials.FirstOrDefault(x => x.CredentialType == CredentialType.Submission);
            if (creds != null)
            {
                _username = creds.UserName;
                _password = encDec.Decrypt(creds.PasswordEnc);
                _requiredRole = "dare-tre-admin";
            }
            //Log.Information("{Function} Creds are there? {Creds} with username {Username}, Password {Password} and role {Role}", "DareClientWithoutTokenHelper", _username, _password, _requiredRole);
        }

        public bool CheckCredsAreAvailable()
        {
            return CredDb.KeycloakCredentials.Any(x => x.CredentialType == CredentialType.Submission);
        }
    }
}
