using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Tre_Credentials.DbContexts
{
    public class CredentialsDbContextFactory : IDesignTimeDbContextFactory<CredentialsDbContext>
    {
        public CredentialsDbContext CreateDbContext(string[] args)
        {
            
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<CredentialsDbContext>();
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("CredentialsConnection"));

            return new CredentialsDbContext(optionsBuilder.Options);
        }
    }
}