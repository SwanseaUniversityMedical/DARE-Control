using BL.Models;
using BL.Models.ViewModels;
using BL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BL.Models.Settings;
using Microsoft.CodeAnalysis;
using Microsoft.AspNetCore.Http;

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
        public IActionResult GetATre(int id)
        {
            var paramlist = new Dictionary<string, string>();
            paramlist.Add("treId", id.ToString());
            var test = _clientHelper.CallAPIWithoutModel<Tre?>(
                "/api/Tre/GetATre/", paramlist).Result;

            return View(test);
        }


        [HttpPost]
        public async Task<IActionResult> TreFormSubmission([FromBody] object arg, int id)
        {
            var str = arg?.ToString();

            if (!string.IsNullOrEmpty(str))
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<FormData>(str);
                data.FormIoString = str;

                var result = await _clientHelper.CallAPI<FormData, Tre?>("/api/Tre/SaveTre", data);
                         
                if (result.Error)
                    return BadRequest();

                TempData["success"] = "Tre Save Successfully";
                return Ok(result);
            }
            return BadRequest();
        }

    }
}
