using BL.Models;
using Microsoft.EntityFrameworkCore;

namespace Data_Egress_API.Repositories.DbContexts
{
    public class ApplicationDbContext : DbContext
    {
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseNpgsql("DefaultConnection")
        //    //.UseUtcDateTime();
        //}
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options)
        {

        }
        public DbSet<KeycloakCredentials> TreCredentials { get; set; }
        public DbSet<EgressSubmission> EgressSubmissions { get; set; }
        public DbSet<EgressFile> EgressFiles { get; set; }


    }
}
