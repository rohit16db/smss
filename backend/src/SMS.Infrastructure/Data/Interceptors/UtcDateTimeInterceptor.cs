using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace SMS.Infrastructure.Data.Interceptors;

/// <summary>
/// Interceptor to ensure all DateTime values are converted to UTC before saving
/// Prevents: "Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone'"
/// </summary>
public class UtcDateTimeInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ConvertDateTimesToUtc(eventData.Context);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        ConvertDateTimesToUtc(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private static void ConvertDateTimesToUtc(DbContext? context)
    {
        if (context == null)
            return;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            foreach (var property in entry.Properties)
            {
                // Check if property is a DateTime value
                if (property.CurrentValue is DateTime dateTime)
                {
                    // If the DateTime is Unspecified, convert it to UTC
                    if (dateTime.Kind == DateTimeKind.Unspecified)
                    {
                        property.CurrentValue = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                    }
                    // If it's Local time, convert to UTC
                    else if (dateTime.Kind == DateTimeKind.Local)
                    {
                        property.CurrentValue = dateTime.ToUniversalTime();
                    }
                }
                // Check original values as well
                else if (property.OriginalValue is DateTime originalDateTime)
                {
                    if (originalDateTime.Kind == DateTimeKind.Unspecified)
                    {
                        property.OriginalValue = DateTime.SpecifyKind(originalDateTime, DateTimeKind.Utc);
                    }
                    else if (originalDateTime.Kind == DateTimeKind.Local)
                    {
                        property.OriginalValue = originalDateTime.ToUniversalTime();
                    }
                }
            }
        }
    }
}
