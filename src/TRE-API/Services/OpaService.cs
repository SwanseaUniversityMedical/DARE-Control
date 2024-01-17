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
        public async Task<bool> LoadPolicyAsync(string treName, TreProject? treproject, List<UserExpiryInfo> userExpiryInfoList)
        {
            var policy = PolicyHelper.GetPolicy();
    
            var inputData = new PolicyInputData          
            {              
                    Id = treproject.Id.ToString(),
                    Description = treproject.Description,
                    trecount = 1,
                    tre = new TreClass{ name = treName, active = true, users = userExpiryInfoList },
                        
                    
            };
           
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            string jsonInput = JsonConvert.SerializeObject(inputData, settings);



            var policyContent = new StringContent(policy, Encoding.UTF8, "text/plain");

            var policyResponse = await _httpClient.PutAsync("/v1/policies/userpermit", policyContent);

            policyResponse.EnsureSuccessStatusCode();

            
            policyContent = new StringContent(jsonInput, Encoding.UTF8, "application/json");

            policyResponse = await _httpClient.PutAsync("/v1/data/dareprojectdata", policyContent);

            policyResponse.EnsureSuccessStatusCode();

            var opaUserList = await GetOpaUserLinkAsync();

            var updatedProjectJson = JsonConvert.SerializeObject(inputData, settings);
            
            return true;
          
        }
        private async Task<List<UserExpiryInfo>> GetOpaUserLinkAsync()

        {

            var response = await _httpClient.GetAsync("/v1/data/dareprojectdata");
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                //Deserialize the response content into a list of UserExpiryInfo
                var userList = JsonConvert.DeserializeObject<List<UserExpiryInfo>>(responseContent);
                return userList;
            }
            else
            {
                throw new Exception($"Failed to retrieve user list from opa.Status code:{response.StatusCode}");
            }
        
        }

        
        private async Task CreateProjectAsync(string projectName)

        {
      
            Console.WriteLine($"Creating project: {projectName}");

            // adds project 

        }

    }

}



