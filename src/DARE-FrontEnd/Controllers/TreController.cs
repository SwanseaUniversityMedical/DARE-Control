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
        protected readonly IHttpContextAccessor _httpContextAccessor;
        public TreController(IDareClientHelper client, FormIOSettings formIo, IHttpContextAccessor httpContextAccessor)
        {
            _clientHelper = client;

            _formIOSettings = formIo;

            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        
        public IActionResult SaveATre(int treId)
        {
            var formData = new FormData()
            {
                
                FormIoUrl = _formIOSettings.TreForm,
                FormIoString = @"{""id"":0}"
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
                var audit = new AuditLog()
                {
                    FormData = data.FormIoString,
                    IPaddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                    UserName = @User?.FindFirst("name")?.Value,
                    Module = "Tres",
                    AuditValues = "Added Tre/" + " " + result.Id.ToString() + " " + result.ErrorMessage,
                    Action = "TreFormSubmission",
                    Date = DateTime.Now.ToUniversalTime()
                };
                var log = await _clientHelper.CallAPI<AuditLog, AuditLog?>("/api/Audit/SaveAuditLogs", audit);

                if (result.Error)
                    return BadRequest();

                return Ok(result);
            }
            return BadRequest();
        }



   

    }
}
