using Microsoft.EntityFrameworkCore;
using SMS.Domain.Entities;
using SMS.Infrastructure.Data;

namespace SMS.Infrastructure.Repositories;

/// <summary>
/// Repository for GradeConfiguration entity operations
/// Single Responsibility: Handle grade scale configuration management
/// </summary>
public interface IGradeConfigurationRepository
{
    Task<List<GradeConfiguration>> GetBySchoolAsync(Guid schoolId, CancellationToken cancellationToken = default);
    Task<GradeConfiguration?> GetByGradeNameAsync(Guid schoolId, string gradeName, CancellationToken cancellationToken = default);
    Task<bool> AddAsync(GradeConfiguration configuration, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(GradeConfiguration configuration, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class GradeConfigurationRepository : IGradeConfigurationRepository
{
    private readonly ApplicationDbContext _context;

    public GradeConfigurationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GradeConfiguration>> GetBySchoolAsync(Guid schoolId, CancellationToken cancellationToken = default)
    {
        return await _context.GradeConfigurations
            .Where(g => g.SchoolId == schoolId)
            .OrderBy(g => g.MinPercentage)
            .ToListAsync(cancellationToken);
    }

    public async Task<GradeConfiguration?> GetByGradeNameAsync(Guid schoolId, string gradeName, CancellationToken cancellationToken = default)
    {
        return await _context.GradeConfigurations
            .FirstOrDefaultAsync(g => g.SchoolId == schoolId && g.GradeName == gradeName, cancellationToken);
    }

    public async Task<bool> AddAsync(GradeConfiguration configuration, CancellationToken cancellationToken = default)
    {
        _context.GradeConfigurations.Add(configuration);
        var result = await _context.SaveChangesAsync(cancellationToken);
        return result > 0;
    }

    public async Task<bool> UpdateAsync(GradeConfiguration configuration, CancellationToken cancellationToken = default)
    {
        _context.GradeConfigurations.Update(configuration);
        var result = await _context.SaveChangesAsync(cancellationToken);
        return result > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var grade = await _context.GradeConfigurations.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        if (grade == null)
            return false;

        _context.GradeConfigurations.Remove(grade);
        var result = await _context.SaveChangesAsync(cancellationToken);
        return result > 0;
    }
}
