using Newtonsoft.Json;
using DARE_API.Repositories.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Endpoint = BL.Models.Endpoint;
using BL.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using BL.Models;

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
                Endpoint endpoint = JsonConvert.DeserializeObject<Endpoint>(data.FormIoString);
                endpoint.Name = endpoint.Name?.Trim();
                if (_DbContext.Endpoints.Any(x => x.Name.ToLower() == endpoint.Name.ToLower().Trim() && x.Id != endpoint.Id))
                {
                    
                    return new Endpoint(){Error = true, ErrorMessage = "Another endpoint already exists with the same name"};
                }
                
                if  (_DbContext.Endpoints.Any(x => x.AdminUsername.ToLower() == endpoint.AdminUsername.ToLower() && x.Id != endpoint.Id))
                {
                    return new Endpoint() { Error = true, ErrorMessage = "Another endpoint already exists with the same TRE Admin Name" };
                }
                endpoint.FormData = data.FormIoString;
                _DbContext.Endpoints.Add(endpoint);

                await _DbContext.SaveChangesAsync();
                return endpoint;

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

        [HttpPost("EditEndpoint")]
        public async Task<Endpoint?> EditEndpoint(FormData data)
        {
            try
            {
                Endpoint endpoint = JsonConvert.DeserializeObject<Endpoint>(data.FormIoString);
                var id = data.Id;
                var dbendpoint = _DbContext.Endpoints.Find(id);
                if (_DbContext.Projects.Any(x => x.Name.ToLower() == endpoint.Name.ToLower().Trim() && x.Id != endpoint.Id))
                {

                    return new Endpoint() { Error = true, ErrorMessage = "Another endpoint already exists with the same name" };
                }

                if (dbendpoint != null)
                {
                    dbendpoint.Id = id;
                    dbendpoint.Name = endpoint.Name;
                    dbendpoint.AdminUsername = endpoint.AdminUsername;
                    dbendpoint.FormData = data.FormIoString;
                }

                _DbContext.Endpoints.Update(dbendpoint);

                await _DbContext.SaveChangesAsync();

                Log.Information("{Function} Endpoint Updated successfully", "EditEndpoint");
                return dbendpoint;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "EditEndpoint");
                var errorModel = new Endpoint();
                return errorModel;
                throw;
            }

        }

    }
}
