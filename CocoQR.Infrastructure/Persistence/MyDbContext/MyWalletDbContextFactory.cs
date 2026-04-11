using CocoQR.Domain.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CocoQR.Infrastructure.Persistence.MyDbContext
{
    public class CocoQRDbContextFactory : IDesignTimeDbContextFactory<CocoQRDbContext>
    {
        public CocoQRDbContext CreateDbContext(string[] args)
        {
            // Try to find the API project from different possible locations
            var currentDirectory = Directory.GetCurrentDirectory();
            var apiProjectPath = Path.Combine(currentDirectory, "../CocoQR.API");

            // Fallback: if running from solution root
            if (!Directory.Exists(apiProjectPath))
            {
                apiProjectPath = Path.Combine(currentDirectory, "CocoQR.API");
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(apiProjectPath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString(Database.DefaultConnection);

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException(
                    $"{string.Format(ErrorMessages.ConnectionStringNotFound, Database.DefaultConnection)} Searched in: {apiProjectPath}",
                    Database.DefaultConnection);
            }

            var optionsBuilder = new DbContextOptionsBuilder<CocoQRDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new CocoQRDbContext(optionsBuilder.Options);
        }
    }
}
