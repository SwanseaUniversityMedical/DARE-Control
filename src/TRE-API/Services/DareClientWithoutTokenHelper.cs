using BL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using TRE_API.Repositories.DbContexts;

namespace TRE_API.Services
{
    public class DareClientWithoutTokenHelper : BaseClientHelper, IDareClientWithoutTokenHelper
    {

        public DareClientWithoutTokenHelper(IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor, IConfiguration config, ApplicationDbContext db,
            IKeycloakTokenHelper keycloak, IEncDecHelper encDec) : base(httpClientFactory, httpContextAccessor,
            config["DareAPISettings:Address"], keycloak)
        {
            
            var creds = db.ControlCredentials.FirstOrDefault();
            if (creds != null)
            {
                _username = creds.UserName;
                _password = encDec.Decrypt(creds.PasswordEnc);
                _requiredRole = "dare-tre-admin";
            }


        }
    }
}
