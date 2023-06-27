using BL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Npgsql.EntityFrameworkCore.PostgreSQL;


namespace BL.Repositories.DbContexts
{
    public class ApplicationDbContext : DbContext
    {
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseNpgsql("DefaultConnection")
        //    .UseUtcDateTime();
        //}
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
          : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Projects> Projects { get; set; }
        
        
        public DbSet<Endpoints> Endpoints { get; set; }

        public DbSet<FormData> FormData { get; set; }

        public DbSet<Submission> Submissions { get; set; }



    }
}
