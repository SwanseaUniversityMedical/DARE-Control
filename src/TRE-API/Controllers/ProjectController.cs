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


        [HttpGet("GetAllTreProjects")]
        public List<TreProject> GetAllTreProjects()
        {
            return _DbContext.Projects.ToList();
        }

        [HttpGet("GetAllActiveTreProjects")]
        public List<TreProject> GetAllActiveTreProjects()
        {
            return _DbContext.Projects.Where(x => !x.Archived).ToList();
        }

        [HttpGet("GetAllUndecidedTreProjects")]
        public List<TreProject> GetAllUndecidedTreProjects()
        {
            return _DbContext.Projects.Where(x => !x.Archived && string.IsNullOrWhiteSpace(x.ApprovedBy)).ToList();
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
            return _DbContext.MembershipDecisions.Where(x => !x.Archived && string.IsNullOrWhiteSpace(x.ApprovedBy))
                .ToList();
        }

        [HttpPost("UpdateProjectApprovals")]
        public async Task UpdateProjectApprovals(List<TreProject> projects, string approvedBy)
        {
            if (string.IsNullOrWhiteSpace(approvedBy))
            {
                approvedBy = "[Unknown]";
            }

            var approvedDate = DateTime.Now.ToUniversalTime();
            foreach (var treProject in projects)
            {
                var dbproj = _DbContext.Projects.First(x => x.Id == treProject.Id);
                dbproj.Approved = treProject.Approved;
                dbproj.ApprovedBy = approvedBy;
                dbproj.LastDecisionDate = approvedDate;

            }

            await _DbContext.SaveChangesAsync();

        }

        [HttpPost("UpdateMembershipApprovals")]
        public async Task UpdateMembershipApprovals(List<TreMembershipDecision> membershipDecisions, string approvedBy)
        {
            if (string.IsNullOrWhiteSpace(approvedBy))
            {
                approvedBy = "[Unknown]";
            }

            var approvedDate = DateTime.Now.ToUniversalTime();
            foreach (var membershipDecision in membershipDecisions)
            {
                var dbMembership = _DbContext.MembershipDecisions.First(x => x.Id == membershipDecision.Id);
                dbMembership.Approved = membershipDecision.Approved;
                dbMembership.ApprovedBy = approvedBy;
                dbMembership.LastDecisionDate = approvedDate;

            }

            await _DbContext.SaveChangesAsync();

        }

        [HttpGet("SyncSubmissionWithTre")]
        public async Task<BoolReturn> SyncSubmissionWithTre()
        {
            return await _dareSyncHelper.SyncSubmissionWithTre();
            

        }





    }
}
