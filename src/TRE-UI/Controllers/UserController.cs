using BL.Models;
using BL.Models.DTO;
using BL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;


namespace TRE_UI.Controllers
{
    public class UserController : Controller
    {
        private readonly IDareClientHelper _clientHelper;
        public UserController(IDareClientHelper client)
        {
            _clientHelper = client;
        }
    
        [HttpGet]
        public IActionResult GetAllUsers()
        {

            var users = _clientHelper.CallAPIWithoutModel<List<User>>("/api/User/GetAllUsers/").Result;

            return View(users);
        }
        public IActionResult GetUser(int id)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("userId", id.ToString());
            var test = _clientHelper.CallAPIWithoutModel<User?>(
                "/api/User/GetUser/", paramlist).Result;

            return View(test);
        }
    }
}
