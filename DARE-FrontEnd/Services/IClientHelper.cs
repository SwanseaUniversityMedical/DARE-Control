using DARE_FrontEnd.Models;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;

namespace DARE_FrontEnd.Services
{
    public interface IClientHelper
    {
        Task<T> GenericHttpRequestWithReturnType<T>(string endPoint, StringContent jsonString = null, bool usePut = false) where T : class, new();
        Task GenericHTTPRequest(string endPoint, StringContent jsonString = null, bool usePut = false);

        Task<HttpResponseMessage> ClientHelperRequestAsync(string endPoint, HttpMethod method, StringContent? jsonString = null);

        StringContent GetStringContent<T>(T datasetObj) where T : class;
    }

    public class ClientHelper : IClientHelper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly WebAPISettings _webAPISettings;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ClientHelper(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, WebAPISettings webAPISettings)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            this._webAPISettings = webAPISettings;
            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                //AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
            };
        }

        public async Task<T> GenericHttpRequestWithReturnType<T>(string endPoint, StringContent jsonString = null, bool usePut = false) where T : class, new()
        {

            HttpResponseMessage response = null;

            if (jsonString == null)
            {
                response = await ClientHelperRequestAsync(_webAPISettings.Address + endPoint, HttpMethod.Get);
            }
            else if (usePut)
            {
                response = await ClientHelperRequestAsync(_webAPISettings.Address + endPoint, HttpMethod.Put, jsonString);
            }
            else
            {
                response = await ClientHelperRequestAsync(_webAPISettings.Address + endPoint, HttpMethod.Post, jsonString);
            }


            if (response.IsSuccessStatusCode)
            {
                var result = response.Content;
                try
                {
                    var data = await result.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            }

            return null;
        }

        public async Task GenericHTTPRequest(string endPoint, StringContent jsonString = null, bool usePut = false)
        {
            var response = jsonString == null
                ? await ClientHelperRequestAsync(_webAPISettings.Address + endPoint, HttpMethod.Get)
                : usePut
                    ? await ClientHelperRequestAsync(_webAPISettings.Address + endPoint, HttpMethod.Put, jsonString)
                    : await ClientHelperRequestAsync(_webAPISettings.Address + endPoint, HttpMethod.Post, jsonString);
        }


        public async Task<HttpResponseMessage> ClientHelperRequestAsync(string endPoint, HttpMethod method, StringContent? jsonString = null)
        {
            try
            {
                if (string.IsNullOrEmpty(endPoint)) return new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.BadRequest };
                if (_httpContextAccessor.HttpContext == null) { return new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.BadRequest }; }

                var apiClient = await CreateClientWithToken();


                HttpResponseMessage res = new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };

                if (method == HttpMethod.Get) res = await apiClient.GetAsync(endPoint);
                if (method == HttpMethod.Post) res = await apiClient.PostAsync(endPoint, jsonString);
                if (method == HttpMethod.Put) res = await apiClient.PutAsync(endPoint, jsonString);
                if (method == HttpMethod.Delete) res = await apiClient.DeleteAsync(endPoint);

                Console.Out.Write(res);
                return res;
            }
            catch (Exception ex) {
                return null;
            }
        }

        private async Task<HttpClient> CreateClientWithToken()
        {
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            var apiClient = _httpClientFactory.CreateClient();
            apiClient.SetBearerToken(accessToken);
            apiClient.DefaultRequestHeaders.Add("Accept", "application/json");
            return apiClient;
        }

        #region Helpers

        public StringContent GetStringContent<T>(T datasetObj) where T : class
        {
            var jsonString = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(datasetObj, _jsonSerializerOptions),
                Encoding.UTF8,
                "application/json");
            return jsonString;
        }

        #endregion
    }
}
