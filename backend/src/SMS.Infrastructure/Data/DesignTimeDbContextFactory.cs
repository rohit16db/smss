using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SMS.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        // Hardcode for a moment just to get the migration added if config is failing
        optionsBuilder.UseNpgsql("Host=localhost;Database=sms_db;Username=postgres;Password=Pass@123");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
