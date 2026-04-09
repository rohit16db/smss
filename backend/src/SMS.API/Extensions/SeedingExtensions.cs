using Microsoft.EntityFrameworkCore;
using SMS.Infrastructure.Data;
using SMS.Infrastructure.Seeders;

namespace SMS.API.Extensions
{
    public static class SeedingExtensions
    {
        /// <summary>
        /// Apply pending database migrations (runs on all environments)
        /// </summary>
        public static async Task MigrateDatabaseAsync(this WebApplication app)
        {
            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    Console.WriteLine("[Database] Applying pending migrations...");
                    await context.Database.MigrateAsync();
                    Console.WriteLine("[Database] ✓ Migrations applied successfully");
                }
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogWarning(ex, "[Database] ✗ Error applying migrations during startup: {Message}. App will continue.", ex.Message);
                if (ex.InnerException != null)
                {
                    logger.LogWarning(ex.InnerException, "[Database] ✗ Inner exception: {Message}", ex.InnerException.Message);
                }
                // Don't throw here to allow DesignTime tools to work even if DB is not reachable
            }
        }

        /// <summary>
        /// Seed the database with initial data (development only)
        /// </summary>
        public static async Task SeedDatabaseAsync(this WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                return;
            }

            try
            {
                using (var scope = app.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    await DatabaseSeeder.SeedAsync(context);
                }
            }
            catch (Exception ex)
            {
                var logger = app.Services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "[Database] ✗ Error seeding database: {Message}", ex.Message);
                if (ex.InnerException != null)
                {
                    logger.LogError(ex.InnerException, "[Database] ✗ Inner exception: {Message}", ex.InnerException.Message);
                }
                // Don't rethrow for seeding - app can continue, but log the error
                Console.WriteLine($"[Database] Seeding Error: {ex}");
            }
        }
    }
}
