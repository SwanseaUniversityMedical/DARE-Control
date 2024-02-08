using BL.Models.Settings;
using BL.Models.ViewModels;
using DARE_API.Services.Contract;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System.IO;
using System.Net.Http.Headers;
using System.Runtime;
using System.Text;
using System.Net;

namespace DARE_API.Services
{
    public class KeycloakMinioUserService : IKeycloakMinioUserService
    {
        public  SubmissionKeyCloakSettings _submissionKeyCloakSettings;
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
                var userId = await GetUserIDAsync(accessToken, userName);
                var userAttributesJson = await GetUserAttributesAsync(baseUrl, realm, accessToken, userId);

                if (userAttributesJson != null)
                {

                    JObject user = JObject.Parse(userAttributesJson);

                    if (user["attributes"] == null)
                    {
                        JObject attributes = new JObject();

                        // Add the "attributes" object to the user object
                        user["attributes"] = attributes;
                    }
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
                var userId = await GetUserIDAsync(accessToken, userName);
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
        public async Task<string> GetUserIDAsync(string accessToken, string userName)
        {
            var baseUrl = _submissionKeyCloakSettings.Server;
            var realm = _submissionKeyCloakSettings.Realm;
            HttpClient httpClient = new HttpClient(_submissionKeyCloakSettings.getProxyHandler);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var apiUrl = $"https://{baseUrl}/admin/realms/{realm}/users?username={userName}";

            var response = await httpClient.GetAsync(apiUrl);

            var jsonString = await response.Content.ReadAsStringAsync();

            Log.Information($"GetUserIDAsync jsonString > {jsonString}");

            try
            {
                
                JArray jsonObject = JsonConvert.DeserializeObject<JArray>(jsonString);
                foreach (var item in jsonObject)
                {
                    if (item["username"].ToString().ToLower() == userName.ToLower())
                    {
                        return item["id"].ToString();
                    }
                }

                return string.Empty;
                //throw new Exception("User not found");
            }
            catch (Exception ex)
            {

                throw;
            }


            return string.Empty;
        }
        public async Task<string> GetUserAttributesAsync(string baseUrl, string realm, string accessToken, string userID)
        {
            using (var httpClient = new HttpClient(_submissionKeyCloakSettings.getProxyHandler))
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
        public async Task<bool> UpdateUserAttributes(string keycloakBaseUrl, string realm, string userId, string accessToken, string updatedUserData)
        {
            using (var httpClient = new HttpClient(_submissionKeyCloakSettings.getProxyHandler))
            {
                httpClient.BaseAddress = new Uri($"https://{keycloakBaseUrl}/admin/realms/{realm}/users/{userId}");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var content = new StringContent(updatedUserData, System.Text.Encoding.UTF8, "application/json");
                var response = await httpClient.PutAsync("", content);
                var stream = response.Content.ReadAsStream();
                string content2 = "";
                using (StreamReader reader = new StreamReader(stream))
                {
                    // Read the stream content into a string
                    content2 = reader.ReadToEnd();

                    // Output the string content

                }
                return response.IsSuccessStatusCode;
            }
        }
    }
}
