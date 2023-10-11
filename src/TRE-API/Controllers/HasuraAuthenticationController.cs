using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using Microsoft.AspNetCore.Http;

using TRE_TESK.Models;
using Newtonsoft.Json.Linq;
using TRE_API.Repositories.DbContexts;
using TRE_API.Models;
using TRE_API.Services;

namespace TRE_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HasuraAuthenticationController : Controller
    {
        private readonly AuthenticationSettings _authenticationSettings;
    
        private readonly IHasuraAuthenticationService _hasuraAuthenticationService;

        public HasuraAuthenticationController(AuthenticationSettings AuthenticationSettings, IHasuraAuthenticationService hasuraAuthenticationService)
        {
            _authenticationSettings = AuthenticationSettings;          
            _hasuraAuthenticationService = hasuraAuthenticationService;
        }

        [HttpGet("GetNewToken/{role}")]
        public string GetNewToken(string role)
        {
            return _hasuraAuthenticationService.GetNewToken(role);

        }


        [HttpPost("ExpirerToken/{Token}")]
        public bool ExpirerToken(string Token)
        {
            return _hasuraAuthenticationService.ExpirerToken(Token);

        }


        [HttpGet("")]
        public string Index([FromHeader] string MYCOOLToken)
        {
            return _hasuraAuthenticationService.CheckeTokenAndGetRoles(MYCOOLToken);
        }
    }
}
