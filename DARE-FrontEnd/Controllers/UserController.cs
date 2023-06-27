using BL.Models;
using BL.Models.DTO;
using DARE_FrontEnd.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace DARE_FrontEnd.Controllers
{
    public class UserController : Controller
    {
        private readonly IClientHelper _clientHelper;
        public UserController(IClientHelper client)
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

        [HttpPost]
        public async Task<IActionResult> UserFormSubmission([FromBody] FormData submissionData)
        {

            var result = await _clientHelper.CallAPI<FormData, User>("/api/User/AddUser", submissionData);
            
            return Ok(result);
        }

        
    }
}
