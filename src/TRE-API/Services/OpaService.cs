using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Diagnostics.Eventing.Reader;
using BL.Models;
using BL.Models.ViewModels;
using Newtonsoft.Json;

namespace TRE_API.Services
{
    public class OpaService
    {
        private readonly HttpClient _httpClient;
        private readonly OPASettings _opaSettings;
        public OpaService()
        {
            _httpClient = new HttpClient();
            //_httpClient.BaseAddress = new Uri(_opaSettings.OPAUrl);
            _httpClient.BaseAddress = new System.Uri("http://localhost:8181/v1/policies/checkaccess");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> CheckAccess(string userName, DateTime expiryDate, List<TreProject>? treData)
        {         
            DateTime today = DateTime.Today;
            if (expiryDate > today)
            {
                expiryDate = DateTime.Now.AddMinutes(_opaSettings.ExpiryDelayMinutes);
            }
            var inputData = new
            {
                userName = userName,
                expiryDate = "2023-12-1T00:00:00Z",
                treData = new { tre = treData },
      
            };
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            string jsonInput = JsonConvert.SerializeObject(inputData,settings);
            var requestUri = $"http://localhost:8181/v1/data/app/checkaccess{jsonInput}";

            var response = await _httpClient.GetAsync(requestUri);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            // Handle error cases/throwing new exception;
            return false;
        }
    }
}


