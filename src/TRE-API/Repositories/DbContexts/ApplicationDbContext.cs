
using BL.Models;
using Microsoft.EntityFrameworkCore;
using TRE_TESK.Models;
using TREAgent.Repositories;

namespace TRE_API.Repositories.DbContexts
{
    public class ApplicationDbContext : DbContext
    {
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseNpgsql("DefaultConnection");
        //    //.UseUtcDateTime();
        //}
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options)
        {

        }

        public DbSet<TreUser> Users { get; set; }
        public DbSet<TreProject> Projects { get; set; }
        public DbSet<TreMembershipDecision> MembershipDecisions { get; set; }
        
        public DbSet<KeycloakCredentials> KeycloakCredentials { get; set; }
        
        public DbSet<TreAuditLog> TreAuditLogs { get; set; }

        public DbSet<TESKstatus> TESK_Status { get; set; }
        public DbSet<TeskAudit> TESK_Audit { get; set; }
        

        public DbSet<TokenToExpire> TokensToExpire { get; set; }

        public DbSet<GeneratedRole> GeneratedRole { get; set; }

        public DbSet<RoleData> DataToRoles { get; set; }

    }
}
