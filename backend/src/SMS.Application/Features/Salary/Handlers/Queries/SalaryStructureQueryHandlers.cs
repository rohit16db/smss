using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Salary.DTOs;
using SMS.Application.Features.Salary.Queries;

namespace SMS.Application.Features.Salary.Handlers.Queries;

public class GetAllSalaryStructuresQueryHandler : IRequestHandler<GetAllSalaryStructuresQuery, List<SalaryStructureDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllSalaryStructuresQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SalaryStructureDto>> Handle(GetAllSalaryStructuresQuery request, CancellationToken cancellationToken)
    {
        var query = _context.SalaryStructures.AsQueryable();

        if (request.IsActive.HasValue)
            query = query.Where(s => s.IsActive == request.IsActive.Value);

        var structures = await query
            .OrderByDescending(s => s.EffectiveFromDate)
            .ToListAsync(cancellationToken);

        return structures.Select(MapToDto).ToList();
    }

    private SalaryStructureDto MapToDto(SMS.Domain.Entities.SalaryStructure structure)
    {
        return new SalaryStructureDto
        {
            Id = structure.Id,
            Name = structure.Name,
            Description = structure.Description,
            BaseSalary = structure.BaseSalary,
            HRA = structure.HRA,
            DA = structure.DA,
            MedicalAllowance = structure.MedicalAllowance,
            ConveyanceAllowance = structure.ConveyanceAllowance,
            OtherAllowances = structure.OtherAllowances,
            StandardDeduction = structure.StandardDeduction,
            GrossSalary = structure.GrossSalary,
            TotalAllowances = structure.TotalAllowances,
            MinExperienceYears = structure.MinExperienceYears,
            ApplicableQualifications = structure.ApplicableQualifications,
            IsActive = structure.IsActive,
            EffectiveFromDate = structure.EffectiveFromDate,
            EffectiveToDate = structure.EffectiveToDate,
            CreatedAt = structure.CreatedAt,
            UpdatedAt = structure.UpdatedAt
        };
    }
}

public class GetSalaryStructureByIdQueryHandler : IRequestHandler<GetSalaryStructureByIdQuery, SalaryStructureDto>
{
    private readonly IApplicationDbContext _context;

    public GetSalaryStructureByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SalaryStructureDto> Handle(GetSalaryStructureByIdQuery request, CancellationToken cancellationToken)
    {
        var structure = await _context.SalaryStructures.FindAsync(new object[] { request.Id }, cancellationToken);
        
        if (structure == null)
            throw new InvalidOperationException($"Salary structure with ID {request.Id} not found");

        return MapToDto(structure);
    }

    private SalaryStructureDto MapToDto(SMS.Domain.Entities.SalaryStructure structure)
    {
        return new SalaryStructureDto
        {
            Id = structure.Id,
            Name = structure.Name,
            Description = structure.Description,
            BaseSalary = structure.BaseSalary,
            HRA = structure.HRA,
            DA = structure.DA,
            MedicalAllowance = structure.MedicalAllowance,
            ConveyanceAllowance = structure.ConveyanceAllowance,
            OtherAllowances = structure.OtherAllowances,
            StandardDeduction = structure.StandardDeduction,
            GrossSalary = structure.GrossSalary,
            TotalAllowances = structure.TotalAllowances,
            MinExperienceYears = structure.MinExperienceYears,
            ApplicableQualifications = structure.ApplicableQualifications,
            IsActive = structure.IsActive,
            EffectiveFromDate = structure.EffectiveFromDate,
            EffectiveToDate = structure.EffectiveToDate,
            CreatedAt = structure.CreatedAt,
            UpdatedAt = structure.UpdatedAt
        };
    }
}

public class GetApplicableSalaryStructuresQueryHandler : IRequestHandler<GetApplicableSalaryStructuresQuery, List<SalaryStructureDto>>
{
    private readonly IApplicationDbContext _context;

    public GetApplicableSalaryStructuresQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SalaryStructureDto>> Handle(GetApplicableSalaryStructuresQuery request, CancellationToken cancellationToken)
    {
        var staff = await _context.Staff
            .FirstOrDefaultAsync(s => s.Id == request.StaffId, cancellationToken);
        if (staff == null)
            throw new InvalidOperationException($"Staff with ID {request.StaffId} not found");

        var structures = await _context.SalaryStructures
            .Where(s => s.IsActive)
            .Where(s => s.MinExperienceYears <= staff.ExperienceYears)
            .OrderByDescending(s => s.EffectiveFromDate)
            .ToListAsync(cancellationToken);

        return structures.Select(MapToDto).ToList();
    }

    private SalaryStructureDto MapToDto(SMS.Domain.Entities.SalaryStructure structure)
    {
        return new SalaryStructureDto
        {
            Id = structure.Id,
            Name = structure.Name,
            Description = structure.Description,
            BaseSalary = structure.BaseSalary,
            HRA = structure.HRA,
            DA = structure.DA,
            MedicalAllowance = structure.MedicalAllowance,
            ConveyanceAllowance = structure.ConveyanceAllowance,
            OtherAllowances = structure.OtherAllowances,
            StandardDeduction = structure.StandardDeduction,
            GrossSalary = structure.GrossSalary,
            TotalAllowances = structure.TotalAllowances,
            MinExperienceYears = structure.MinExperienceYears,
            ApplicableQualifications = structure.ApplicableQualifications,
            IsActive = structure.IsActive,
            EffectiveFromDate = structure.EffectiveFromDate,
            EffectiveToDate = structure.EffectiveToDate,
            CreatedAt = structure.CreatedAt,
            UpdatedAt = structure.UpdatedAt
        };
    }
}

public class GetStaffCurrentSalaryStructureQueryHandler : IRequestHandler<GetStaffCurrentSalaryStructureQuery, StaffSalaryAssignmentDto>
{
    private readonly IApplicationDbContext _context;

    public GetStaffCurrentSalaryStructureQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StaffSalaryAssignmentDto> Handle(GetStaffCurrentSalaryStructureQuery request, CancellationToken cancellationToken)
    {
        var staff = await _context.Staff
            .Include(t => t.UserProfile)
            .Include(t => t.SalaryStructure)
            .FirstOrDefaultAsync(t => t.Id == request.StaffId, cancellationToken);

        if (staff == null)
            throw new InvalidOperationException($"Staff with ID {request.StaffId} not found");

        if (staff.SalaryStructure == null)
            throw new InvalidOperationException($"No salary structure assigned to staff {staff.FullName}");

        return new StaffSalaryAssignmentDto
        {
            StaffId = staff.Id,
            StaffName = staff.FullName,
            StaffEmail = staff.UserProfile?.Email ?? string.Empty,
            StaffImagePath = staff.UserProfile?.ImagePath,
            SalaryStructureId = staff.SalaryStructure.Id,
            SalaryStructureName = staff.SalaryStructure.Name,
            GrossSalary = staff.SalaryStructure.GrossSalary,
            EffectiveDate = staff.SalaryStructureEffectiveDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            AssignedAt = staff.SalaryStructure.CreatedAt
        };
    }
}

public class GetStaffWithSalaryStructuresQueryHandler : IRequestHandler<GetStaffWithSalaryStructuresQuery, List<StaffSalaryAssignmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStaffWithSalaryStructuresQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StaffSalaryAssignmentDto>> Handle(GetStaffWithSalaryStructuresQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Staff
            .Include(t => t.UserProfile)
            .Include(t => t.SalaryStructure)
            .Where(t => t.SalaryStructureId != null);

        if (request.IsActive.HasValue)
            query = query.Where(t => t.IsActive == request.IsActive.Value);

        var staffMembers = await query
            .OrderBy(t => t.UserProfile.FirstName)
            .ToListAsync(cancellationToken);

        return staffMembers.Select(t => new StaffSalaryAssignmentDto
        {
            StaffId = t.Id,
            StaffName = t.FullName,
            StaffEmail = t.UserProfile?.Email ?? string.Empty,
            StaffImagePath = t.UserProfile?.ImagePath,
            SalaryStructureId = t.SalaryStructureId ?? Guid.Empty,
            SalaryStructureName = t.SalaryStructure?.Name ?? "N/A",
            GrossSalary = t.SalaryStructure?.GrossSalary ?? 0,
            EffectiveDate = t.SalaryStructureEffectiveDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            AssignedAt = t.SalaryStructure?.CreatedAt ?? DateTime.UtcNow
        }).ToList();
    }
}
