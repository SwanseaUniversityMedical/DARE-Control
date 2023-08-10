using DARE_API.Repositories.DbContexts;
using Microsoft.AspNetCore.Mvc;
using BL.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using BL.Models.ViewModels;

namespace BL.Controllers
{

    [Authorize]
    //[ApiController]
    [Authorize(Roles = "dare-control-admin,dare-tre-admin")]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        
        private readonly ApplicationDbContext _DbContext;

        public UserController(ApplicationDbContext applicationDbContext)
        {

            _DbContext = applicationDbContext;
        
        }


        [HttpPost("AddUser")]
        public async Task<User> AddUser([FromBody] FormData data) 
        {
            try
            {

                User userData = JsonConvert.DeserializeObject<User>(data.FormIoString);
                userData.Name = userData.Name.Trim();
                userData.Email = userData.Email.Trim();
                if (_DbContext.Users.Any(x => x.Name.ToLower() == userData.Name.ToLower().Trim()))
                {
                    return new User();
                }

                userData.FormData = data.FormIoString;

                if (userData.Id > 0)
                {
                    if (_DbContext.Users.Select(x=>x.Id==userData.Id).Any())
                        _DbContext.Users.Update(userData);
                    else
                        _DbContext.Users.Add(userData);
                }
                else
                    _DbContext.Users.Add(userData);

                await _DbContext.SaveChangesAsync();

                return userData;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "AddUser");
                return new User(); ;
                throw;
            }

            
        }


        [AllowAnonymous]
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


        [AllowAnonymous]
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

        //[AllowAnonymous]
        //[HttpPost("UpdateUser")]
        //public User? UpdateUser([FromBody] FormData data)
        //{
        //    User users = JsonConvert.DeserializeObject<User>(data.FormIoString);
        //    var id = data.Id;
        //    var user = _DbContext.Users.Find(id);
        //    try
        //    {
        //        Log.Information("{Function} User retrieved successfully", "UpdateUser");
        //        if (user != null)
        //        {
        //            user.Name = users.Name;
        //            user.Email = users.Email;
        //            user.FormData = data.FormIoString;
        //            _DbContext.Users.Update(user);
        //            _DbContext.SaveChanges();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "{Function} Crashed", "UpdateUser");
        //        throw;
        //    }
        //    return user;

        //}
    }
}
