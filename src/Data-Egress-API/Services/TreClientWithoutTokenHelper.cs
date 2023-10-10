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
            config["TreAPISettings:Address"])
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
