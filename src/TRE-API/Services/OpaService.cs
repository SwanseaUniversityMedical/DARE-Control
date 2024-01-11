﻿using System.Collections.Generic;
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
        static
        public async Task<bool> UserPermit(string projectName, TreProject? treData, string treName,List<Tre?>? treuser, DateTime selectedexpirydate)
        {
            var policy = PolicyHelper.GetPolicy();
        
            var treUser = treuser.Select(treUser => new { name = treUser.AdminUsername, expiry = selectedexpirydate }).ToList();

            var inputData= new
            {
                input = new
                {
                    id = projectName,
                    Description = treData.Description,
                    trecount = 1,
                    tre  = new { name = treName, active = true },
                    users = new { treUser }
                },
            };
      
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            string jsonInput = JsonConvert.SerializeObject(inputData, settings);
            LoadPolicyAsync(policy, jsonInput);
            EvaluatePolicyAndCreateProject(jsonInput, projectName).Wait();

            var content = new StringContent(jsonInput, Encoding.UTF8, "application/json");
            var requestUri = $"http://localhost:8181/v1/data/app/userpermit";

            var response = await _httpClient.PostAsync(requestUri, content);
            var resultjson = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<CheckAccessResponse>(resultjson);
            bool allow = responseObject?.Result?.Allow ?? false;
            if (allow)
            {
                Log.Information("{Function}Opa User Access Allowed for:" + projectName, "CheckUserAccess");
            }
            else
            {
                Log.Information("{Function}Opa User Access Denied for:" + projectName, "CheckUserAccess");
            }
            return allow;
        }
        public async Task LoadPolicyAsync(string policy, string data)

        {
            var policyContent = new StringContent(policy, Encoding.UTF8, "text/plain");

            var policyResponse = await _httpClient.PutAsync("/v1/policies/userpermit", policyContent);

            policyResponse.EnsureSuccessStatusCode();

            
            policyContent = new StringContent(data, Encoding.UTF8, "application/json");

            policyResponse = await _httpClient.PutAsync("/v1/data/dareprojectdata", policyContent);

            policyResponse.EnsureSuccessStatusCode();

        }
        public async Task<string> EvaluatePolicyAndCreateProject(string input, string projectName)

        {
            var inputContent = new StringContent(input, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/v1/data/app/userpermit", inputContent);
            response.EnsureSuccessStatusCode();

            var evaluationResult = await response.Content.ReadAsStringAsync();
           
            // Checks if project does not exist

            if (ShouldCreateProject(evaluationResult, projectName))

            {
                // create project
                    
                await CreateProjectAsync(projectName);

                return "Project created";
            }
            else
            {
                return "Project already exists ";
            }

        }

        private bool ShouldCreateProject(string evaluationResult, string projectName)

        {

            return !evaluationResult.Contains($"\"{projectName}\":");

        }
        private async Task CreateProjectAsync(string projectName)

        {
      
            Console.WriteLine($"Creating project: {projectName}");

            // adds project 

        }

    }

}



