using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BL.Models;
using BL.Models.APISimpleTypeReturns;
using BL.Models.Enums;
using BL.Models.ViewModels;
using Serilog;
using Microsoft.Extensions.Options;
using BL.Services;
using TRE_API.Repositories.DbContexts;
using TRE_API.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using BL.Models.Tes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Threading;
using System.Collections.Generic;

namespace OPA.Controllers
{

    [Route("api/[controller]")]
    [ApiController]

    public class OPAController : Controller
    {

        private readonly ApplicationDbContext _DbContext;
        private readonly IDareClientWithoutTokenHelper _dareHelper;
        private readonly OpaService _opaService;
        public OPAController(IDareClientWithoutTokenHelper helper, ApplicationDbContext applicationDbContext, OpaService opaservice)
        {
            _dareHelper = helper;
            _DbContext = applicationDbContext;
            _opaService = opaservice;
        }

    
        [HttpGet("GetAuthorizedProjects")]
        public async Task<IActionResult> AllowAccessToProjects()
        {        
            try
            {
                var today = DateTime.Today;
                var userName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First();
                var treData =  _dareHelper.CallAPIWithoutModel<List<Project>>("/api/Project/GetAllProjectsForTre").Result;
                var user = new
                {
                    user = "PatriciaAkinkuade",
                    today = DateTime.Today
                };      
                bool hasAccess = await _opaService.CheckAccess(userName, today, treData);
                if (hasAccess)
                    if (hasAccess)
                    {
                        // Access allowed  return View();  

                        // }else {Access denied
                        
                        // return RedirectToAction("AccessDenied");
                    }

                Log.Information("{Function} Projects retrieved successfully", "GetAllProjectsForTre");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "GetAllProjectsForTre");
                throw;
            }

        }


    }

}