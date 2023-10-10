using BL.Models.Settings;
using BL.Services;
using TRE_API.Repositories.DbContexts;

namespace TRE_API.Services
{
    public class DataEgressClientWithoutTokenHelper : BaseClientHelper, IDataEgressClientWithoutTokenHelper
    {
        public ApplicationDbContext CredDb { get; set; }

        public DataEgressClientWithoutTokenHelper(IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor, IConfiguration config, ApplicationDbContext db,
            IEncDecHelper encDec, DataEgressKeyCloakSettings settings) : base(httpClientFactory, httpContextAccessor,
            config["DareAPISettings:Address"])
        {
            CredDb = db;
            _keycloakTokenHelper = new KeycloakTokenHelper(settings.BaseUrl, settings.ClientId, settings.ClientSecret, settings.Proxy, settings.ProxyAddresURL);

            var creds = db.SubmissionCredentials.FirstOrDefault();
            if (creds != null)
            {
                _username = creds.UserName;
                _password = encDec.Decrypt(creds.PasswordEnc);
                _requiredRole = "dare-tre-admin";
            }


        }

        public bool CheckCredsAreAvailable()
        {
            return CredDb.SubmissionCredentials.Any();
        }
    }
}
