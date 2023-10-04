using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace BL.Services
{
    public class HutchClientHelper : BaseClientHelper, IDataEgressClientHelper
    {
        public HutchClientHelper (IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IConfiguration config) : base(httpClientFactory, httpContextAccessor, config["DataEgressAPISettings:Address"], null)
        {

        }
    }
}
