using BL.Models;
using DARE_FrontEnd.Services.Project;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace DARE_FrontEnd.Services
{
    public interface IClientHelper
    {
        Task<HttpResponseMessage> ClientHelperRequestAsync(string endPoint, HttpMethod method, StringContent? jsonString = null);

    }

    public class ClientHelper : IClientHelper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly IProjectsHandler _projectsHandler;
        public ClientHelper(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IProjectsHandler _projectsHandler)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                //AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
            };
        }
        public async Task<HttpResponseMessage> ClientHelperRequestAsync(string endPoint, HttpMethod method, StringContent? jsonString = null)
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


            return res;
        }

        private async Task<HttpClient> CreateClientWithToken()
        {
            var user = new User();
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            var apiClient = _httpClientFactory.CreateClient();
            //await _projectsHandler.GetNewToken(user.Id);
            apiClient.SetBearerToken(accessToken);
            apiClient.DefaultRequestHeaders.Add("Accept", "application/json");
            return apiClient;
        }
    }
}
