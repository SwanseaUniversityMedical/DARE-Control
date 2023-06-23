using RestSharp;

namespace TRE_FrontEnd.Services.Project
{
    public class APICaller : IAPICaller
    {
        public RestClient Client { get; set; }

        public APICaller(string URL)
        {
            Client = new RestClient(URL);
        }
    }

    public interface IAPICaller
    {
        RestClient Client { get; set; }
    }
}
