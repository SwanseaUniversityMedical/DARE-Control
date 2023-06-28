using BL.Models;
using DARE_FrontEnd.Services.Project;
using RestSharp;
using static DARE_FrontEnd.Controllers.FormsController;

namespace DARE_FrontEnd.Services
{
    public class EndpointHandler : IEndpointHandler
    {

        private readonly IClientHelper _clientHelper;
        private readonly IAPICaller _apiCaller;

        public EndpointHandler(IAPICaller IApiCaller, IClientHelper clientHelper)
        {
            _clientHelper = clientHelper;

            _apiCaller = IApiCaller;

        }

        public async Task<Endpoints> CreateEndpoint(data model)
        {
            try
            {
                //var stringContent = _clientHelper.GetStringContent(new ContainString() { Data = model.ToString()});
                var stringContent = _clientHelper.GetStringContent(model);
                var result = await _clientHelper.GenericHttpRequestWithReturnType<Endpoints>("/api/Endpoint/Add_Endpoint", stringContent);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                throw;
            }
        }

        public async Task<Endpoints> ListOfAllEndpoints(List<Endpoints> endpoints)
        {
            var request = new RestRequest($"https://localhost:7058/api/Endpoint/ListOfAllEndpoints", Method.Get);
            request.Method = Method.Get;
            request.AddHeader("Accept", "application/json");
            request.AddParameter("application/json", ParameterType.RequestBody);
            var test = _apiCaller.Client.Execute<Endpoints>(request);
            return test.Data;

        }
    }
}
