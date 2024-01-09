﻿using System.Collections.Generic;
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
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc.Rendering;
using BL.Services;
using Build.Security.AspNetCore.Middleware.Dto;

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

        public async Task<bool> UserPermit(string userName, string projectName, DateTime expiryDate, TreProject? treData, string treName,List<Tre?>? treuser, DateTime selectedexpirydate)
        {
            LoadPolicyAsync("package app.userpermit\r\n\r\nimport future.keywords.if\r\nimport future.keywords.in\r\n\r\ndefault allow := false\r\n\r\ndefault any_invalid_tre := false\r\n\r\ndefault any_valid_users := true\r\n\r\ndefault project_allow := false\r\n\r\ndefault any_is_user_allowed := false\r\n\r\ndefault any_is_tre_data_valid := false\r\n\r\nallow if {\r\n\tis_tre_data_valid(input.treData)\r\n\tis_user_allowed(input.userName)\r\n}\r\n\r\nis_tre_data_valid(treData) if {\r\n\ttreData != null\r\n\tcount(treData) > 0\r\n}\r\n\r\nis_user_allowed(userName) if {\r\n\tuserName != null\r\n\tcount(userName) > 0\r\n}\r\n\r\nproject_allow if {\r\n\tany_is_tre_data_valid\r\n\tany_is_user_allowed\r\n}\r\n").Wait();
            var treUser = treuser.Select(treUser => new { name = treUser.AdminUsername, expiry = selectedexpirydate }).ToList();

            var inputData = new
            {
                input = new
                {
                    id = projectName,
                    Description = treData.Description,
                    trecount = 1,
                    tre = treName,
                    treData = new { name = treName, active = true },
                    users = new { treUser }
                },
            };
      
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            string jsonInput = JsonConvert.SerializeObject(inputData, settings);
            EvaluatePolicyAndCreateProject(jsonInput, projectName).Wait();

            var content = new StringContent(jsonInput, Encoding.UTF8, "application/json");
            var requestUri = $"http://localhost:8181/v1/data/app/userpermit";

            var response = await _httpClient.PostAsync(requestUri, content);
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
        public async Task LoadPolicyAsync(string policy)

        {

            var policyContent = new StringContent(policy, Encoding.UTF8, "text/plain");

            var policyResponse = await _httpClient.PutAsync("/v1/policies/userpermit", policyContent);

            policyResponse.EnsureSuccessStatusCode();

        }
        public async Task<string> EvaluatePolicyAndCreateProject(string input, string projectName)

        {
            var inputContent = new StringContent(input, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/v1/data/app/userpermit", inputContent);
            response.EnsureSuccessStatusCode();

            var evaluationResult = await response.Content.ReadAsStringAsync();

            // Check the evaluation result to decide whether to create the project

            if (ShouldCreateProject(evaluationResult, projectName))

            {

                // Call a method to create the project

                await CreateProjectAsync(projectName);

                return "Project created";

            }

            else

            {

                return "Project already exists or creation not allowed by policy";

            }

        }



        private bool ShouldCreateProject(string evaluationResult, string projectName)

        {

            // Implement logic to determine if the project should be created based on the evaluation result

            // You should parse the evaluation result and apply your policy-specific logic here

            // For example, check if the project name exists in the result

            // Return true if the project should be created, false otherwise

            return !evaluationResult.Contains($"\"{projectName}\":");

        }



        private async Task CreateProjectAsync(string projectName)

        {

            // Implement the logic to create the project

            // This could involve making a call to another part of your application or to an external service

            // For demonstration purposes, we'll simply print a message here

            Console.WriteLine($"Creating project: {projectName}");

            // You can add the actual project creation logic here

        }



        // Other methods for data loading, evaluation, and policy saving remain the same

        // ...

    }

}



