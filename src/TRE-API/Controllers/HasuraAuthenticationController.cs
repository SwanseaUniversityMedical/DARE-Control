using Microsoft.AspNetCore.Mvc;
using TRE_API.Config;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using TRE_API.Services;

namespace TRE_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HasuraAuthenticationController : Controller
    {
        private readonly IHasuraAuthenticationService _hasuraAuthenticationService;

        public HasuraAuthenticationController(AuthenticationSettings AuthenticationSettings, IHasuraAuthenticationService hasuraAuthenticationService)
        {        
            _hasuraAuthenticationService = hasuraAuthenticationService;
        }

        [HttpGet("")]
        public string Index([FromHeader] string Token)
        {
            return _hasuraAuthenticationService.CheckeTokenAndGetRoles(Token);
        }
    }
}
