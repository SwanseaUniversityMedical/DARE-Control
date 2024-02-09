using BL.Models.Settings;
using IdentityModel.Client;
using Newtonsoft.Json;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace BL.Services
{
    public class KeycloakTokenHelper
    {

        
        public string _keycloakBaseUrl { get; set; }
        public string _clientId { get; set; }
        public string _clientSecret { get; set; }
        public bool _useProxy { get; set; }
        public string _proxyUrl { get; set; }

        public KeycloakTokenHelper(string keycloakBaseUrl, string clientId, string clientSecret, bool useProxy, string proxyurl)
        {
            _keycloakBaseUrl = keycloakBaseUrl;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _useProxy = useProxy;
            _proxyUrl = proxyurl;
        }

        public async Task<string> GetTokenForUser(string username, string password, string requiredRole)
        {



            string keycloakBaseUrl = _keycloakBaseUrl;
            string clientId = _clientId;
            string clientSecret = _clientSecret;

            Log.Information($"GetTokenForUser _proxyUrl > {_proxyUrl} UseProxy > {_useProxy}");

            // Create an HttpClientHandler with proxy settings
            HttpClientHandler handler = new HttpClientHandler
            {
                Proxy = new WebProxy(_proxyUrl), // Replace with your proxy server URL
                UseProxy = _useProxy
            };

            // Create an HttpClient with the handler
            return await KeycloakCommon.GetTokenForUserGuts(username, password, requiredRole, handler, keycloakBaseUrl, clientId, clientSecret);

        }
    }

    public class TokenRoles
    {
        public List<string> roles { get; set; }
    }
}
