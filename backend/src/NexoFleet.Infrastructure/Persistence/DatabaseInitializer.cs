using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexoFleet.Infrastructure.Identity;

namespace NexoFleet.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    [SuppressMessage("Usage", "CA1848:Use the LoggerMessage delegates", Justification = "Startup initialization logs")]
    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services,
        IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            if (dbContext.Database.IsRelational())
            {
                logger.LogInformation("Applying EF Core database migrations...");
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("EF Core database migrations applied successfully.");
            }

            logger.LogInformation("Seeding identity roles and bootstrap admin...");
            await services.SeedIdentityAsync(configuration);
            logger.LogInformation("Database initialization completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }
}
