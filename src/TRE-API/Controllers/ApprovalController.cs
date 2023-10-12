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

namespace TRE_API.Controllers
{

    // 
    
    [Route("api/[controller]")]
    [ApiController]

    public class ApprovalController : Controller
    {

        private readonly ApplicationDbContext _DbContext;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        public IDareSyncHelper _dareSyncHelper { get; set; }

        public ApprovalController(IDareSyncHelper dareSyncHelper, ApplicationDbContext applicationDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dareSyncHelper = dareSyncHelper;
            _DbContext = applicationDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        [Authorize(Roles = "dare-tre-admin")]
        [HttpGet("GetMemberships")]
        public List<TreMembershipDecision> GetMemberships(int projectId, bool showOnlyUnprocessed)
        {
            return _DbContext.MembershipDecisions.Where(x =>
                (projectId <= 0 || x.Project.Id == projectId) &&
                (!showOnlyUnprocessed || x.Decision == Decision.Undecided)).ToList();
        }



        [Authorize(Roles = "dare-tre-admin")]
        [HttpGet("GetAllTreProjects")]
        public List<TreProject> GetAllTreProjects(bool showOnlyUnprocessed)
        {
            return _DbContext.Projects.Where(x => !showOnlyUnprocessed || x.Decision == Decision.Undecided).ToList();
        }

        [Authorize(Roles = "dare-tre-admin")]
        [HttpGet("GetTreProject")]
        public TreProject GetTreProject(int projectId)
        {
            return _DbContext.Projects.First(x => x.Id == projectId);
        }

        [Authorize(Roles = "dare-tre-admin")]
        [HttpGet("GetAllActiveTreProjects")]
        public List<TreProject> GetAllActiveTreProjects()
        {
            return _DbContext.Projects.Where(x => !x.Archived).ToList();
        }


        [Authorize(Roles = "dare-tre-admin")]
        [HttpGet("GetAllTreUsers")]
        public List<TreUser> GetAllTreUsers()
        {
            return _DbContext.Users.ToList();
        }

        [Authorize(Roles = "dare-tre-admin")]
        [HttpGet("GetAllActiveTreUsers")]
        public List<TreUser> GetAllActiveTreUsers()
        {
            return _DbContext.Users.Where(x => !x.Archived).ToList();
        }

        [Authorize(Roles = "dare-tre-admin")]
        [HttpGet("GetAllMembershipDecisions")]
        public List<TreMembershipDecision> GetAllMembershipDecisions()
        {
            return _DbContext.MembershipDecisions.ToList();
        }

        [Authorize(Roles = "dare-tre-admin")]
        [HttpGet("GetAllActiveMembershipDecisions")]
        public List<TreMembershipDecision> GetAllActiveMembershipDecisions()
        {
            return _DbContext.MembershipDecisions.Where(x => !x.Archived).ToList();
        }

        [Authorize(Roles = "dare-tre-admin")]
        [HttpGet("GetAllUndecidedMembershipDecisions")]
        public List<TreMembershipDecision> GetAllUndecidedActiveMembershipDecisions()
        {
            return _DbContext.MembershipDecisions.Where(x => !x.Archived && x.Decision  == Decision.Undecided)
                .ToList();
        }

        [Authorize(Roles = "dare-tre-admin")]
        [HttpPost("UpdateProjects")]
        public async Task<List<TreProject>> UpdateProjects(List<TreProject> projects)
        {
            var approvedBy = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First();
            if (string.IsNullOrWhiteSpace(approvedBy))
            {
                approvedBy = "[Unknown]";
            }
            var resultList = new List<TreProject>();
            var approvedDate = DateTime.Now.ToUniversalTime();
            foreach (var treProject in projects)
            {
                var dbproj = _DbContext.Projects.First(x => x.Id == treProject.Id);
                dbproj.LocalProjectName = treProject.LocalProjectName;

                if (treProject.Password == null)
                {
                    dbproj.Password = dbproj.Password;

                }
                else {
                    dbproj.Password = treProject.Password;
                }
                if (treProject.UserName == null)
                {
                    dbproj.UserName = dbproj.UserName;

                }
                else
                {
                    dbproj.UserName = treProject.UserName;
                }

                if (treProject.Decision != dbproj.Decision)
                {
                    dbproj.Decision = treProject.Decision;
                    dbproj.ApprovedBy = approvedBy;
                    dbproj.LastDecisionDate = approvedDate;
                }
                resultList.Add(dbproj);

                var audit = new TreAuditLog()
                {
                    Decision = "TreProject Decision:" + treProject.Decision.ToString(),
                    IPaddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                    ApprovedBy = approvedBy,                 
                    Date = DateTime.Now.ToUniversalTime()
                };
                _DbContext.TreAuditLogs.Add(audit);
                await _DbContext.SaveChangesAsync();
                Log.Information("{Function}:", "AuditLogs", "Treproject Decision:" + treProject.Decision.ToString(), "ApprovedBy:" + approvedBy);
            }
            await _DbContext.SaveChangesAsync();
            return resultList;
        }

        [Authorize(Roles = "dare-tre-admin")]
        [HttpPost("UpdateMembershipDecisions")]
        public async Task<List<TreMembershipDecision>> UpdateMembershipDecisions(List<TreMembershipDecision> membershipDecisions)
        {
            var approvedBy = (from x in User.Claims where x.Type == "preferred_username" select x.Value).First();
            var returnResult = new List<TreMembershipDecision>();
            if (string.IsNullOrWhiteSpace(approvedBy))
            {
                approvedBy = "[Unknown]";
            }

            var approvedDate = DateTime.Now.ToUniversalTime();
            foreach (var membershipDecision in membershipDecisions)
            {
                var dbMembership = _DbContext.MembershipDecisions.First(x => x.Id == membershipDecision.Id);
                if (membershipDecision.Decision != dbMembership.Decision)
                {
                    dbMembership.Decision = membershipDecision.Decision;
                    dbMembership.ApprovedBy = approvedBy;
                    dbMembership.LastDecisionDate = approvedDate;
                }           
                returnResult.Add(dbMembership);

                var audit = new TreAuditLog()
                {
                    Decision = "Membership Decision:" + membershipDecision.Decision.ToString(),
                    IPaddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                    ApprovedBy = approvedBy,
                    Date = DateTime.Now.ToUniversalTime()
                };
                _DbContext.TreAuditLogs.Add(audit);
                await _DbContext.SaveChangesAsync();
                Log.Information("{Function}:", "AuditLogs", "Membership Decision:" + membershipDecision.Decision.ToString(), "ApprovedBy:" + approvedBy);

            }

            await _DbContext.SaveChangesAsync();
            return returnResult; 

        }

        [Authorize(Roles = "dare-tre-admin")]
        [HttpGet("SyncSubmissionWithTre")]
        public async Task<BoolReturn> SyncSubmissionWithTre()
        {
            return await _dareSyncHelper.SyncSubmissionWithTre();
            

        }

    }
}
