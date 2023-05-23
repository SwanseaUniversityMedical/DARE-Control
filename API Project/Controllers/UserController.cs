using BL.Repositories.DbContexts;
using API_Project.Services.Project;
using Microsoft.AspNetCore.Mvc;
using BL.Models;
using Microsoft.EntityFrameworkCore;

namespace API_Project.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
    {

        //private readonly IUserHandler _userHandler;
        private readonly ApplicationDbContext _DbContext;
        // private readonly IUserHandler _UserService;

        public UserController(ApplicationDbContext applicationDbContext)
        {

            _DbContext = applicationDbContext;



        }
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost("Add_User")]

        public async Task<User> AddUser([FromBody] User Users)
        {
            _DbContext.Users.Add(Users);
            await _DbContext.SaveChangesAsync();

            return Users;
        }

        [HttpGet("Get_User/{userId}")]

        public User GetUser(int userId)
        {
            var returned = _DbContext.Users.Find(userId);
            if (returned == null)
            {
                return null;
            }
            //return returned.FirstOrDefault();
            return returned;
        }
        //public async Task<User> CreateUser(User User)
        //{

        //    //chek and update job if needed 

        //    var existingUser = _DbContext.Projects.Find(User.Id);

        //    if (existingUser == null)
        //    {
        //        var id = 500;
        //        var newUser = await _userHandler.CreateUser(User);
        //    }

        //    await _userHandler.CreateUser(User);
        //    _DbContext.SaveChangesAsync();

        //    return User;
        //}

    }
}
