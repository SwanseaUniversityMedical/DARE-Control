using Microsoft.AspNetCore.Mvc;
using BL.Services.Project;
using RestSharp;

namespace Project_Admin.Services.Project
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