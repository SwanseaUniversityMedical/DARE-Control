using BL.Models;
using DARE_FrontEnd.Services.Project;
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

        public async Task<BL.Models.Endpoint> CreateEndpoint(data model)
        {
            try
            {
                //var stringContent = _clientHelper.GetStringContent(new ContainString() { Data = model.ToString()});
                var stringContent = _clientHelper.GetStringContent(model);
                var result = await _clientHelper.GenericHttpRequestWithReturnType<BL.Models.Endpoint>("/api/Endpoint/Add_Endpoint", stringContent);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                throw;
            }
        }
    }
}
