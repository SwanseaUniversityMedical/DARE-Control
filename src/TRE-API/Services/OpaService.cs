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
            var input = new
            {
                input = new { user = userName, expiryDate },
                data = new { tre = treData }
            };
            //var queryString = $"input={JsonConvert.SerializeObject(input)}";
            var inputData = new
            {

                userName = "PatriciaAkinkuade",

                expiryDate = "2023-12-31T00:00:00Z",

                treData = new { tre = treData },

                time = "2023-11-13T12:34:56Z"

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
                var result = await response.Content.ReadAsAsync<Dictionary<string, object>>();
                return (bool)result["result"];
            }
            // Handle error cases/throwing new exception;
            return false;
        }
    }
}


