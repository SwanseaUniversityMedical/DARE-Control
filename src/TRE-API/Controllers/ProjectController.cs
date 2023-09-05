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

namespace TRE_API.Controllers
{

    [Authorize(Roles = "dare-tre-admin")]
    [Route("api/[controller]")]
    [ApiController]

    public class ProjectController : Controller
    {

        private readonly ApplicationDbContext _DbContext;


        public IDareSyncHelper _dareSyncHelper { get; set; }

        public ProjectController(IDareSyncHelper dareSyncHelper, ApplicationDbContext applicationDbContext)
        {
            _dareSyncHelper = dareSyncHelper;
            _DbContext = applicationDbContext;

        }
        
        [HttpGet("GetMemberships")]
        public List<TreMembershipDecision> GetMemberships(int projectId, bool showOnlyUnprocessed)
        {
            return _DbContext.MembershipDecisions.Where(x =>
                (projectId <= 0 || x.Project.Id == projectId) &&
                (!showOnlyUnprocessed || x.Decision == Decision.Undecided)).ToList();
        }

        [HttpGet("GetAllTreProjects")]
        public List<TreProject> GetAllTreProjects(bool showOnlyUnprocessed)
        {
            return _DbContext.Projects.Where(x => !showOnlyUnprocessed || x.Decision == Decision.Undecided).ToList();
        }

        [HttpGet("GetTreProject")]
        public TreProject GetTreProject(int projectId)
        {
            return _DbContext.Projects.First(x => x.Id == projectId);
        }

        [HttpGet("GetAllActiveTreProjects")]
        public List<TreProject> GetAllActiveTreProjects()
        {
            return _DbContext.Projects.Where(x => !x.Archived).ToList();
        }

        

        [HttpGet("GetAllTreUsers")]
        public List<TreUser> GetAllTreUsers()
        {
            return _DbContext.Users.ToList();
        }

        [HttpGet("GetAllActiveTreUsers")]
        public List<TreUser> GetAllActiveTreUsers()
        {
            return _DbContext.Users.Where(x => !x.Archived).ToList();
        }

        [HttpGet("GetAllMembershipDecisions")]
        public List<TreMembershipDecision> GetAllMembershipDecisions()
        {
            return _DbContext.MembershipDecisions.ToList();
        }

        [HttpGet("GetAllActiveMembershipDecisions")]
        public List<TreMembershipDecision> GetAllActiveMembershipDecisions()
        {
            return _DbContext.MembershipDecisions.Where(x => !x.Archived).ToList();
        }

        [HttpGet("GetAllUndecidedMembershipDecisions")]
        public List<TreMembershipDecision> GetAllUndecidedActiveMembershipDecisions()
        {
            return _DbContext.MembershipDecisions.Where(x => !x.Archived && x.Decision  == Decision.Undecided)
                .ToList();
        }

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
                if (dbproj.Decision == Decision.Undecided)
                {
                    dbproj.Decision = treProject.Decision;
                    dbproj.ApprovedBy = approvedBy;
                    dbproj.LastDecisionDate = approvedDate;
                }
                resultList.Add(dbproj);

            }

            await _DbContext.SaveChangesAsync();
            return resultList;

        }

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
                if (membershipDecision.Decision != Decision.Undecided)
                {
                    dbMembership.Decision = membershipDecision.Decision;
                    dbMembership.ApprovedBy = approvedBy;
                    dbMembership.LastDecisionDate = approvedDate;
                }
                
                returnResult.Add(dbMembership);

            }

            await _DbContext.SaveChangesAsync();
            return returnResult; 

        }

        [HttpGet("SyncSubmissionWithTre")]
        public async Task<BoolReturn> SyncSubmissionWithTre()
        {
            return await _dareSyncHelper.SyncSubmissionWithTre();
            

        }





    }
}
