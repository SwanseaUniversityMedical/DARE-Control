using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BL.Services
{
    public class ICODAClientHelper : BaseClientHelper, IICODAClientHelper
    {
        public ICODAClientHelper(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IConfiguration config) : base(httpClientFactory, httpContextAccessor, config["ICODA-Address:Address"], null)
        {


        }

        public Task<TOutput?> CallAPIWithAccessCode<TInput, TOutput>(string endPoint, string AccessCode, TInput model, Dictionary<string, string>? paramList = null, bool usePut = false)
            where TInput : class
            where TOutput : class, new()
        {
            StringContent? modelString = null;
            if (model != null)
            {
                modelString = GetStringContent<TInput>(model);
            }


            return CallAPIWithReturnTypeICODA<TOutput>(endPoint, AccessCode, modelString, paramList, usePut);

            throw new NotImplementedException();
        }

        public Task<TOutput?> CallAPIWithoutModelWithAccessCode<TOutput>(string endPoint, string AccessCode, Dictionary<string, string>? paramList = null) where TOutput : class, new()
        {
            return CallAPIWithReturnTypeICODA<TOutput>(endPoint, AccessCode, null, paramList);
        }

        public async Task GenericHTTPRequest(string endPoint, string AccessCode,StringContent jsonString = null, bool usePut = false)
        {
            var response = jsonString == null
                ? await ClientHelperRequestAsyncICODA(_address + endPoint, AccessCode, HttpMethod.Get)
                : usePut
                    ? await ClientHelperRequestAsyncICODA(_address + endPoint, AccessCode, HttpMethod.Put, jsonString)
                    : await ClientHelperRequestAsyncICODA(_address + endPoint, AccessCode, HttpMethod.Post, jsonString);
        }

        private async Task<T> CallAPIWithReturnTypeICODA<T>(string endPoint, string AccessCode, StringContent? jsonString = null, Dictionary<string, string>? paramlist = null, bool usePut = false) where T : class?, new()
        {

            HttpResponseMessage response = null;

            if (jsonString == null)
            {
                response = await ClientHelperRequestAsyncICODA(_address + endPoint, AccessCode, HttpMethod.Get, jsonString, paramlist);
            }
            else if (usePut)
            {
                response = await ClientHelperRequestAsyncICODA(_address + endPoint, AccessCode, HttpMethod.Put, jsonString, paramlist);
            }
            else
            {
                response = await ClientHelperRequestAsyncICODA(_address + endPoint, AccessCode, HttpMethod.Post, jsonString, paramlist);
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
                    Log.Error(e, "{Function} Failed deserialising string ", "CallAPIWithReturnType");
                    throw;
                }

            }
            else
            {
                Log.Error("{Function} Invalid Return Code", "CallAPIWithReturnType");
                throw new Exception("API Failure");
            }
        }

        private async Task<HttpResponseMessage> ClientHelperRequestAsyncICODA(string endPoint, string AccessCode, HttpMethod method, StringContent? jsonString = null, Dictionary<string, string>? paramlist = null)
        {
            try
            {
                if (string.IsNullOrEmpty(endPoint)) return new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.BadRequest };

                HttpClient? apiClient;

                apiClient = await CreateClient(AccessCode);

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
                if (!res.IsSuccessStatusCode)
                {
                    throw new Exception("API Call Failure: " + res.StatusCode + ": " + res.ReasonPhrase);
                }
                Log.Information("{Function} The response {res}", "ClientHelperRequestAsync", res);
                Console.Out.Write(res);
                return res;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "ClientHelperRequestAsync");
                throw;
            }
        }

        private async Task<HttpClient> CreateClient(string token)
        {
            var apiClient = _httpClientFactory.CreateClient();
            apiClient.SetBearerToken(token);
            apiClient.DefaultRequestHeaders.Add("Accept", "application/json");
            return apiClient;
        }

    }
}
