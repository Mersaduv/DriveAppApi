using Microsoft.EntityFrameworkCore;

namespace DriveApp.Data;

/// <summary>
/// Helper class for managing database migrations and seeding.
/// </summary>
public static class DatabaseMigrator
{
    /// <summary>
    /// Apply pending migrations and seed initial data if needed.
    /// </summary>
    public static async Task MigrateAndSeedAsync(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = loggerFactory.CreateLogger<DbInitializer>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            logger.LogInformation("Checking for pending migrations...");
            
            // Get pending migrations
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            var pendingMigrationCount = pendingMigrations.Count();
            
            if (pendingMigrationCount > 0)
            {
                logger.LogInformation("Found {count} pending migrations. Applying...", pendingMigrationCount);
                await context.Database.MigrateAsync();
                logger.LogInformation("Successfully applied {count} migrations", pendingMigrationCount);
            }
            else
            {
                logger.LogInformation("No pending migrations found");
            }
            
            // Seed initial data if needed
            await DbInitializer.Initialize(serviceProvider, loggerFactory);
            
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database");
            throw;
        }
    }
} 