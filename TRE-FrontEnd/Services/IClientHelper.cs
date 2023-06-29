﻿using TRE_FrontEnd.Controllers;
using TRE_FrontEnd.Models;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Serilog;


namespace TRE_FrontEnd.Services
{
    public interface IClientHelper
    {

        Task<TOutput?> CallAPI<TInput, TOutput>(string endPoint, TInput model,
            Dictionary<string, string>? paramList = null) where TInput : class? where TOutput : class?, new();

        Task<TOutput?> CallAPIWithoutModel<TOutput>(string endPoint, Dictionary<string, string>? paramList = null)
            where TOutput : class?, new();
    }
    public class ClientHelper : IClientHelper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly WebAPISettings _webAPISettings;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ClientHelper(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, WebAPISettings webAPISettings)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            this._webAPISettings = webAPISettings;
            _jsonSerializerOptions = new JsonSerializerOptions()
            {

                PropertyNameCaseInsensitive = true,
            };

        }

        private async Task<T> CallAPIWithReturnType<T>(string endPoint, StringContent? jsonString = null, Dictionary<string, string>? paramlist = null, bool usePut = false) where T : class?, new()
        {

            HttpResponseMessage response = null;

            if (jsonString == null)
            {
                response = await ClientHelperRequestAsync(_webAPISettings.Address + endPoint, HttpMethod.Get, jsonString, paramlist);
            }
            else if (usePut)
            {
                response = await ClientHelperRequestAsync(_webAPISettings.Address + endPoint, HttpMethod.Put, jsonString, paramlist);
            }
            else
            {
                response = await ClientHelperRequestAsync(_webAPISettings.Address + endPoint, HttpMethod.Post, jsonString, paramlist);
            }


            if (response.IsSuccessStatusCode)
            {
                var result = response.Content;
                try
                {
                    var data = await result.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(data) ?? throw new InvalidOperationException();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

            }
            else
            {
                Log.Error("{Function} Invalid Return Code", "CallAPIWithReturnType");
                throw new Exception("API Failure");
            }


        }

        private async Task<HttpResponseMessage> ClientHelperRequestAsync(string endPoint, HttpMethod method, StringContent? jsonString = null, Dictionary<string, string>? paramlist = null)
        {
            try
            {
                if (string.IsNullOrEmpty(endPoint)) return new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.BadRequest };
                if (_httpContextAccessor.HttpContext == null) { return new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.BadRequest }; }

                var apiClient = await CreateClientWithToken();
                if (paramlist != null)
                {
                    if (endPoint.EndsWith("/"))
                    {
                        endPoint = endPoint.Substring(0, endPoint.Length - 1);
                    }

                    if (!endPoint.EndsWith("?"))
                    {
                        endPoint += "?";
                    }

                    var firstparam = true;
                    foreach (var item in paramlist)
                    {
                        if (firstparam)
                        {
                            firstparam = false;

                        }
                        else
                        {
                            endPoint += "&";
                        }
                        endPoint += item.Key + "=" + item.Value;

                    }
                }

                HttpResponseMessage res = new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };


                if (method == HttpMethod.Get) res = await apiClient.GetAsync(endPoint);
                if (method == HttpMethod.Post) res = await apiClient.PostAsync(endPoint, jsonString);
                if (method == HttpMethod.Put) res = await apiClient.PutAsync(endPoint, jsonString);
                if (method == HttpMethod.Delete) res = await apiClient.DeleteAsync(endPoint);

                Console.Out.Write(res);
                return res;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "ClientHelperRequestAsync");
                throw;
            }
        }

        private async Task<HttpClient> CreateClientWithToken()
        {
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            var apiClient = _httpClientFactory.CreateClient();
            apiClient.SetBearerToken(accessToken);
            apiClient.DefaultRequestHeaders.Add("Accept", "application/json");
            return apiClient;
        }

        #region Helpers

        private StringContent GetStringContent<T>(T datasetObj) where T : class?
        {
            var jsonString = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(datasetObj, _jsonSerializerOptions),
                Encoding.UTF8,
                "application/json");
            return jsonString;
        }

        public async Task<TOutput?> CallAPI<TInput, TOutput>(string endPoint, TInput model, Dictionary<string, string>? paramList = null) where TInput : class? where TOutput : class?, new()
        {
            StringContent? modelString = null;
            if (model != null)
            {
                modelString = GetStringContent<TInput>(model);
            }


            return await CallAPIWithReturnType<TOutput>(endPoint, modelString, paramList);

        }

        public async Task<TOutput?> CallAPIWithoutModel<TOutput>(string endPoint, Dictionary<string, string>? paramList = null) where TOutput : class?, new()
        {


            return await CallAPIWithReturnType<TOutput>(endPoint, null, paramList);

        }



        #endregion
    }
}
