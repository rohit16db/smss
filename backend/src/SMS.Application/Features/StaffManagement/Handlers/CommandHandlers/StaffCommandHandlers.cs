using MediatR;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.StaffManagement.Commands;
using SMS.Application.Features.StaffManagement.DTOs;
using Microsoft.EntityFrameworkCore;
using SMS.Domain.Entities;

namespace SMS.Application.Features.StaffManagement.Handlers.CommandHandlers;

/// <summary>
/// Handler for CreateStaffCommand
/// </summary>
public class CreateStaffCommandHandler : IRequestHandler<CreateStaffCommand, StaffDto>
{
    private readonly IApplicationDbContext _context;

    public CreateStaffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StaffDto> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
    {
        // 1. Create UserProfile for PII
        var userProfile = new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(), // Link to identity system
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            ImagePath = request.ImagePath
        };

        // 2. Create Staff record linked to Profile
        var staff = new Staff
        {
            Id = Guid.NewGuid(),
            UserProfileId = userProfile.Id,
            UserProfile = userProfile,
            DepartmentId = request.DepartmentId,
            Designation = request.Designation,
            RoleType = request.RoleType,
            ExperienceYears = request.ExperienceYears,
            JoiningDate = DateOnly.FromDateTime(request.JoiningDate),
            BasicSalary = request.BasicSalary,
            IsActive = true,
            CreatedBy = request.CreatedByUserId,
            UpdatedBy = request.CreatedByUserId
        };

        // 3. Add Qualifications
        if (request.Qualifications != null && request.Qualifications.Any())
        {
            foreach (var q in request.Qualifications)
            {
                staff.Qualifications.Add(new EducationalQualification
                {
                    DegreeName = q.Degree,
                    Institution = q.Institution,
                    YearOfPassing = q.YearOfPassing,
                    GradeOrPercentage = q.GradeOrPercentage
                });
            }
        }

        _context.Staff.Add(staff);
        await _context.SaveChangesAsync(cancellationToken);

        // Fetch again with includes for mapping
        var result = await _context.Staff
            .Include(s => s.UserProfile)
            .Include(s => s.Department)
            .Include(s => s.Qualifications)
            .FirstAsync(s => s.Id == staff.Id, cancellationToken);

        return MapToDto(result);
    }

    private static StaffDto MapToDto(Staff staff)
    {
        return new StaffDto
        {
            Id = staff.Id.ToString(),
            UserId = staff.UserProfile.UserId.ToString(),
            FirstName = staff.UserProfile.FirstName,
            LastName = staff.UserProfile.LastName,
            Email = staff.UserProfile.Email,
            Phone = staff.UserProfile.Phone,
            Designation = staff.Designation,
            DepartmentId = staff.DepartmentId,
            DepartmentName = staff.Department?.Name ?? "Unknown",
            RoleType = staff.RoleType,
            ExperienceYears = staff.ExperienceYears,
            JoiningDate = staff.JoiningDate.ToDateTime(TimeOnly.MinValue),
            BasicSalary = staff.BasicSalary,
            IsActive = staff.IsActive,
            CreatedAt = staff.CreatedAt,
            UpdatedAt = staff.UpdatedAt,
            ImagePath = staff.UserProfile.ImagePath,
            Qualifications = staff.Qualifications.Select(q => new EducationalQualificationDto
            {
                Id = q.Id,
                Degree = q.DegreeName,
                Institution = q.Institution,
                YearOfPassing = q.YearOfPassing,
                GradeOrPercentage = q.GradeOrPercentage
            }).ToList()
        };
    }
}

/// <summary>
/// Handler for UpdateStaffCommand
/// </summary>
public class UpdateStaffCommandHandler : IRequestHandler<UpdateStaffCommand, StaffDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateStaffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StaffDto> Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var staffId))
            throw new InvalidOperationException($"Invalid staff ID format: {request.Id}");

        var staff = await _context.Staff
            .Include(s => s.UserProfile)
            .Include(s => s.Department)
            .Include(s => s.Qualifications)
            .FirstOrDefaultAsync(s => s.Id == staffId, cancellationToken)
            ?? throw new InvalidOperationException($"Staff with ID {request.Id} not found");

        // Update Profile
        staff.UserProfile.FirstName = request.FirstName;
        staff.UserProfile.LastName = request.LastName;
        staff.UserProfile.Email = request.Email;
        staff.UserProfile.Phone = request.Phone;
        staff.UserProfile.ImagePath = request.ImagePath;
        
        // Update Staff
        staff.DepartmentId = request.DepartmentId;
        staff.Designation = request.Designation;
        staff.RoleType = request.RoleType;
        staff.ExperienceYears = request.ExperienceYears;
        staff.BasicSalary = request.BasicSalary;
        staff.IsActive = request.IsActive;
        staff.UpdatedBy = request.UpdatedByUserId;

        // Sync Qualifications
        _context.EducationalQualifications.RemoveRange(staff.Qualifications);
        staff.Qualifications.Clear();
        
        if (request.Qualifications != null)
        {
            foreach (var q in request.Qualifications)
            {
                staff.Qualifications.Add(new EducationalQualification
                {
                    DegreeName = q.Degree,
                    Institution = q.Institution,
                    YearOfPassing = q.YearOfPassing,
                    GradeOrPercentage = q.GradeOrPercentage
                });
            }
        }

        _context.Staff.Update(staff);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(staff);
    }

    private static StaffDto MapToDto(Staff staff)
    {
        return new StaffDto
        {
            Id = staff.Id.ToString(),
            UserId = staff.UserProfile.UserId.ToString(),
            FirstName = staff.UserProfile.FirstName,
            LastName = staff.UserProfile.LastName,
            Email = staff.UserProfile.Email,
            Phone = staff.UserProfile.Phone,
            Designation = staff.Designation,
            DepartmentId = staff.DepartmentId,
            DepartmentName = staff.Department?.Name ?? "Unknown",
            RoleType = staff.RoleType,
            ExperienceYears = staff.ExperienceYears,
            JoiningDate = staff.JoiningDate.ToDateTime(TimeOnly.MinValue),
            BasicSalary = staff.BasicSalary,
            IsActive = staff.IsActive,
            CreatedAt = staff.CreatedAt,
            UpdatedAt = staff.UpdatedAt,
            ImagePath = staff.UserProfile.ImagePath,
            Qualifications = staff.Qualifications.Select(q => new EducationalQualificationDto
            {
                Id = q.Id,
                Degree = q.DegreeName,
                Institution = q.Institution,
                YearOfPassing = q.YearOfPassing,
                GradeOrPercentage = q.GradeOrPercentage
            }).ToList()
        };
    }
}

/// <summary>
/// Handler for DeactivateStaffCommand
/// </summary>
public class DeactivateStaffCommandHandler : IRequestHandler<DeactivateStaffCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeactivateStaffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeactivateStaffCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var staffId))
            throw new InvalidOperationException($"Invalid staff ID format: {request.Id}");

        var staff = await _context.Staff.FindAsync(new object[] { staffId }, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Staff with ID {request.Id} not found");

        staff.IsActive = false;
        staff.UpdatedBy = request.UpdatedByUserId;

        _context.Staff.Update(staff);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

/// <summary>
/// Handler for ActivateStaffCommand
/// </summary>
public class ActivateStaffCommandHandler : IRequestHandler<ActivateStaffCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ActivateStaffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ActivateStaffCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var staffId))
            throw new InvalidOperationException($"Invalid staff ID format: {request.Id}");

        var staff = await _context.Staff.FindAsync(new object[] { staffId }, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Staff with ID {request.Id} not found");

        staff.IsActive = true;
        staff.UpdatedBy = request.UpdatedByUserId;

        _context.Staff.Update(staff);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

/// <summary>
/// Handler for DeleteStaffCommand
/// </summary>
public class DeleteStaffCommandHandler : IRequestHandler<DeleteStaffCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteStaffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteStaffCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var staffId))
            throw new InvalidOperationException($"Invalid staff ID format: {request.Id}");

        var staff = await _context.Staff.FindAsync(new object[] { staffId }, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Staff with ID {request.Id} not found");

        _context.Staff.Remove(staff);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
