using BL.Models.DTO;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BL.Services
{
    public class DareClientHelper: BaseClientHelper, IDareClientHelper
    {

        public DareClientHelper(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, DareAPISettings webAPISettings): base(httpClientFactory, httpContextAccessor, webAPISettings)
        {
        
        }
    }
}
