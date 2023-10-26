using BL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TREAgent.Repositories.DbContexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("agent");
        }

        public DbSet<TESKstatus> TESK_Status { get; set; }
        public DbSet<TeskAudit> TESK_Audit { get; set; }

        public DbSet<TokenToExpire> TokensToExpire { get; set; }

        public DbSet<GeneratedRole> GeneratedRole { get; set; }


    }
}
