using Newtonsoft.Json;
using DARE_API.Repositories.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Endpoint = BL.Models.Endpoint;
using BL.Models.DTO;
using BL.Models;

namespace DARE_API.Controllers
{
    [Authorize(Roles = "dare-control-admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class EndpointController : Controller
    {
        private readonly ApplicationDbContext _DbContext;


        public EndpointController(ApplicationDbContext applicationDbContext)
        {

            _DbContext = applicationDbContext;

        }
        
        [HttpPost("AddEndpoint")]

        public async Task<Endpoint?> AddEndpoint(FormData data)
        {
            try
            {
                Endpoint endpoints = JsonConvert.DeserializeObject<Endpoint>(data.FormIoString);

                
                var model = new Endpoint();
                model.Name = endpoints.Name.Trim();
                model.FormData = data.FormIoString;

                if (_DbContext.Endpoints.Any(x => x.Name.ToLower() == endpoints.Name.ToLower().Trim()))
                {
                    return null;
                }

                _DbContext.Endpoints.Add(model);

                await _DbContext.SaveChangesAsync();

                Log.Information("{Function} Endpoint created successfully", "AddEndpoint");
                return model;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "AddEndpoint");
                var errorEndpoint = new Endpoint();
                return errorEndpoint;
                throw;
            }

        }

        [HttpPost("AddEndpointMVC")]

        public async Task<Endpoint?> AddEndpointMVC(FormData data)
        {
            try
            {
                Endpoint model = JsonConvert.DeserializeObject<Endpoint>(data.FormIoString);

                model.Name = model.Name.Trim();
                

                if (_DbContext.Endpoints.Any(x => x.Name.ToLower() == model.Name.ToLower().Trim()))
                {
                    return null;
                }

                model.FormData = data.FormIoString;
                _DbContext.Endpoints.Add(model);

                await _DbContext.SaveChangesAsync();

                Log.Information("{Function} Endpoint created successfully", "AddEndpointMVC");
                return model;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "AddEndpointMVC");
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


        [AllowAnonymous]
        [HttpGet("GetAllEndpoints")]
        public List<Endpoint> GetAllEndpoints()
        {
            try
            {

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
        [AllowAnonymous]
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
