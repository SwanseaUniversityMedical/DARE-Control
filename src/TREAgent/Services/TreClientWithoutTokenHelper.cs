using BL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;


namespace TREAgent.Services
{
    public class TreClientWithoutTokenHelper : BaseClientHelper, ITreClientWithoutTokenHelper
    {

        public TreClientWithoutTokenHelper(IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor, IConfiguration config,
            IKeycloakTokenHelper keycloak, IEncDecHelper encDec, StoredKeycloakLogin creds) : base(httpClientFactory,
            httpContextAccessor,
            config["TREAPISettings:Address"], keycloak)
        {
            _username = creds.Username;
            _password = encDec.Decrypt(creds.EncPass);
            _requiredRole = "dare-tre-agent";

        }
    }
}
