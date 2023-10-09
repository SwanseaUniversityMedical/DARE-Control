using BL.Models.Settings;
using IdentityModel.Client;
using Newtonsoft.Json;
using Serilog;
using System.IdentityModel.Tokens.Jwt;

namespace BL.Services
{
    public class KeycloakTokenHelper
    {

        
        public string _keycloakBaseUrl { get; set; }
        public string _clientId { get; set; }
        public string _clientSecret { get; set; }

        public KeycloakTokenHelper(string keycloakBaseUrl, string clientId, string clientSecret)
        {
            _keycloakBaseUrl = keycloakBaseUrl;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public async Task<string> GetTokenForUser(string username, string password, string requiredRole)
        {



            string keycloakBaseUrl = _keycloakBaseUrl;
            string clientId = _clientId;
            string clientSecret = _clientSecret;



            var client = new HttpClient();

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
                Log.Error("{Function} {Error}", "GetTokenForUser", disco.Error);
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
                Log.Error("{Function} {Error} for user {Username}", "GetTokenForUser", tokenResponse.Error, username);
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
                        "GetTokenForUser", username, requiredRole);
                    return "";
                }

                Log.Information("{Function} Token found with correct role {AdminRole} for User {Username}",
                    "GetTokenForUser", requiredRole, username);
            }
            else
            {
                Log.Information("{Function} Token found for User {Username}, no role required",
                    "GetTokenForUser", requiredRole, username);
            }

            return tokenResponse.AccessToken;

        }
    }

    public class TokenRoles
    {
        public List<string> roles { get; set; }
    }
}
