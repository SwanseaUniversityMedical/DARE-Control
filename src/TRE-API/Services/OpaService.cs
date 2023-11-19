using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Diagnostics.Eventing.Reader;
using BL.Models;
using BL.Models.ViewModels;
using Newtonsoft.Json;
using System.Text;
using System.Linq;
using Serilog;

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
            _httpClient.BaseAddress = new System.Uri("http://localhost:8181/v1/data/app/checkaccess");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> CheckAccess(string userName, DateTime expiryDate, List<TreProject>? treData)
        {           
            var inputData = new
            {
                input = new
                {
                userName = userName,
                expiryDate = expiryDate,
                treData = new { tre = treData },
                }
        };
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            string jsonInput = JsonConvert.SerializeObject(inputData,settings);
            var content = new StringContent(jsonInput, Encoding.UTF8, "application/json");
            var requestUri = $"http://localhost:8181/v1/data/app/checkaccess";
        
            var response = await _httpClient.PostAsync(requestUri,content);
            var resultjson = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<CheckAccessResponse>(resultjson);
            bool allow = responseObject?.Result?.Allow ?? false;
            if (allow)
            {
                Log.Information("{Function}Opa User Access Allowed for:" + userName, "CheckUserAccess");
            }
            else
            {
                Log.Information("{Function}Opa User Access Denied for:" + userName, "CheckUserAccess");
            }
            return allow;
        }
    }
}


