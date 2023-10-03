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
        public DbSet<SubmissionCredentials> SubmissionCredentials { get; set; }
        public DbSet<DataFiles> DataEgressFiles{ get; set; }

    }
}
