using BL.Models;

using Newtonsoft.Json;
using BL.Repositories.DbContexts;


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Endpoint = BL.Models.Endpoint;

namespace DARE_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EndpointController : Controller
    {
        private readonly ApplicationDbContext _DbContext;


        public EndpointController(ApplicationDbContext applicationDbContext)
        {

            _DbContext = applicationDbContext;

        }
        
        [HttpPost]

        public async Task<Endpoint?> AddEndpoint(FormData data)
        {
            try
            {
                BL.Models.Endpoint endpoints = JsonConvert.DeserializeObject<BL.Models.Endpoint>(data.FormIoString);

                var model = new BL.Models.Endpoint();
                model.Name = endpoints.Name;

                _DbContext.Endpoints.Add(model);

                await _DbContext.SaveChangesAsync();

                Log.Information("{Function} Endpoint created successfully", "AddEndpoint");
                return model;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "AddEndpoint");
                throw;
            }

        }

        [HttpPost]
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
    }
}
