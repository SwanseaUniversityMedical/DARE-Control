using Amazon.Runtime.Internal.Transform;
using BL.Models;
using BL.Models.Settings;
using BL.Services;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using Data_Egress_API.Repositories.DbContexts;
using System.Xml.Linq;

namespace Data_Egress_API.Services
{
    public interface IKeyCloakService
    {
        Task<KeyCloakService.TokenResponse> GenAccessTokenSimple(string adminName, string adminPassword, string max_age);
        Task<List<string>> GetEmailsOfAccountWithRole(string role);
    }

    public class KeyCloakService : IKeyCloakService
    {


        private readonly ApplicationDbContext _DbContext;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public readonly DataEgressKeyCloakSettings _DataEgressKeyCloakSettings;
        private readonly IEncDecHelper _encDecHelper;
        private readonly EmailSettings _EmailSettings;

        public KeyCloakService(ApplicationDbContext applicationDbContext,
            IHttpContextAccessor httpContextAccessor,
            DataEgressKeyCloakSettings DataEgressKeyCloakSettings,
            IEncDecHelper encDecHelper,
            EmailSettings emailSettings)
        {
            _DbContext = applicationDbContext;
            _httpContextAccessor = httpContextAccessor;
            _DataEgressKeyCloakSettings = DataEgressKeyCloakSettings;
            _encDecHelper = encDecHelper;
            _EmailSettings = emailSettings;
        }

        public async Task<List<string>> GetEmailsOfAccountWithRole(string role)
        {

            var creds = _DbContext.KeycloakCredentials.FirstOrDefault(x => x.CredentialType == CredentialType.Egress);

            var adminName = creds.UserName;

            var adminPassword = (creds.PasswordEnc); //_encDecHelper.Decrypt


            // Get an access token from Keycloak using admin credentials
            var token = await GetAccessTokenAsync(adminName, adminPassword, "120");

            string accessToken = token.access_token;
            var client = new HttpClient(GetHttpClientHandler());
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var url = "";

            url = _DataEgressKeyCloakSettings.RootUrl + $"/admin/realms/{_DataEgressKeyCloakSettings.Realm}/roles/{role}/users";
            Log.Information(" GetUserIdAsync url >" + url);
            
           
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            Log.Information(" GetUserIdAsync content >" + content);
  
            var uers = JsonConvert.DeserializeObject<List<User>>(content).Where(x => _EmailSettings.EmailsToIgnor.Contains( x.email) == false ).Select(x => x.email).ToList();

            return uers;
        }

   

        public class TokenRoles
        {
            public List<string> roles { get; set; }
        }

        public class GenAccountData
        {
            public string Name { get; set; }
            public string Password { get; set; }   
            public string Email { get; set; }
        }


        public async Task<TokenResponse> GenAccessTokenSimple(string adminName, string adminPassword, string max_age)
        {
            return await GetAccessTokenAsync( adminName, adminPassword, max_age);
        }
        public async Task<TokenResponse> GetAccessTokenAsync( string adminName, string adminPassword, string max_age)
        {
            var url = _DataEgressKeyCloakSettings.BaseUrl + "/protocol/openid-connect/token";

            Log.Information(" GetAccessTokenAsync url >" + url);
            var client = new HttpClient(GetHttpClientHandler());

            var tokenRequestBody = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                {"client_id", "admin-cli"},
                {"username", adminName},
                {"password", adminPassword},
                {"grant_type", "password"},
                {"max_age", max_age } // Set a longer max_age in seconds
            });

            var response = await client.PostAsync(url, tokenRequestBody);


            var content = await response.Content.ReadAsStringAsync();
            Log.Information(" GetAccessTokenAsync content >" + content);
            var token = JsonConvert.DeserializeObject<TokenResponse>(content);
            return token;
        }

        async Task<string> GetUserIdAsync(string domain, string realm, string accessToken, string username)
        {
            var url = "";
            if (domain.Contains("http")) //NOTEEE will Break if you've got http in url and it's old key cloak
            {
                url = $"{domain}/admin/realms/{realm}/users/?username={username}";
            }
            else
            {
                url = $"http://{domain}/auth/realms/{realm}/protocol/openid-connect/token";
            }

            Log.Information(" GetUserIdAsync url >" + url);
            var client = new HttpClient(GetHttpClientHandler());
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            Log.Information(" GetUserIdAsync content >" + content);
            var users = JsonConvert.DeserializeObject<List<User>>(content);
            return users.Count > 0 ? users[0].Id : null;
        }


  

        public HttpClientHandler GetHttpClientHandler()
        {
            var handler = new HttpClientHandler();

            if (_DataEgressKeyCloakSettings.Proxy)
            {
                handler.Proxy = new WebProxy()
                {
                    Address = new Uri(_DataEgressKeyCloakSettings.ProxyAddresURL),
                    BypassList = new[] { _DataEgressKeyCloakSettings.BypassProxy }
                };
                handler.UseProxy = true;
            }


            return handler;
        }

        public class TokenResponse
        {
            public string access_token { get; set; }
        }

        class User
        {
            public string Id { get; set; }
            public string email { get; set; }
        }


        class NewUserData
        {
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string email { get; set; }
            public string enabled { get; set; }
            public string username { get; set; }
            public List<Credentials> credentials { get; set; } = new List<Credentials>();

            public List<string> requiredActions { get; set; } = new List<string>();
        }



        class Credentials
        {
            public string type { get; set; } = "password";
            public string value { get; set; } = "test123";

            public string temporary { get; set; } = "true";
        }

    }
}
