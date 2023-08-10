
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
     
        public DbSet<ProjectApproval> ProjectApprovals { get; set; }
        public DbSet<ControlCredentials> ControlCredentials { get; set; }


    }
}
