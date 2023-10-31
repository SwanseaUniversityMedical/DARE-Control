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


namespace OPA.Controllers
{

    [Route("api/[controller]")]
    [ApiController]

    public class OPAController : Controller
    {

        private readonly ApplicationDbContext _DbContext;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public IDareSyncHelper _dareSyncHelper { get; set; }

        public OPAController(IDareSyncHelper dareSyncHelper, ApplicationDbContext applicationDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dareSyncHelper = dareSyncHelper;
            _DbContext = applicationDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        //[Authorize(Roles = "dare-tre-admin")]
        //[HttpGet("GetOPAUser")]
        //public List<TreMembershipDecision> GetOPAUser(string Username)
        //{
        //    try
        //    {
        //    //    return _DbContext.MembershipDecisions.Where(x =>
        //    //        (projectId <= 0 || x.Project.Id == projectId) &&
        //    //        (!showOnlyUnprocessed || x.Decision == Decision.Undecided)).ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "{Function} Crash", "GetOPAUser");
        //        throw;
        //    }
        //}
        [HttpGet("GetOPAUser")]
        // GET /bundles/{bundleId}
        public List<TreMembershipDecision> GetOPAUser(string Username)
        {
            // Retrieve the policy bundle based on the bundleId
            // You can use a database query or any other mechanism to fetch the data
            var today = DateTime.Today;
            var userDetails = FetchPolicyBundleFromDataSource(Username); 
        // If the bundle does not exist, return a 404 Not Found response
         if (userDetails == null) { return null;
        } 
        // Format the bundle data as desired (e.g., convert to JSON)
        var formattedBundle = FormatPolicyBundle(userDetails);
        // Return the formatted bundle as the API response
         return Json(formattedBundle, JsonRequestBehavior.AllowGet);
         } 
        // Helper methods
        private OPAUser FetchPolicyBundleFromDataSource(string Username)
        { 
        // Implement the logic to fetch the policy bundle from the data source (e.g., database) 
        // Return the fetched bundle or null if it doesn't exist
        }
        //private object FormatPolicyBundle(PolicyBundle bundle) { 
        //// Implement the logic to format the policy bundle as desired (e.g., convert to JSON) 
        // Return the formatted bundle data
        }
        }

    
}
