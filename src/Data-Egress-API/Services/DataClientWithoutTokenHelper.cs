using BL.Services;
using Data_Egress_API.Repositories.DbContexts;

namespace Data_Egress_API.Services
{
    public class DataClientWithoutTokenHelper : BaseClientHelper, IDataClientWithoutTokenHelper
    {
        public ApplicationDbContext CredDb { get; set; }

        public DataClientWithoutTokenHelper(IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor, IConfiguration config, ApplicationDbContext db,
            IKeycloakTokenHelper keycloak, IEncDecHelper encDec) : base(httpClientFactory, httpContextAccessor,
            config["DataEgressAPISettings"], keycloak)
        {
            CredDb = db;

            var creds = db.SubmissionCredentials.FirstOrDefault();
            if (creds != null)
            {
                _username = creds.UserName;
                _password = encDec.Decrypt(creds.PasswordEnc);
                _requiredRole = "data-egress-admin";
            }


        }

        public bool CheckCredsAreAvailable()
        {
            return CredDb.SubmissionCredentials.Any();
        }
    }
}
