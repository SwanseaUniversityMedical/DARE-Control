using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BL.Models.ViewModels
{
    public class OpaAuthorizationMiddleware
    {
        private readonly HttpClient _httpClient;
        private readonly string _opaUrl;

        public OpaAuthorizationMiddleware(RequestDelegate next, string opaUrl)
        {
            _httpClient = new HttpClient();
            _opaUrl = opaUrl;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Evaluate the policy using OPA
            //var treuser = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First();

            var input = new
            {
                user = "Patricia",
                today = DateTime.Today
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_opaUrl, requestContent);

            if (!response.IsSuccessStatusCode)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                return;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<AuthorizationResult>(responseBody);

            // Check if the request is authorized
            if (result.allow == "true" & result.project_allow == "true")
            {
                //await _next(context);
                return;
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

            }

        }
    }
}
