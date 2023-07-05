using BL.Repositories.DbContexts;
using Microsoft.AspNetCore.Mvc;
using BL.Models;
using System.Text.Json.Nodes;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using IdentityModel.Client;
using System.Text;
using BL.Models.DTO;
using DARE_API.Controllers;

namespace BL.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        
        private readonly ApplicationDbContext _DbContext;

        public UserController(ApplicationDbContext applicationDbContext)
        {

            _DbContext = applicationDbContext;
        
        }


        [HttpPost("AddUser")]
        public async Task<User> AddUser(FormData data) 
        {
            try
            {
               
                User users = JsonConvert.DeserializeObject<User>(data.FormIoString);
                users.Name = users.Name.Trim();
                if (_DbContext.Users.Any(x => x.Name.ToLower() == users.Name.ToLower().Trim()))
                {
                    return null;
                }
                users.FormData = data.FormIoString;

                _DbContext.Users.Add(users);

                await _DbContext.SaveChangesAsync();

                return users;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "AddUser");
                var errorUser = new User();
                return errorUser;
                throw;
            }

            
        }



        [HttpGet("GetUser")]
        public User? GetUser(int userId)
        {
            try
            {
                var returned = _DbContext.Users.Find(userId);
                if (returned == null)
                {
                    return null;
                }
                Log.Information("{Function} User retrieved successfully", "GetUser");
                return returned;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetUser");
                throw;
            }

            
        }



        [HttpGet("GetAllUsers")]
        public List<User> GetAllUsers()
        {
            try
            {
                var allUsers = _DbContext.Users.ToList();

                
            
               Log.Information("{Function} Users retrieved successfully", "GetAllUsers");
                return allUsers;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "GetAllUsers");
                throw;
            }

            
        }
    }
}
