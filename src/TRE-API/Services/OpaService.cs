using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Diagnostics.Eventing.Reader;
using BL.Models;
using BL.Models.ViewModels;

namespace TRE_API.Services
{
   public class OpaService
    {
        private readonly HttpClient _httpClient;
        private readonly OPASettings _opaSettings;
     public OpaService(OPASettings opaSettings)
        {
            _opaSettings = opaSettings;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_opaSettings.OPAUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }



        public async Task<bool> CheckAccess(string userName, DateTime today, List<Project> treData)
        {
            var input = new
            {
                input = new { user = userName, today },
                data = new { tre = treData }
            };
            var response = await _httpClient.PostAsJsonAsync("app/userallowed/project_allow", input);
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


