using BL.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Services
{
    public class TREClientHelper: BaseClientHelper, ITREClientHelper
    {
        public TREClientHelper(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor , IConfiguration config) : base(httpClientFactory, httpContextAccessor, config["TREAPISettings:Address"])
        {

        }
    }
}
