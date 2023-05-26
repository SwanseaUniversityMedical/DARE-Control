using BL.Models;
using DARE_FrontEnd.Services.FormIO;
using DARE_FrontEnd.Services.Project;
using Newtonsoft.Json;
using RestSharp;

namespace DARE_FrontEnd.Services
{
    public class FormHandler : IFormHandler
    {
        private readonly IAPICaller _apiCaller;

        public FormHandler(IAPICaller IApiCaller)
        {

            _apiCaller = IApiCaller;
        }

        public async Task<FormData> GetFormDataById(int id)
        {

            var request = new RestRequest($"https://localhost:7058/api/Project/Get_FormData/{id}", Method.Get);
            request.Method = Method.Get;
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json", JsonConvert.SerializeObject(id), ParameterType.RequestBody);
            var test = _apiCaller.Client.Execute<FormData>(request);
            return test.Data;

            //return await _apiCaller.GenericGetData<FormData>($"/api/FormData/GetFormData/{id}");
        }
    }
}
