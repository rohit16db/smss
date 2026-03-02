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
        var teacher = await _context.Teachers.FindAsync(new object[] { request.TeacherId }, cancellationToken);
        if (teacher == null)
            throw new InvalidOperationException($"Teacher with ID {request.TeacherId} not found");

        var structures = await _context.SalaryStructures
            .Where(s => s.IsActive)
            .Where(s => s.MinExperienceYears <= teacher.ExperienceYears)
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

public class GetTeacherCurrentSalaryStructureQueryHandler : IRequestHandler<GetTeacherCurrentSalaryStructureQuery, TeacherSalaryAssignmentDto>
{
    private readonly IApplicationDbContext _context;

    public GetTeacherCurrentSalaryStructureQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherSalaryAssignmentDto> Handle(GetTeacherCurrentSalaryStructureQuery request, CancellationToken cancellationToken)
    {
        var teacher = await _context.Teachers
            .Include(t => t.SalaryStructure)
            .FirstOrDefaultAsync(t => t.Id == request.TeacherId, cancellationToken);

        if (teacher == null)
            throw new InvalidOperationException($"Teacher with ID {request.TeacherId} not found");

        if (teacher.SalaryStructure == null)
            throw new InvalidOperationException($"No salary structure assigned to teacher {teacher.FullName}");

        return new TeacherSalaryAssignmentDto
        {
            TeacherId = teacher.Id,
            TeacherName = teacher.FullName,
            TeacherEmail = teacher.Email,
            TeacherImagePath = teacher.ImagePath,
            SalaryStructureId = teacher.SalaryStructure.Id,
            SalaryStructureName = teacher.SalaryStructure.Name,
            GrossSalary = teacher.SalaryStructure.GrossSalary,
            EffectiveDate = teacher.SalaryStructureEffectiveDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            AssignedAt = teacher.SalaryStructure.CreatedAt
        };
    }
}

public class GetTeachersWithSalaryStructuresQueryHandler : IRequestHandler<GetTeachersWithSalaryStructuresQuery, List<TeacherSalaryAssignmentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTeachersWithSalaryStructuresQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TeacherSalaryAssignmentDto>> Handle(GetTeachersWithSalaryStructuresQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Teachers
            .Include(t => t.SalaryStructure)
            .Where(t => t.SalaryStructureId != null);

        if (request.IsActive.HasValue)
            query = query.Where(t => t.IsActive == request.IsActive.Value);

        var teachers = await query
            .OrderBy(t => t.FirstName)
            .ToListAsync(cancellationToken);

        return teachers.Select(t => new TeacherSalaryAssignmentDto
        {
            TeacherId = t.Id,
            TeacherName = t.FullName,
            TeacherEmail = t.Email,
            TeacherImagePath = t.ImagePath,
            SalaryStructureId = t.SalaryStructureId ?? Guid.Empty,
            SalaryStructureName = t.SalaryStructure?.Name ?? "N/A",
            GrossSalary = t.SalaryStructure?.GrossSalary ?? 0,
            EffectiveDate = t.SalaryStructureEffectiveDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            AssignedAt = t.SalaryStructure?.CreatedAt ?? DateTime.UtcNow
        }).ToList();
    }
}
