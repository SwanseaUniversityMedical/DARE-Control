using BL.Models;
using BL.Models.DTO;
using BL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DARE_FrontEnd.Controllers
{
    public class UserController : Controller
    {
        private readonly IDareClientHelper _clientHelper;
        public UserController(IDareClientHelper client)
        {
            _clientHelper = client;
        }
        public IActionResult AddUserForm()
        {
            return View(new FormData()
            {
                FormIoUrl = "https://bthbspqizezypsb.form.io/dareuser/dareuserregistration"
            });
        }

        [HttpGet]
        public IActionResult GetAllUsers()
        {

            var test = _clientHelper.CallAPIWithoutModel<List<User>>("/api/User/GetAllUsers/").Result;

            return View(test);
        }

        [HttpPost]
        public async Task<IActionResult> UserFormSubmission([FromBody] FormData submissionData)
        {

            var result = await _clientHelper.CallAPI<FormData, User>("/api/User/AddUser", submissionData);
            
            return Ok(result);
        }

        
    }
}
