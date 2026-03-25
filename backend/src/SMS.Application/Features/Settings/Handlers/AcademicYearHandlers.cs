using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Settings.Commands;
using SMS.Application.Features.Settings.DTOs;
using SMS.Application.Features.Settings.Queries;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Settings.Handlers;

public class AcademicYearQueryHandlers : 
    IRequestHandler<GetAcademicYearsQuery, List<AcademicYearDto>>,
    IRequestHandler<GetActiveAcademicYearQuery, AcademicYearDto?>
{
    private readonly IApplicationDbContext _context;

    public AcademicYearQueryHandlers(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AcademicYearDto>> Handle(GetAcademicYearsQuery request, CancellationToken cancellationToken)
    {
        return await _context.AcademicYears
            .OrderByDescending(ay => ay.StartDate)
            .Select(ay => new AcademicYearDto
            {
                Id = ay.Id,
                Name = ay.Name,
                StartDate = ay.StartDate,
                EndDate = ay.EndDate,
                IsActive = ay.IsActive,
                Status = ay.IsActive ? "Active" : "Inactive"
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AcademicYearDto?> Handle(GetActiveAcademicYearQuery request, CancellationToken cancellationToken)
    {
        return await _context.AcademicYears
            .Where(ay => ay.IsActive)
            .Select(ay => new AcademicYearDto
            {
                Id = ay.Id,
                Name = ay.Name,
                StartDate = ay.StartDate,
                EndDate = ay.EndDate,
                IsActive = ay.IsActive,
                Status = "Active"
            })
            .OrderByDescending(ay => ay.StartDate)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

public class AcademicYearCommandHandlers :
    IRequestHandler<CreateAcademicYearCommand, AcademicYearDto>,
    IRequestHandler<ToggleAcademicYearStatusCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public AcademicYearCommandHandlers(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AcademicYearDto> Handle(CreateAcademicYearCommand request, CancellationToken cancellationToken)
    {
        // Deactivate other years if this one is intended to be active
        if (request.IsActive)
        {
            var activeYears = await _context.AcademicYears.Where(ay => ay.IsActive).ToListAsync(cancellationToken);
            foreach (var ay in activeYears)
            {
                ay.IsActive = false;
            }
        }

        var entity = new AcademicYear
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.AcademicYears.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new AcademicYearDto
        {
            Id = entity.Id,
            Name = entity.Name,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            IsActive = entity.IsActive,
            Status = entity.IsActive ? "Active" : "Inactive"
        };
    }

    public async Task<bool> Handle(ToggleAcademicYearStatusCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.AcademicYears.FindAsync(new object[] { request.Id }, cancellationToken);
        if (entity == null) return false;

        entity.IsActive = !entity.IsActive;
        
        // If enabling this year, deactivate others
        if (entity.IsActive)
        {
             var others = await _context.AcademicYears
                .Where(ay => ay.Id != request.Id && ay.IsActive)
                .ToListAsync(cancellationToken);
            
            foreach (var ay in others)
            {
                ay.IsActive = false;
            }
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
