using SMS.Infrastructure.Data;
using SMS.Infrastructure.Seeders;

namespace SMS.API.Extensions
{
    public static class SeedingExtensions
    {
        /// <summary>
        /// Seed the database with initial data if running in development environment
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
                logger.LogError(ex, "Error seeding database: {Message}", ex.Message);
                if (ex.InnerException != null)
                {
                    logger.LogError(ex.InnerException, "Inner exception: {Message}", ex.InnerException.Message);
                }
                // Don't rethrow - let the app continue
                Console.WriteLine($"Seeding Error: {ex}");
            }
        }
    }
}
