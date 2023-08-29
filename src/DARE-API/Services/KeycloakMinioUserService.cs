using BL.Models.ViewModels;
using DARE_API.Services.Contract;
using IdentityModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace DARE_API.Services
{
    public class KeycloakMinioUserService : IKeycloakMinioUserService
    {
        public async Task<bool> SetMinioUserAttribute(string accessToken, string userName, string attributeName, string attributeValue)
        {
            try
            {
                HttpClient httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var baseUrl = "auth2.ukserp.ac.uk";
                var realm = "Dare-Control";
                var userId = await GetUserIDAsync(baseUrl, realm, accessToken, userName);
                var user = await GetUserAttributesAsync(baseUrl, realm, accessToken, userId);

                if (user.attributes == null)
                {
                    user.attributes = new UserAttributes();
                }

                if (user.attributes.Policy == null)
                {
                    user.attributes.Policy = new string[] { attributeValue };
                }
                else
                {
                    var policyIndex = Array.IndexOf(user.attributes.Policy, attributeName);
                    if (policyIndex >= 0)
                    {
                        user.attributes.Policy[policyIndex] = attributeValue;
                    }
                    else
                    {
                        var updatedPolicy = new string[user.attributes.Policy.Length + 1];
                        Array.Copy(user.attributes.Policy, updatedPolicy, user.attributes.Policy.Length);
                        updatedPolicy[updatedPolicy.Length - 1] = attributeValue;
                        user.attributes.Policy = updatedPolicy;
                    }
                }

                var apiUrlforUpdate = $"https://{baseUrl}/admin/realms/{realm}/users/{user.id}";

                var updatedUserJson = JsonConvert.SerializeObject(user);
                var updatedContent = new StringContent(updatedUserJson, Encoding.UTF8, "application/json");
                var putResponse = await httpClient.PutAsync(apiUrlforUpdate, updatedContent);


            }
            catch (Exception ex)
            {

                throw;
            }
            return true;
        }
        public async Task<bool> RemoveMinioUserAttribute(string accessToken, string userName, string attributeName, string attributeValue)
        {
            try
            {
                HttpClient httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var baseUrl = "auth2.ukserp.ac.uk";
                var realm = "Dare-Control";
                var userId = await GetUserIDAsync(baseUrl, realm, accessToken, userName);
                var user = await GetUserAttributesAsync(baseUrl, realm, accessToken, userId);

                if (user.attributes != null && user.attributes.Policy != null)
                {
                    //var updatedPolicy = user.attributes.Policy.Where(attr => attr != attributeValue).ToArray();
                    //user.attributes.Policy = updatedPolicy;
                    if (user.attributes.Policy.Contains(attributeValue))
                    {
                        user.attributes.Policy = user.attributes.Policy.Where(attr => attr != attributeValue).ToArray();
                    }
                }

                var apiUrlforUpdate = $"https://{baseUrl}/admin/realms/{realm}/users/{user.id}";

                var updatedUserJson = JsonConvert.SerializeObject(user);
                var updatedContent = new StringContent(updatedUserJson, Encoding.UTF8, "application/json");
                var putResponse = await httpClient.PutAsync(apiUrlforUpdate, updatedContent);

                if (!putResponse.IsSuccessStatusCode)
                {
                    // Handle error here
                }

            }
            catch (Exception ex)
            {

                throw;
            }
            return true;
        }
        static async Task<string> GetUserIDAsync(string baseUrl, string realm, string accessToken, string userName)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var apiUrl = $"https://{baseUrl}/admin/realms/{realm}/users?username={userName}";

            var response = await httpClient.GetAsync(apiUrl);

            var jsonString = await response.Content.ReadAsStringAsync();
            try
            {
                JArray jsonObject = JsonConvert.DeserializeObject<JArray>(jsonString);

                var userId = jsonObject[0]["id"].ToString();

                return userId;
            }
            catch (Exception ex)
            {

                throw;
            }


            return string.Empty;
        }
        static async Task<KeycloakUser> GetUserAttributesAsync(string baseUrl, string realm, string accessToken, string userID)
        {
            HttpClient httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var apiUrl = $"https://{baseUrl}/admin/realms/{realm}/users/{userID}";

            var response = await httpClient.GetAsync(apiUrl);

            var userJson = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<KeycloakUser>(userJson);

            return user;
        }
    }
}
