using Microsoft.AspNetCore.Mvc;
using System.Web.Http;
using TRE_TESK.Services;

namespace TRE_TESK.Controllers
{
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    public class HasuraController : ApiController
    {
        private readonly IHasuraService _IHasuraService;
        public HasuraController(IHasuraService IHasuraService) {
            _IHasuraService = IHasuraService;
        }

        [Microsoft.AspNetCore.Mvc.HttpPost("")]
        public async Task SetUpHasura()
        {
            await _IHasuraService.Run();
            return;
        }
    }
}
