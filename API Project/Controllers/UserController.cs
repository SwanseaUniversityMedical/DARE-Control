using API_Project.Repositories.DbContexts;
using API_Project.Services.Project;
using Microsoft.AspNetCore.Mvc;
using Project_Admin.Models;

namespace API_Project.Controllers
{
    public class UserController : Controller
    {

        private readonly IUserHandler _userHandler;
        private readonly ApplicationDbContext _DbContext;
       // private readonly IUserHandler _UserService;
        public IActionResult Index()
        {
            return View();
        }

        public async Task<User> CreateUser(User User)
        {

            //chek and update job if needed 

            var existingUser = _DbContext.Projects.Find(User.Id);

            if (existingUser == null)
            {
                var id = 500;
                var newUser = await _userHandler.CreateUser(User);
            }

            await _userHandler.CreateUser(User);
            _DbContext.SaveChangesAsync();

            return User;
        }

    }
}
