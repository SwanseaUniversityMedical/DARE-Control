using Newtonsoft.Json;
using DARE_API.Repositories.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Endpoint = BL.Models.Endpoint;
using BL.Models.DTO;
using BL.Models;
using Microsoft.AspNetCore.Authentication;

namespace DARE_API.Controllers
{
    [Authorize(Roles = "dare-control-admin")]
    //[ApiController]
    [Route("api/[controller]")]
    public class EndpointController : Controller
    {
        private readonly ApplicationDbContext _DbContext;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public EndpointController(ApplicationDbContext applicationDbContext, IHttpContextAccessor httpContextAccessor)
        {

            _DbContext = applicationDbContext;
            _httpContextAccessor= httpContextAccessor;

        }

        [HttpPost("AddEndpoint")]

        public async Task<Endpoint> AddEndpoint([FromBody] FormData data)
        {           
            try
            {
                Endpoint endpoints = JsonConvert.DeserializeObject<Endpoint>(data.FormIoString);
                endpoints.Name = endpoints.Name.Trim();
                if (_DbContext.Endpoints.Any(x => x.Name.ToLower() == endpoints.Name.ToLower().Trim()))
                {
                    return null;
                }
                endpoints.FormData = data.FormIoString;
                _DbContext.Endpoints.Add(endpoints);

                await _DbContext.SaveChangesAsync();
                return endpoints;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "AddEndpoint");

                var errorEndpoint = new Endpoint();
                return errorEndpoint;
                throw;
            }
        }
     
        [HttpGet("GetEndPointsInProject/{projectId}")]
        [AllowAnonymous]
        public List<Endpoint> GetEndPointsInProject(int projectId)
        {
            try
            {
                List<Endpoint> endpointsList = _DbContext.Projects.Where(p => p.Id == projectId).SelectMany(p => p.Endpoints).ToList();
                return endpointsList;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function Crashed", "GetEndPointsInProject");
                throw;
            }
        }

        [HttpGet("GetAllEndpoints")]
        public async Task<List<Endpoint>> GetAllEndpoints()
        {
            try
            {
                var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
                var allEndpoints = _DbContext.Endpoints.ToList();

                

                Log.Information("{Function} Endpoints retrieved successfully", "GetAllEndpoints");
                return allEndpoints;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllEndpoints");
                throw;
            }


        }
        
        [HttpGet("GetAnEndpoint")]
        public Endpoint? GetAnEndpoint(int endpointId)
        {
            try
            {
                var returned = _DbContext.Endpoints.Find(endpointId);
                if (returned == null)
                {
                    return null;
                }

                Log.Information("{Function} Project retrieved successfully", "GetAnEndpoint");
                return returned;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAnEndpoint");
                throw;
            }


        }
    }
}
