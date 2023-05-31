using BL.Repositories.DbContexts;

using Microsoft.AspNetCore.Mvc;
using BL.Models;


namespace BL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {

        
        private readonly ApplicationDbContext _DbContext;
        

        public UserController(ApplicationDbContext applicationDbContext)
        {

            _DbContext = applicationDbContext;



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
            
            return returned;
        }
        

    }
}
