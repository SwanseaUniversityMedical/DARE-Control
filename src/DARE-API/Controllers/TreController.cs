using Newtonsoft.Json;
using DARE_API.Repositories.DbContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using BL.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using BL.Models;
using BL.Models.Tes;
using DARE_API.Services;

namespace DARE_API.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class TreController : Controller
    {
        private readonly ApplicationDbContext _DbContext;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public TreController(ApplicationDbContext applicationDbContext, IHttpContextAccessor httpContextAccessor)
        {

            _DbContext = applicationDbContext;
            _httpContextAccessor= httpContextAccessor;

        }

        [Authorize(Roles = "dare-control-admin")]
        [HttpPost("SaveTre")]
        public async Task<Tre> SaveTre([FromBody] FormData data)
        {           
            try
            {
                Tre tre = JsonConvert.DeserializeObject<Tre>(data.FormIoString);
                tre.Name = tre.Name?.Trim();
                if (_DbContext.Tres.Any(x => x.Name.ToLower() == tre.Name.ToLower().Trim() && x.Id != tre.Id))
                {
                    
                    return new Tre(){Error = true, ErrorMessage = "Another tre already exists with the same name"};
                }
                
                if  (_DbContext.Tres.Any(x => x.AdminUsername.ToLower() == tre.AdminUsername.ToLower() && x.Id != tre.Id))
                {
                    return new Tre() { Error = true, ErrorMessage = "Another tre already exists with the same TRE Admin Name" };
                }
                if (_DbContext.Tres.Any(x => x.About.ToLower() == tre.About.ToLower() && x.Id != tre.Id))
                {
                    return new Tre() { Error = true, ErrorMessage = "Another tre already exists with the same TRE Admin Name" };
                }
                tre.FormData = data.FormIoString;

                var logtype = LogType.AddTre;
                if (tre.Id > 0)
                {
                    if (_DbContext.Tres.Select(x => x.Id == tre.Id).Any())
                    {
                        _DbContext.Tres.Update(tre);
                        logtype = LogType.UpdateTre;
                    }
                    else
                    {
                        _DbContext.Tres.Add(tre);
                    }
                }

                else {
                    _DbContext.Tres.Add(tre);
                }
                await _DbContext.SaveChangesAsync();
                await ControllerHelpers.AddAuditLog(logtype, null, null, tre, null, null, _httpContextAccessor, User, _DbContext);
             
                return tre;

            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "SaveTre");

                var errorTre = new Tre();
                return errorTre;
                throw;
            }
        }
     
        [HttpGet("GetTresInProject/{projectId}")]
        [AllowAnonymous]
        public List<Tre> GetTresInProject(int projectId)
        {
            try
            {
                List<Tre> treslist = _DbContext.Projects.Where(p => p.Id == projectId).SelectMany(p => p.Tres).ToList();
                return treslist;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function Crashed", "GetTresInProject");
                throw;
            }
        }

        [HttpGet("GetAllTres")]
        [AllowAnonymous]
        public async Task<List<Tre>> GetAllTres()
        {
            try
            {
                var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
                var allTres = _DbContext.Tres.ToList();

                

                Log.Information("{Function} Tres retrieved successfully", "GetAllTres");
                return allTres;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllTres");
                throw;
            }


        }
        
        [HttpGet("GetATre")]
        [AllowAnonymous]
        public Tre? GetATre(int treId)
        {
            try
            {
                var returned = _DbContext.Tres.Find(treId);
                if (returned == null)
                {
                    return null;
                }

                Log.Information("{Function} Project retrieved successfully", "GetATre");
                return returned;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetATre");
                throw;
            }


        }


    }
}
