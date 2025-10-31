﻿using BL.Models;
using BL.Models.ViewModels;
using BL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BL.Models.Settings;
using Microsoft.CodeAnalysis;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace DARE_FrontEnd.Controllers
{
    [Authorize(Roles = "dare-control-admin")]
    public class TreController : Controller
    {
        private readonly IDareClientHelper _clientHelper;
        
        private readonly FormIOSettings _formIOSettings;
     
        public TreController(IDareClientHelper client, FormIOSettings formIo)
        {
            _clientHelper = client;

            _formIOSettings = formIo;

        }

        [HttpGet]
        
        public IActionResult SaveATre(int treId)
        {
            if (!ModelState.IsValid) // SonarQube security
            {
                return View("/");
            }

            var formData = new FormData()
            {
                
                FormIoUrl = _formIOSettings.TreForm,
                FormIoString = @"{""id"":0}",
                Id = treId
            };

            if (treId > 0)
            {
                var paramList = new Dictionary<string, string>();
                paramList.Add("treId", treId.ToString());
                var tre = _clientHelper.CallAPIWithoutModel<Tre>("/api/Tre/GetATre/", paramList).Result;
                formData.FormIoString = tre?.FormData;
                formData.FormIoString = formData.FormIoString?.Replace(@"""id"":0", @"""Id"":" + treId.ToString(), StringComparison.CurrentCultureIgnoreCase);
            }

            return View(formData);
        }

     

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetAllTres()
        {

            var tres = _clientHelper.CallAPIWithoutModel<List<Tre>>("/api/Tre/GetAllTres/").Result;

            return View(tres);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetATre(int id)
        {
            if (!ModelState.IsValid) // SonarQube security
            {
                return View("/");
            }

            var paramlist = new Dictionary<string, string>();
            paramlist.Add("treId", id.ToString());
            var Tre = _clientHelper.CallAPIWithoutModel<Tre?>(
                "/api/Tre/GetATre/", paramlist).Result;

            return View(Tre);
        }

     
        [HttpPost]
        public async Task<IActionResult> TreFormSubmission([FromBody] object arg, int id)
        {
            if (!ModelState.IsValid) // SonarQube security
            {
                return View("/");
            }

            var str = arg?.ToString();

            if (!string.IsNullOrEmpty(str))
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<FormData>(str);
                data.FormIoString = str;

                
                var json = System.Text.Json.JsonSerializer.Serialize(data);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
               
                var response = await _clientHelper.CallAPI("/api/Tre/SaveTre", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var tre = JsonConvert.DeserializeObject<Tre>(result);
                    TempData["success"] = "Tre saved Successfully";
                    return Ok(tre);
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return BadRequest(errorMessage);
                }
            }
            return BadRequest("Invalid data");
        }
    }
}
