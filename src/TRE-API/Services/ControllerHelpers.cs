using BL.Models;
using BL.Models.APISimpleTypeReturns;
using BL.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;
using Sentry.Protocol;
using TRE_API.Repositories.DbContexts;

namespace TRE_API.Services
{
    public class ControllerHelpers
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

        public static async Task<BoolReturn> CheckCredentialsAreValid(KeycloakTokenHelper keycloakTokenHelper, IEncDecHelper encDecHelper, ApplicationDbContext dbContext, CredentialType type)
        {
            try
            {
                var result = new BoolReturn() { Result = false };
                var creds = dbContext.KeycloakCredentials.FirstOrDefault(x =>
                    x.CredentialType == type);
                if (creds != null)
                {
                    var token = await keycloakTokenHelper.GetTokenForUser(creds.UserName,
                        encDecHelper.Decrypt(creds.PasswordEnc), "dare-tre-admin");
                    result.Result = !string.IsNullOrWhiteSpace(token);
                }

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "CheckCredentialsAreValid");
                throw;
            }
        }

        public static async Task<KeycloakCredentials> UpdateCredentials(KeycloakCredentials creds, KeycloakTokenHelper keycloakTokenHelper,
            ApplicationDbContext DbContext, IEncDecHelper encDecHelper, CredentialType type, string requiredrole)
        {
            try
            {
                creds.Valid = true;
                var token = await keycloakTokenHelper.GetTokenForUser(creds.UserName,
                    creds.PasswordEnc, requiredrole);
                if (string.IsNullOrWhiteSpace(token))
                {
                    creds.Valid = false;
                    return creds;
                }

                var add = true;
                var dbcred = DbContext.KeycloakCredentials.FirstOrDefault(x => x.CredentialType == CredentialType.Tre);
                if (dbcred != null)
                {
                    creds.Id = dbcred.Id;
                    creds.CredentialType = type;
                    add = false;
                }

                creds.PasswordEnc = encDecHelper.Encrypt(creds.PasswordEnc);
                if (add)
                {
                    DbContext.KeycloakCredentials.Add(creds);

                }
                else
                {
                    DbContext.KeycloakCredentials.Update(creds);
                }

                await DbContext.SaveChangesAsync();

                Log.Information("{Function} Credentials Successfully update", "UpdateCredentials");
                return creds;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "{Function} Crash", "UpdateCredentials");
                throw;
            }
        }
    }
}
