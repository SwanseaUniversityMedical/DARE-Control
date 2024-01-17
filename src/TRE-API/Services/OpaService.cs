using System.Collections.Generic;
using System;
using System.IO;
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
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc.Rendering;
using BL.Services;
using Build.Security.AspNetCore.Middleware.Dto;
using TRE_API.Models;
using Amazon.S3.Model;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Components.Forms;

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
            _httpClient.BaseAddress = new System.Uri("http://localhost:8181/v1/data/app/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        public async Task<bool> UpdateOpaListAsync(string treName, TreProject? treproject, List<UserExpiryInfo> userExpiryInfoList)
        {
         
            var inputData = new PolicyInputData          
            {              
                    Id = treproject.Id.ToString(),
                    Description = treproject.Description,
                    trecount = 1,
                    tre = new List<TreClass> { new TreClass { name = treName, active = true, users = userExpiryInfoList }},
                        

            };          
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            string jsonInput = JsonConvert.SerializeObject(inputData, settings);

            await LoadPolicy();
            

            var opaUserList = await GetOpaUserLinkAsync();
            foreach (var user in opaUserList)
            {
                if (user.Id == treproject.Id.ToString() )
                {
                    var dataContent = new StringContent(jsonInput, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PutAsync($"/v1/data/dareprojectdata/{user.Id}", dataContent);

                    response.EnsureSuccessStatusCode();
                    var responseData = await response.Content.ReadAsStringAsync();
                    var updatedInputData = JsonConvert.DeserializeObject<PolicyInputData>(responseData);
               //update
                
                }
            }
            return true;
          
        }
        private async Task<List<PolicyInputData>> GetOpaUserLinkAsync()

        {

            var response = await _httpClient.GetAsync("/v1/data/dareprojectdata");
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

               
                var userList = JsonConvert.DeserializeObject<List<PolicyInputData>>(responseContent);
              
                return userList;
            }
            else
            {
                throw new Exception($"Failed to retrieve user list from opa.Status code:{response.StatusCode}");
            }
     
        }

        
        private async Task<bool> LoadData(string data)

        {
            var dataContent = new StringContent(data, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync("/v1/data/dareprojectdata", dataContent);

            response.EnsureSuccessStatusCode();

            return true;

        }

        private async Task<bool> LoadPolicy()

        {
            var policy = PolicyHelper.GetPolicy();

            var policyContent = new StringContent(policy, Encoding.UTF8, "text/plain");

            var Response = await _httpClient.PutAsync("/v1/policies/userpermit", policyContent);
            Response.EnsureSuccessStatusCode();

            return true;

        }

    }

}



