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
using TRE_API.Controllers;

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


   



       
        [HttpGet("ListAllUsers")]
        public List<User> ListAllUsers()
        {
            try
            {
                var allUsers = _DbContext.Users.ToList();



                Log.Information("{Function} Users retrieved successfully", "ListAllUsers");
                return allUsers;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "ListAllUsers");
                throw;
            }


        }
    }
}
