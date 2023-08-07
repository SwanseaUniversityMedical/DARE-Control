
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using BL.Models.DTO;
using Microsoft.AspNetCore.Http;

using Serilog;

namespace BL.Services
{
    

    public class BaseClientHelper : IBaseClientHelper
    {
        
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly string _address;
        protected readonly JsonSerializerOptions _jsonSerializerOptions;

        public BaseClientHelper(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, string address)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _address = address;
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
                response = await ClientHelperRequestAsync(_address + endPoint, HttpMethod.Get, jsonString, paramlist);
            }
            else if (usePut)
            {
                response = await ClientHelperRequestAsync(_address + endPoint, HttpMethod.Put, jsonString, paramlist);
            }
            else
            {
                response = await ClientHelperRequestAsync(_address + endPoint, HttpMethod.Post, jsonString, paramlist);
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

       


        protected async Task<HttpResponseMessage> ClientHelperRequestAsync(string endPoint, HttpMethod method, StringContent? jsonString = null, Dictionary<string, string>? paramlist = null)
        {
            try
            {
                var usetoken = true;
                if (string.IsNullOrEmpty(endPoint)) return new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.BadRequest };
                if (_httpContextAccessor.HttpContext == null)
                {
                    usetoken = false;
                };

                HttpClient? apiClient;
                if (usetoken)
                {
                    apiClient = await CreateClientWithToken();
                }
                else
                {
                    apiClient = await CreateClientWithOutToken();
                }
                    
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
                        endPoint +=  item.Key + "=" + item.Value;
                        
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
            catch (Exception ex) {
                Log.Error(ex, "{Function} Crash", "ClientHelperRequestAsync");
                throw;
            }
        }

        protected async Task<HttpClient> CreateClientWithToken()
        {
            var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            var apiClient = _httpClientFactory.CreateClient();
            apiClient.SetBearerToken(accessToken);
            apiClient.DefaultRequestHeaders.Add("Accept", "application/json");
            return apiClient;
        }

        protected async Task<HttpClient> CreateClientWithOutToken()
        {
            
            var apiClient = _httpClientFactory.CreateClient();
            apiClient.DefaultRequestHeaders.Add("Accept", "application/json");
            return apiClient;
        }

        #region Helpers

        protected StringContent GetStringContent<T>(T datasetObj) where T : class?
        {
            var jsonString = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(datasetObj, _jsonSerializerOptions),
                Encoding.UTF8,
                "application/json");
            return jsonString;
        }

        public async Task<TOutput?> CallAPI<TInput, TOutput>(string endPoint,TInput model, Dictionary<string, string>? paramList = null) where TInput : class? where TOutput : class?, new()
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
