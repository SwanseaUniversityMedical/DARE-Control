using BL.Models;
using BL.Models.DTO;
using BL.Models.Settings;
using BL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Data;

namespace DARE_FrontEnd.Controllers
{
    [Authorize(Roles = "dare-control-admin")]
    public class UserController : Controller
    {
        private readonly IDareClientHelper _clientHelper;
        private readonly IConfiguration _configuration;
        private readonly IFormIOSettings _formIOSettings;

        public UserController(IDareClientHelper client, IConfiguration configuration)
        {
            _clientHelper = client;
            _configuration = configuration;
            _formIOSettings = new FormIOSettings();
            configuration.Bind(nameof(FormIOSettings), _formIOSettings);
        }

        public IActionResult AddUserForm()
        {
            return View(new FormData() { FormIoUrl = _formIOSettings.UserForm });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAllUsers()
        {

            var test = _clientHelper.CallAPIWithoutModel<List<User>>("/api/User/GetAllUsers/").Result;

            return View(test);
        }

        [HttpPost]
        public async Task<IActionResult> UserFormSubmission([FromBody] FormData submissionData)
        {

            var result = await _clientHelper.CallAPI<FormData, User>("/api/User/AddUser", submissionData);

            if (result.Id == 0)
            {
                return BadRequest();

            }
            return Ok(result);
        }

        [AllowAnonymous]
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
