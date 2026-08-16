using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SCIQUSTICKETS.DATA.Contexts
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var connectionString = "Server=localhost;Database=SCIQUS_TICKETS_DB;User=root;Password=root;";
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 32));
            optionsBuilder.UseMySql(connectionString, serverVersion);
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
