using DARE_API.Repositories.DbContexts;
using Microsoft.AspNetCore.Mvc;
using BL.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using BL.Models.ViewModels;
using DARE_API.Services.Contract;
using Microsoft.AspNetCore.Authentication;

namespace DARE_API.Controllers
{
    public class AuditController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
