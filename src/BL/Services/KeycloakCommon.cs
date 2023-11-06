using IdentityModel.Client;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Services
{
    public class KeycloakCommon
    {
        public static async Task<string> GetTokenForUserGuts(string username, string password, string requiredRole, HttpClientHandler proxyHandler,
            string keycloakBaseUrl, string clientId, string clientSecret)
        {
            Log.Information("{Function} Proxy > " + proxyHandler.Proxy + " keycloakBaseUrl > " + keycloakBaseUrl , "GetTokenForUserGuts");
            var client = new HttpClient(proxyHandler);
            var disco = await client.GetDiscoveryDocumentAsync(new DiscoveryDocumentRequest
            {
                Address = keycloakBaseUrl,
                Policy = new DiscoveryPolicy
                {
                    ValidateIssuerName = false, // Keycloak may have a different issuer name format
                }
            });

            if (disco.IsError)
            {
                Log.Error("{Function} {Error}", "GetTokenForUserGuts", disco.Error);
                return "";
            }

            var tokenResponse = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                UserName = username,
                Password = password
            });


            if (tokenResponse.IsError)
            {
                Log.Error("{Function} {Error} for user {Username}", "GetTokenForUserGuts", tokenResponse.Error, username);
                return "";
            }


            var jwtHandler = new JwtSecurityTokenHandler();
            var token = jwtHandler.ReadJwtToken(tokenResponse.AccessToken);
            var groupClaims = token.Claims.Where(c => c.Type == "realm_access").Select(c => c.Value);
            var roles = new TokenRoles()
            {
                roles = new List<string>()
            };
            if (!string.IsNullOrWhiteSpace(requiredRole))
            {
                if (groupClaims.Any())
                {
                    roles = JsonConvert.DeserializeObject<TokenRoles>(groupClaims.First());
                }

                if (!roles.roles.Any(gc => gc.Equals(requiredRole)))
                {
                    Log.Information("{Function} User {Username} does not have correct role {AdminRole}",
                        "GetTokenForUserGuts", username, requiredRole);
                    return "";
                }

                Log.Information("{Function} Token found with correct role {AdminRole} for User {Username}",
                    "GetTokenForUserGuts", requiredRole, username);
            }
            else
            {
                Log.Information("{Function} Token found for User {Username}, no role required",
                    "GetTokenForUserGuts", requiredRole, username);
            }

            return tokenResponse.AccessToken;
        }
    }
}
