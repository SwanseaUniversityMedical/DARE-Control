using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace BL.Services
{
    public class HutchClientHelper : BaseClientHelper, IHutchClientHelper
    {
        public HutchClientHelper(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IConfiguration config) : base(httpClientFactory, httpContextAccessor, config["Hutch:APIAddress"],  config["IgnoreHutchSSL"])
        {

        }
    }
}
