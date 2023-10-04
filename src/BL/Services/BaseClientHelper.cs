using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Serilog;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.FileProviders;

namespace BL.Services
{
  
    public class BaseClientHelper : IBaseClientHelper
    {
        
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly string _address;
        protected readonly JsonSerializerOptions _jsonSerializerOptions;
        protected readonly IKeycloakTokenHelper? __keycloakTokenHelper;
        public string _requiredRole { get; set; }
        public string _password { get; set; }
        public string _username { get; set; }
        

        public BaseClientHelper(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, string address, IKeycloakTokenHelper? keycloakTokenHelper)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _address = address;
            
            __keycloakTokenHelper = keycloakTokenHelper;
            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                
                PropertyNameCaseInsensitive = true,
            };
            
        }


        private async Task<T> CallAPIWithReturnType<T>(string endPoint, StringContent? jsonString = null, Dictionary<string, string>? paramlist = null, bool usePut = false, string? fileParameterName = null, IFormFile? file = null) where T : class?, new()
        {

            HttpResponseMessage response = null;

            if (jsonString == null && file == null)
            {
                response = await ClientHelperRequestAsync(_address + endPoint, HttpMethod.Get, jsonString, paramlist, fileParameterName, file);
            }
            else if (usePut)
            {
                response = await ClientHelperRequestAsync(_address + endPoint, HttpMethod.Put, jsonString, paramlist, fileParameterName, file);
            }
            else
            {
                response = await ClientHelperRequestAsync(_address + endPoint, HttpMethod.Post, jsonString, paramlist, fileParameterName, file);
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

       


        protected async Task<HttpResponseMessage> ClientHelperRequestAsync(string endPoint, HttpMethod method, StringContent? jsonString, Dictionary<string, string>? paramlist, string? fileParameterName, IFormFile? fileInfo)
        {
            try
            {
                var usetoken = true;
                if (string.IsNullOrEmpty(endPoint)) return new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.BadRequest };
                if (_httpContextAccessor.HttpContext == null)
                {
                    usetoken = false;
                };
                usetoken = true;
                HttpClient? apiClient;
                if (usetoken)
                {
                    apiClient = await CreateClientWithKeycloak();
                }
                else
                {
                    apiClient = await CreateClientWithOutKeycloak();
                }
                    
                endPoint = ConstructEndPoint(endPoint, paramlist);

                HttpResponseMessage res = new HttpResponseMessage
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };

                if (fileInfo != null && fileParameterName != null)
                {
                    res = await UploadFileAsync(fileInfo, apiClient, fileParameterName, endPoint);
                }
                else
                {
                    if (method == HttpMethod.Get) res = await apiClient.GetAsync(endPoint);
                    if (method == HttpMethod.Post) res = await apiClient.PostAsync(endPoint, jsonString);
                    if (method == HttpMethod.Put) res = await apiClient.PutAsync(endPoint, jsonString);
                    if (method == HttpMethod.Delete) res = await apiClient.DeleteAsync(endPoint);
                }
                if (!res.IsSuccessStatusCode)
                {
                   throw new Exception("API Call Failure: " + res.StatusCode + ": " + res.ReasonPhrase);
                }
                Log.Information("{Function} The response {res}", "ClientHelperRequestAsync", res);
               
                return res;
            }
            catch (Exception ex) {
                Log.Error(ex, "{Function} Crash", "ClientHelperRequestAsync");
                throw;
            }
        }

        private static string ConstructEndPoint(string endPoint, Dictionary<string, string>? paramlist)
        {
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

            return endPoint;
        }

        protected async Task<HttpClient> CreateClientWithKeycloak()
        {
            var accessToken = "";
            if (__keycloakTokenHelper != null)
            {
                accessToken = await  __keycloakTokenHelper.GetTokenForUser(_username, _password, _requiredRole);
            }
            else
            {
                accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
            }
            
            var apiClient = _httpClientFactory.CreateClient();
            apiClient.SetBearerToken(accessToken);
            apiClient.DefaultRequestHeaders.Add("Accept", "application/json");
            return apiClient;
        }

        protected async Task<HttpClient> CreateClientWithOutKeycloak()
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

        public async Task<HttpResponseMessage> CallAPI(string endPoint, StringContent? jsonString, Dictionary<string, string>? paramList = null, bool usePut = false)
        {
            return  await ClientHelperRequestAsync(_address + endPoint, HttpMethod.Post, jsonString, paramList, null, null);
        }

        public async Task<TOutput?> CallAPI<TInput, TOutput>(string endPoint,TInput model, Dictionary<string, string>? paramList = null, bool usePut = false) where TInput : class? where TOutput : class?, new()
        {
            StringContent? modelString = null;
            if (model != null)
            {
                modelString = GetStringContent<TInput>(model);
            }
            
            return await CallAPIWithReturnType<TOutput>(endPoint, modelString, paramList, usePut);
        }

        public async Task<TOutput?> CallAPIWithoutModel<TOutput>(string endPoint, Dictionary<string, string>? paramList = null) where TOutput : class?, new()
        {
            return await CallAPIWithReturnType<TOutput>(endPoint, null, paramList);
        }


        public async Task<TOutput?> CallAPIToSendFile<TOutput>(string endPoint, string fileParamaterName, IFormFile file, Dictionary<string, string>? paramList = null) where TOutput : class?, new()
        {
            return await CallAPIWithReturnType<TOutput>(endPoint, null, paramList, false, fileParamaterName, file);
        }

        public async Task<HttpResponseMessage> UploadFileAsync(IFormFile file, HttpClient apiClient, string fileParameterName, string endPoint)
        {
            using (var formData = new MultipartFormDataContent())
            {
                // Create a StreamContent from the IFormFile
                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = fileParameterName, // Match the property name in your API's FileInfo class
                    FileName = file.FileName
                };
                formData.Headers.ContentType = new MediaTypeHeaderValue("multipart/form-data");
                formData.Add(streamContent);
                
                
               
                    var response = await apiClient.PostAsync(endPoint, formData);
                    return response;
                   
               
            }
        }

        #endregion
    }
}
