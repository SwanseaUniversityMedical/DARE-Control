using Microsoft.EntityFrameworkCore;


namespace TRE_TESK.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): base(options)
        {

        }

        public DbSet<RoleData> DataToRoles { get; set; }
        public DbSet<GeneratedRole> GeneratedRole { get; set; }
        
    }
}
