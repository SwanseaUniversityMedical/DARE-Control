using BL.Models.Settings;
using IdentityModel.Client;
using Newtonsoft.Json;
using Serilog;
using System.IdentityModel.Tokens.Jwt;

namespace BL.Services
{
    public class KeycloakTokenHelper: IKeycloakTokenHelper
    {

        public ControlKeyCloakSettings _settings { get; set; }

        public KeycloakTokenHelper(ControlKeyCloakSettings settings)
        {
            _settings = settings;
        }
        public async Task<string> GetTokenForUser(string username, string password)
        {

            string keycloakBaseUrl = _settings.BaseUrl;
            string clientId = _settings.ClientId;
            string clientSecret = _settings.ClientSecret;

           

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
                UserName = username,// "testingtreapi",
                Password = password// "statuszero0!"
            });


            if (tokenResponse.IsError)
            {
                Log.Error("{Function} {Error} for user {Username}", "GetTokenForUser", tokenResponse.Error, username);
                return "";
            }

            var dareTreAdminRole = "dare-tre-admin";
            var jwtHandler = new JwtSecurityTokenHandler();
            var token = jwtHandler.ReadJwtToken(tokenResponse.AccessToken);
            var groupClaims = token.Claims.Where(c => c.Type == "realm_access").Select(c => c.Value);
            var roles = new TokenRoles()
            {
                roles = new List<string>()
            };
            if (groupClaims.Any())
            {
                roles = JsonConvert.DeserializeObject<TokenRoles>(groupClaims.First());
            }
            if (!roles.roles.Any(gc => gc.Equals(dareTreAdminRole)))
            {
                Log.Information("{Function} User {Username} does not have correct role {AdminRole}", "GetTokenForUser", username, dareTreAdminRole);
                return "";
            }
            Log.Information("{Function} Token found with correct role {AdminRole} for User {Username}", "GetTokenForUser", dareTreAdminRole, username);
            
            return tokenResponse.AccessToken;
        }
    }

    public class TokenRoles
    {
        public List<string> roles { get; set; }
    }
}
