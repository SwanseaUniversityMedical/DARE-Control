using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tre_Credentials.Models;

namespace Tre_Credentials.DbContexts
{
    public class CredentialsDbContext : DbContext
    {
        public CredentialsDbContext(DbContextOptions<CredentialsDbContext> options) : base(options)
        {
        }

        public DbSet<EphemeralCredential> EphemeralCredentials { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EphemeralCredential>()
                .HasIndex(m => new { m.SubmissionId, m.ProcessInstanceKey })
                .IsUnique(false);           
        }
    }
}
