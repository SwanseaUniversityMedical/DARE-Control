using Microsoft.AspNetCore.Mvc;
using BL;
using System.Reflection.Metadata.Ecma335;
using TREAPI.Services;

namespace TRE_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HasuraController : Controller
    {

        private readonly IHasuraService _hasuraService;

        public HasuraController(IHasuraService hasuraService)
        {
            _hasuraService = hasuraService;
        }

        [HttpGet("RunQuery/{token}/{Query}")]
        public async  Task<string> RunQuery(string token, string Query)
        {
           string a = await _hasuraService.ExecuteQuery(token, Query);
           return a.ToString();
        }

    }
}
