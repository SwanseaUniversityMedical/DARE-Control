﻿using BL.Models.Settings;
using IdentityModel.Client;
using Newtonsoft.Json;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Runtime;

namespace BL.Services
{
    public class KeycloakTokenHelper
    {

        
        public string _keycloakBaseUrl { get; set; }
        public string _clientId { get; set; }
        public string _clientSecret { get; set; }
        public bool _useProxy { get; set; }
        public bool _demoMode { get; set; }
        public string _proxyUrl { get; set; }

        public KeycloakTokenHelper(string keycloakBaseUrl, string clientId, string clientSecret, bool useProxy, string proxyurl, bool demoMode)
        {
            _keycloakBaseUrl = keycloakBaseUrl;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _useProxy = useProxy;
            _proxyUrl = proxyurl;
            _demoMode = demoMode;
        }

        public async Task<string> GetTokenForUser(string username, string password, string requiredRole)
        {



            string keycloakBaseUrl = _keycloakBaseUrl;
            string clientId = _clientId;
            string clientSecret = _clientSecret;

            Log.Information($"GetTokenForUser _proxyUrl > {_proxyUrl} UseProxy > {_useProxy}");
            HttpClientHandler handler = new HttpClientHandler();
            handler.UseProxy = _useProxy;
            if (_useProxy)
            {
                // Create an HttpClientHandler with proxy settings
                handler.Proxy = new WebProxy(_proxyUrl); // Replace with your proxy server URL

            }

            Log.Information("{Function} 2  demoMode {DemoMode}", "GetTokenForUser", _demoMode);
            // Create an HttpClient with the handler
            return await KeycloakCommon.GetTokenForUserGuts(username, password, requiredRole, handler, keycloakBaseUrl, clientId, clientSecret, _demoMode);

        }
    }

    public class TokenRoles
    {
        public List<string> roles { get; set; }
    }
}
