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
        

        [HttpPost("Add_User")]

        public async Task<User> AddUser([FromBody] User Users)
        {
            _DbContext.Users.Add(Users);
            await _DbContext.SaveChangesAsync();

            return Users;
        }


        [HttpPost("AddUser")]
        public IActionResult AddUser([FromBody] JsonObject submissionData)
        {
            //save session id against it
            User users = JsonConvert.DeserializeObject<User>(submissionData.ToString());

            var Name = users.Name;
            var Email = users.Email;
            _DbContext.Users.Add(users);
            _DbContext.SaveChangesAsync();

            return Ok();
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
