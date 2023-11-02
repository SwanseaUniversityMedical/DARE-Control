using BL.Models;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TRE_API.Repositories.DbContexts;

namespace TRE_API.Services
{
    public class ControllerHelper
    {
        public static async Task AddTreAuditLog(TreProject? project, TreMembershipDecision? membershipDecision, bool approved, ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor, ClaimsPrincipal loggedInUser)
        {
            var audit = new TreAuditLog()
            {
                Approved = approved,
                IPaddress = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                ApprovedBy = (from x in loggedInUser.Claims where x.Type == "preferred_username" select x.Value).First(),
                Date = DateTime.Now.ToUniversalTime(),
                Project = project,
                MembershipDecision = membershipDecision,
            };
            dbContext.TreAuditLogs.Add(audit);
            await dbContext.SaveChangesAsync();
            Log.Information("{Function}: AuditLogs: ProjectID: {ProjectID}, Membership {Membership}, Approved: {Approved}, ApprovedBy {ApprovedBy}", "AddTreAuditLog", project == null ? "[null]": project.Id, membershipDecision == null ? "[null]" : membershipDecision.Id, approved, (from x in loggedInUser.Claims where x.Type == "preferred_username" select x.Value).First());
        }
    }
}
