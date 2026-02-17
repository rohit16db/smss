using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Domain.Interfaces;

namespace SMS.Infrastructure.Services;

/// <summary>
/// Generates unique student IDs with format: STU-NNNNNN
/// Example: STU-000001, STU-000002, etc.
/// </summary>
public class StudentIdGenerator : IStudentIdGenerator
{
    private readonly IApplicationDbContext _context;
    private const string PREFIX = "STU";

    public StudentIdGenerator(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateStudentIdAsync(CancellationToken cancellationToken = default)
    {
        // Get the latest enrollment number starting with PREFIX
        var latestStudent = await _context.Students
            .Where(s => s.EnrollmentNumber.StartsWith($"{PREFIX}-"))
            .OrderByDescending(s => s.EnrollmentNumber)
            .FirstOrDefaultAsync(cancellationToken);

        int nextNumber = 1;

        if (latestStudent != null)
        {
            // Extract the number from the enrollment number
            // Format could be STU-000001 or legacy STU-YYYY-NNNNNN
            var parts = latestStudent.EnrollmentNumber.Split('-');
            // Take the last part which should be the numeric sequence
            if (parts.Length >= 2 && int.TryParse(parts[^1], out var lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }

        // Format: STU-NNNNNN (6 digits with leading zeros)
        return $"{PREFIX}-{nextNumber:D6}";
    }
}
