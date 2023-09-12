using DARE_API.Repositories.DbContexts;
using Microsoft.AspNetCore.Mvc;
using BL.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using BL.Models.ViewModels;
using DARE_API.Services.Contract;
using Microsoft.AspNetCore.Authentication;
using DARE_API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore.Infrastructure;
using BL.Services;
using Microsoft.AspNetCore.Http;
using BL.Models.Tes;

namespace DARE_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "dare-control-admin")]
    public class AuditController : Controller
    {
        private readonly ApplicationDbContext _DbContext;
        protected readonly IHttpContextAccessor _httpContextAccessor;
        public AuditController(ApplicationDbContext applicationDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _DbContext = applicationDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("SaveAuditLogs")]
        public async Task<AuditLog?> SaveAuditLogs(FormData? data,string? projectId,string? userId,string? treId,string? testaskId)
        {
            try
            {
                var audit = new AuditLog()
                {
                    FormData = data.FormIoString,
                    IPaddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                    UserName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First(),
                    ProjectId = Convert.ToInt32(projectId),
                    UserId = Convert.ToInt32(userId),
                    TreId = Convert.ToInt32(treId),
                    TestaskId = Convert.ToInt32(testaskId),
                    Date = DateTime.Now.ToUniversalTime()
                };
                _DbContext.AuditLogs.Add(audit);
                await _DbContext.SaveChangesAsync();

                Log.Information("{Function}:", "SaveAuditLogs", data.FormIoString, "ProjectId:" + projectId +" TreId:"+ treId +" UserId:"+ userId + " TestaskId:" + testaskId + @User?.FindFirst("name")?.Value
                );
                return audit;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "SaveAuditLogs");
                throw;
            }

        }

    }
}
