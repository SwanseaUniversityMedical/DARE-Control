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

        public async Task<Endpoints> AddEndpoint(data data)
        {
            try
            {
                Endpoints endpoints = JsonConvert.DeserializeObject<Endpoints>(data.FormIoString);

                var model = new Endpoints();
                model.Name = endpoints.Name;
                //model.Projects = endpoints.Projects.ToList();
                
                _DbContext.Endpoints.Add(model);

                await _DbContext.SaveChangesAsync();


                return model;
            }
            catch (Exception ex) { }

            return null;


        }

        [HttpGet("ListOfAllEndpoints")]

        public List<Endpoints> GetListOfAllEndpoints()
        {
            List<Endpoints> endpoints = _DbContext.Endpoints.ToList();

    
            return endpoints;
        }

    }
}
