
using BL.Models;
using Microsoft.EntityFrameworkCore;

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
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<TreUser> Users { get; set; }
        public DbSet<TreProject> Projects { get; set; }
        public DbSet<TreMembershipDecision> MembershipDecisions { get; set; }
        
        public DbSet<KeycloakCredentials> KeycloakCredentials { get; set; }
        
        public DbSet<TreAuditLog> TreAuditLogs { get; set; }

        public DbSet<TESKstatus> TESK_Status { get; set; }
        public DbSet<TeskAudit> TESK_Audit { get; set; }


    }
}
