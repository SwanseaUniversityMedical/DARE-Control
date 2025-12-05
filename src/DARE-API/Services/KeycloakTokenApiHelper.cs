using BL.Models.Settings;
using Serilog;
using BL.Services;
using DARE_API.Services.Contract;

namespace DARE_API.Services
{
    public class KeycloakTokenApiHelper : IKeycloakTokenApiHelper
    {
        public SubmissionKeyCloakSettings _settings { get; set; }


        public KeycloakTokenApiHelper(SubmissionKeyCloakSettings settings)
        {
            _settings = settings;
        }
        public async Task<string> GetTokenForUser(string username, string password, string requiredRole)
        {
            string keycloakBaseUrl = _settings.BaseUrl;
            string clientId = _settings.ClientId;
            string clientSecret = _settings.ClientSecret;
            var proxyhandler = _settings.getProxyHandler;
            
            
            Log.Information("{Function}} 1 using proxyhandler _settings.Authority > {Authority}, KeycloakDemoMode {KeyCloakDemoMode}", "GetTokenForUser", _settings.Authority, _settings.KeycloakDemoMode);
            return (await KeycloakCommon.GetTokenForUserGuts(username, password, requiredRole, proxyhandler, keycloakBaseUrl, clientId, clientSecret, _settings.KeycloakDemoMode)).token;
        }

    }
    
}
