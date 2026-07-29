using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SCIQUSTICKETS.DATA.Contexts
{
    /// <summary>
    /// Used by EF Core CLI tools (migrations) at design time.
    /// This is NOT used at runtime — Program.cs handles that.
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Walk up from DATA/bin to find the WebAPI appsettings.json
            var basePath = Path.Combine(Directory.GetCurrentDirectory(),
                "..", "SCIQUSTICKETS.WebAPI");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
