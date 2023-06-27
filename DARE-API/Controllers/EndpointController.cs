using BL.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using BL.Repositories.DbContexts;

using static BL.Controllers.UserController;
using Microsoft.AspNetCore.Authorization;

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
        //public IActionResult Index()
        //{
        //    return View();
        //}
       
        [HttpPost("Add_Endpoint")]

        public async Task<BL.Models.Endpoint> AddEndpoint(data data)
        {
            try
            {
                BL.Models.Endpoint endpoints = JsonConvert.DeserializeObject<BL.Models.Endpoint>(data.FormIoString);

                var model = new BL.Models.Endpoint();
                model.Name = endpoints.Name;
                //model.Projects = endpoints.Projects.ToList();
                
                _DbContext.Endpoints.Add(model);

                await _DbContext.SaveChangesAsync();


                return model;
            }
            catch (Exception ex) { }

            return null;


        }
    }
}
