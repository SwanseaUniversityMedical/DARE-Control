using BL.Models;
using BL.Repositories.DbContexts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API_Project.Controllers
{
    public class FormController : Controller
    {

        //private readonly IUserHandler _userHandler;
        private readonly ApplicationDbContext _DbContext;
        // private readonly IUserHandler _UserService;

        public FormController(ApplicationDbContext applicationDbContext)
        {

            _DbContext = applicationDbContext;

        }

        
        [HttpPost("Get_FormData")]

        public async Task<FormData> GetFormDataById()
        {

            var form = new FormData();
            //var theuser =

           
            //membership.Id = 1;
            _DbContext.FormData.Add(form);
            await _DbContext.SaveChangesAsync();

            return form;
        }


        public IActionResult Index()
        {
            return View();
        }
    }
}
