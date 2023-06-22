using BL.Repositories.DbContexts;
using Microsoft.AspNetCore.Mvc;
using BL.Models;
using BL.DTO;
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
using DARE_API.Controllers;

namespace BL.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        //private readonly ILogger<UserController> _logger;
        private readonly IConfiguration configuration;
        private readonly ApplicationDbContext _DbContext;

        public UserController(ApplicationDbContext applicationDbContext, IConfiguration configuration)
        {

            _DbContext = applicationDbContext;
            this.configuration = configuration;
        }
        private readonly ILogger<UserController> _logger;
        public class data
        {
            public string? FormIoString { get; set; }

        }

        [HttpPost("AddUser")]
        public async Task<User> AddUser(FormIoData data) 
        {
            try
            {
                FormData formData = new FormData()
                {
                    FormIoString = data.FormIoString,
                    FormIoUrl = data.FormIoUrl
                };

                User users = JsonConvert.DeserializeObject<User>(data.FormIoString);

                users.FormData = formData;

                _DbContext.Users.Add(users);

                await _DbContext.SaveChangesAsync();

                return users;
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        public class ContainString
        {
            public string Data { get; set; }
        }

        [HttpGet("Get_User/{userId}")]

        public User GetUser(int userId)
        {
            try
            {
                var returned = _DbContext.Users.Find(userId);
                if (returned == null)
                {
                    return null;
                }
                _logger.LogInformation("User retrieved successfully");
                return returned;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
            }

            return null;
        }


        [HttpGet("Get_AllUsers")]

        public List<User> GetAllUsers()
        {
            try
            {
                var allUsers = _DbContext.Users.ToList();

                foreach (var user in allUsers)
                {
                    var id = user.Id;
                    var name = user.Name;
                }
                //return returned.FirstOrDefault();
                _logger.LogInformation("Users retrieved successfully");
                return allUsers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
            }

            return null;
        }
    }
}
