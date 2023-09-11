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

namespace DARE_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "dare-control-admin")]
    public class AuditController : Controller
    {
        private readonly ApplicationDbContext _DbContext;
        public AuditController(ApplicationDbContext applicationDbContext)
        {

            _DbContext = applicationDbContext;
        }

        [HttpPost("SaveAuditLogs")]
        public async Task<AuditLog?> SaveAuditLogs(AuditLog model)
        {
            try
            {

                _DbContext.AuditLogs.Add(model);
                await _DbContext.SaveChangesAsync();

                Log.Information("{Function} User: "+ model.UserName+" added a Project", "SaveAuditLogs", model.FormData, model.AuditValues);
                return model;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "SaveAuditLogs");
                throw;
            }


        }

    }
}
