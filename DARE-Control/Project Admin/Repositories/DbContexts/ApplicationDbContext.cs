using Project_Admin.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace Project_Admin.Repositories.DbContexts
{
    public class ApplicationDbContext
    {
        public DbSet<User> Users { get; set; }
    }
}
