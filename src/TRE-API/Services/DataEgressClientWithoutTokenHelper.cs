using BL.Services;
using TRE_API.Repositories.DbContexts;

namespace TRE_API.Services
{
    public class DataEgressClientWithoutTokenHelper : BaseClientHelper, IDataEgressClientHelper
    {
        public ApplicationDbContext CredDb { get; set; }

        public DataEgressClientWithoutTokenHelper(IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor, IConfiguration config, ApplicationDbContext db,
            IKeycloakTokenHelper keycloak, IEncDecHelper encDec) : base(httpClientFactory, httpContextAccessor,
            config["DareAPISettings:Address"], keycloak)
        {
            CredDb = db;
            
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
