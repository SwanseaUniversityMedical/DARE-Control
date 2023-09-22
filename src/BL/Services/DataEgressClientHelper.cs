using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace BL.Services
{
    public class DataEgressClientHelper : BaseClientHelper, IDataEgressClientHelper
    {
        public DataEgressClientHelper(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IConfiguration config) : base(httpClientFactory, httpContextAccessor, config["DataEgressAPISettings:Address"], null)
        {

        }
    }
}
