﻿using BL.Models;
using BL.Models.DTO;
using BL.Models.Settings;
using BL.Services;
using EasyNetQ.Management.Client.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Serilog;
using System.Data;
using User = EasyNetQ.Management.Client.Model.User;

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

        public IActionResult AddUserForm(int userId)
        {
            var formData = new FormData()
            {
                FormIoUrl = _formIOSettings.UserForm,
                FormIoString = @"{""id"":0}"
            };
            
            if (userId > 0)
            {
                var paramList = new Dictionary<string, string>();
                paramList.Add("userId", userId.ToString());
                var user = _clientHelper.CallAPIWithoutModel<BL.Models.User?>("/api/User/GetUser/", paramList).Result;
                formData.FormIoString = user?.FormData;
                formData.FormIoString = formData.FormIoString?.Replace(@"""id"":0", @"""id"":"+userId.ToString());
            }

            return View(formData);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAllUsers()
        {

            var test = _clientHelper.CallAPIWithoutModel<List<BL.Models.User>>("/api/User/GetAllUsers/").Result;

            return View(test);
        }

       

        [AllowAnonymous]
        public IActionResult GetUser(int id)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("userId", id.ToString());
            var test = _clientHelper.CallAPIWithoutModel<BL.Models.User?>(
                "/api/User/GetUser/", paramlist).Result;

            return View(test);
        }

        [HttpPost]
        public async Task<IActionResult> UserEditFormSubmission([FromBody] object arg, int id)
        {
            var str = arg?.ToString();

            if (!string.IsNullOrEmpty(str))
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<FormData>(str);
                data.FormIoString = str;

                var result = await _clientHelper.CallAPI<FormData, BL.Models.User>("/api/User/AddUser", data);

                if (result.Id == 0)
                    return BadRequest();

                return Ok(result);
            }
            return BadRequest();
        }

        //[HttpGet]
        //public async Task<IActionResult> EditUser(int? userId)
        //{
           
        //    var paramList = new Dictionary<string, string>();
        //    paramList.Add("userId", userId.ToString());
        //    var user = _clientHelper.CallAPIWithoutModel<BL.Models.User?>("/api/User/GetUser/", paramList).Result;
        //    var userView = new User()
        //    {
        //        Id = user.Id,
        //        FormData = user.FormData,
        //        Name = user.Name,
        //        Email = user.Email
        //    };

        //    return View(userView);

        //}
    }
}
