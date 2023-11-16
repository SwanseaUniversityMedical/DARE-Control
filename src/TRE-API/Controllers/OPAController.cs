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
using System.Diagnostics.Eventing.Reader;

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


        [HttpGet("CheckUserAccess")]
        public async Task<bool> CheckUserAccess()
        {
            try
            {
                //var userName = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First();
                var userName = "PatriciaAkinkuade";
                var treData = _DbContext.Projects.Where(x=> x.Decision == Decision.Undecided)
                      .ToList();
                DateTime today = DateTime.Today;
               
                bool hasAccess = await _opaService.CheckAccess(userName, today, treData);
                if (hasAccess)
                {
                    Log.Information("{Function} User Access Allowed", "CheckUserAccess");
                    return true;
                }
                else
                {
                    Log.Information("{Function} User Access Denied", "CheckUserAccess");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crashed", "CheckUserAccess");
                return false;
                throw;
            }

        }


    }

}