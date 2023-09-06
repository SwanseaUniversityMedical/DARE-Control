using BL.Models.Settings;
using BL.Models.ViewModels;
using DARE_API.Services.Contract;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Net.Http.Headers;
using System.Text;

namespace DARE_API.Services
{
    public class KeycloakMinioUserService : IKeycloakMinioUserService
    {
        private readonly SubmissionKeyCloakSettings _submissionKeyCloakSettings;
        public KeycloakMinioUserService(SubmissionKeyCloakSettings submissionKeyCloakSettings)
        {
            _submissionKeyCloakSettings = submissionKeyCloakSettings;
        }
        public async Task<bool> SetMinioUserAttribute(string accessToken, string userName, string attributeName, string attributeValueToAdd)
        {
            try
            {
                var baseUrl = _submissionKeyCloakSettings.Server;
                var realm = _submissionKeyCloakSettings.Realm;
                var attributeKey = "policy";
                var userId = await GetUserIDAsync(baseUrl, realm, accessToken, userName);
                var userAttributesJson = await GetUserAttributesAsync(baseUrl, realm, accessToken, userId);

                if (userAttributesJson != null)
                {

                    JObject user = JObject.Parse(userAttributesJson);

                    if (user["attributes"][attributeKey] != null)
                    {
                        var existingValues = user["attributes"][attributeKey].ToObject<JArray>();
                        existingValues.Add(attributeValueToAdd);
                        user["attributes"][attributeKey] = existingValues;
                    }
                    else
                    {
                        user["attributes"][attributeKey] = new JArray(attributeValueToAdd);
                    }


                    string updatedUserData = user.ToString();


                    bool updateResult = await UpdateUserAttributes(baseUrl, realm, userId, accessToken, updatedUserData);

                    if (updateResult)
                    {
                        Log.Information("{Function} attributes added successfully", "SetMinioUserAttribute");
                        return true;
                    }
                    else
                    {
                        Log.Error("{Function} Failed to update user attributes.", "SetMinioUserAttribute");
                        return true;
                    }
                }
                else
                {
                    Log.Error("{Function} Failed to retrieve user attributes.", "SetMinioUserAttribute");
                    return true;
                }


            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public async Task<bool> RemoveMinioUserAttribute(string accessToken, string userName, string attributeName, string attributeValueToRemove)
        {
            try
            {

                var baseUrl = _submissionKeyCloakSettings.Server;
                var realm = _submissionKeyCloakSettings.Realm;
                var attributeKey = "policy";
                var userId = await GetUserIDAsync(baseUrl, realm, accessToken, userName);
                var userAttributesJson = await GetUserAttributesAsync(baseUrl, realm, accessToken, userId);

                if (userAttributesJson != null)
                {

                    JObject user = JObject.Parse(userAttributesJson);

                    if (user["attributes"][attributeKey] != null)
                    {

                        var existingValues = user["attributes"][attributeKey].ToObject<JArray>();


                        var updatedValues = new JArray();


                        foreach (var value in existingValues)
                        {
                            if (value.ToString() != attributeValueToRemove)
                            {
                                updatedValues.Add(value);
                            }
                        }

                        user["attributes"][attributeKey] = updatedValues;
                    }

                    string updatedUserData = user.ToString();

                    bool updateResult = await UpdateUserAttributes(baseUrl, realm, userId, accessToken, updatedUserData);

                    if (updateResult)
                    {
                        Log.Information("{Function} attributes added successfully.", "RemoveMinioUserAttribute");
                        return true;
                    }
                    else
                    {
                        Log.Error("{Function} Failed to update user attributes.", "RemoveMinioUserAttribute");
                        return false;
                    }
                }
                else
                {
                    Log.Error("{Function} Failed to retrieve user attributes.", "RemoveMinioUserAttribute");
                    return false;
                }

            }
            catch (Exception ex)
            {

                throw;
            }
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
        static async Task<string> GetUserAttributesAsync(string baseUrl, string realm, string accessToken, string userID)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri($"https://{baseUrl}/admin/realms/{realm}/users/{userID}");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.GetAsync("");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return null;
                }
            }
        }
        static async Task<bool> UpdateUserAttributes(string keycloakBaseUrl, string realm, string userId, string accessToken, string updatedUserData)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri($"https://{keycloakBaseUrl}/admin/realms/{realm}/users/{userId}");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var content = new StringContent(updatedUserData, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PutAsync("", content);

                return response.IsSuccessStatusCode;
            }
        }
    }
}
