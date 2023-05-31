using BL.Repositories.DbContexts;
using Microsoft.AspNetCore.Mvc;
using BL.Models;
using System.Text.Json.Nodes;
using Newtonsoft.Json;

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
        
        //[HttpPost("Add_User")]

        //public async Task<User> AddUser([FromBody] User Users)
        //{
        //    _DbContext.Users.Add(Users);
        //    await _DbContext.SaveChangesAsync();

        //    return Users;
        //}


        [HttpPost("AddUser")]
        public async Task<User> AddUser([FromBody] JsonObject submissionData, [FromBody] User User)
        {
            //save session id against it
            //User users = JsonConvert.DeserializeObject<User>(submissionData.ToString());
            User = JsonConvert.DeserializeObject<User>(submissionData.ToString());

            var Name = User.Name;
            var Email = User.Email;
            _DbContext.Users.Add(User);
            _DbContext.SaveChangesAsync();

            return User;
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
