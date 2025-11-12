using BL.Models.Settings;
using DARE_API.Services.Contract;
using IdentityModel.Client;
using Newtonsoft.Json;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using BL.Models.ViewModels;
using BL.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authentication;

namespace DARE_API.Services
{
    public class KeyclockTokenAPIHelper : IKeyclockTokenAPIHelper
    {
        public SubmissionKeyCloakSettings _settings { get; set; }


        public KeyclockTokenAPIHelper(SubmissionKeyCloakSettings settings)
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
            return await KeycloakCommon.GetTokenForUserGuts(username, password, requiredRole, proxyhandler, keycloakBaseUrl, clientId, clientSecret, _settings.KeycloakDemoMode);
        }

    }
    
}
