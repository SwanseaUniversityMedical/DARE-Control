using Newtonsoft.Json;
using DARE_API.Repositories.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
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
        public async Task<Tre> AddEndpoint([FromBody] FormData data)
        {           
            try
            {
                Tre tre = JsonConvert.DeserializeObject<Tre>(data.FormIoString);
                tre.Name = tre.Name?.Trim();
                if (_DbContext.Tres.Any(x => x.Name.ToLower() == tre.Name.ToLower().Trim() && x.Id != tre.Id))
                {
                    
                    return new Tre(){Error = true, ErrorMessage = "Another endpoint already exists with the same name"};
                }
                
                if  (_DbContext.Tres.Any(x => x.AdminUsername.ToLower() == tre.AdminUsername.ToLower() && x.Id != tre.Id))
                {
                    return new Tre() { Error = true, ErrorMessage = "Another endpoint already exists with the same TRE Admin Name" };
                }
                if (_DbContext.Tres.Any(x => x.About.ToLower() == tre.About.ToLower() && x.Id != tre.Id))
                {
                    return new Tre() { Error = true, ErrorMessage = "Another endpoint already exists with the same TRE Admin Name" };
                }
                tre.FormData = data.FormIoString;

                if (tre.Id > 0)
                {
                    if(_DbContext.Tres.Select(x => x.Id == tre.Id).Any())
                        _DbContext.Tres.Update(tre);
                    else
                        _DbContext.Tres.Add(tre);
                }

                else {
                    _DbContext.Tres.Add(tre);
                }
                await _DbContext.SaveChangesAsync();
                return tre;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "AddEndpoint");

                var errorEndpoint = new Tre();
                return errorEndpoint;
                throw;
            }
        }
     
        [HttpGet("GetEndPointsInProject/{projectId}")]
        [AllowAnonymous]
        public List<Tre> GetEndPointsInProject(int projectId)
        {
            try
            {
                List<Tre> endpointsList = _DbContext.Projects.Where(p => p.Id == projectId).SelectMany(p => p.Tres).ToList();
                return endpointsList;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function Crashed", "GetEndPointsInProject");
                throw;
            }
        }

        [HttpGet("GetAllEndpoints")]
        public async Task<List<Tre>> GetAllEndpoints()
        {
            try
            {
                var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
                var allEndpoints = _DbContext.Tres.ToList();

                

                Log.Information("{Function} Tres retrieved successfully", "GetAllEndpoints");
                return allEndpoints;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllEndpoints");
                throw;
            }


        }
        
        [HttpGet("GetAnEndpoint")]
        public Tre? GetAnEndpoint(int endpointId)
        {
            try
            {
                var returned = _DbContext.Tres.Find(endpointId);
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
