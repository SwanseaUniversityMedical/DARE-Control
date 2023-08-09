using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace BL.Services
{
    public class TREClientHelper: BaseClientHelper, ITREClientHelper
    {
        public TREClientHelper(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor , IConfiguration config) : base(httpClientFactory, httpContextAccessor, config["TREAPISettings:Address"])
        {

        }
    }
}
