using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Departments.DTOs;
using SMS.Application.Features.Departments.Queries;
using SMS.Application.Features.Departments.Commands;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Departments.Handlers;

public class GetAllDepartmentsQueryHandler : IRequestHandler<GetAllDepartmentsQuery, List<DepartmentListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllDepartmentsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<DepartmentListDto>> Handle(GetAllDepartmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Departments
            .Include(d => d.HeadOfDepartment)
                .ThenInclude(s => s!.UserProfile)
            .Include(d => d.StaffMembers)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(d => d.Name.ToLower().Contains(term) ||
                                     (d.Description != null && d.Description.ToLower().Contains(term)));
        }

        return await query
            .OrderBy(d => d.Name)
            .Select(d => new DepartmentListDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                HeadOfDepartmentName = d.HeadOfDepartment != null && d.HeadOfDepartment.UserProfile != null
                    ? d.HeadOfDepartment.UserProfile.FirstName + " " + d.HeadOfDepartment.UserProfile.LastName
                    : null,
                StaffCount = d.StaffMembers.Count
            })
            .ToListAsync(cancellationToken);
    }
}

public class GetDepartmentByIdQueryHandler : IRequestHandler<GetDepartmentByIdQuery, DepartmentDto?>
{
    private readonly IApplicationDbContext _context;

    public GetDepartmentByIdQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<DepartmentDto?> Handle(GetDepartmentByIdQuery request, CancellationToken cancellationToken)
    {
        var dept = await _context.Departments
            .Include(d => d.HeadOfDepartment)
                .ThenInclude(s => s!.UserProfile)
            .Include(d => d.StaffMembers)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (dept == null) return null;

        return new DepartmentDto
        {
            Id = dept.Id,
            Name = dept.Name,
            Description = dept.Description,
            HeadOfDepartmentId = dept.HeadOfDepartmentId,
            HeadOfDepartmentName = dept.HeadOfDepartment?.UserProfile != null
                ? dept.HeadOfDepartment.UserProfile.FirstName + " " + dept.HeadOfDepartment.UserProfile.LastName
                : null,
            StaffCount = dept.StaffMembers.Count,
            CreatedAt = dept.CreatedAt,
            UpdatedAt = dept.UpdatedAt
        };
    }
}

public class CreateDepartmentCommandHandler : IRequestHandler<CreateDepartmentCommand, DepartmentDto>
{
    private readonly IApplicationDbContext _context;

    public CreateDepartmentCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<DepartmentDto> Handle(CreateDepartmentCommand request, CancellationToken cancellationToken)
    {
        // Check for duplicate name
        var exists = await _context.Departments.AnyAsync(d => d.Name.ToLower() == request.Name.ToLower(), cancellationToken);
        if (exists) throw new InvalidOperationException($"Department with name '{request.Name}' already exists.");

        var department = new Department
        {
            Name = request.Name,
            Description = request.Description,
            HeadOfDepartmentId = request.HeadOfDepartmentId
        };

        _context.Departments.Add(department);
        await _context.SaveChangesAsync(cancellationToken);

        // Re-fetch with includes
        return (await new GetDepartmentByIdQueryHandler(_context).Handle(
            new GetDepartmentByIdQuery { Id = department.Id }, cancellationToken))!;
    }
}

public class UpdateDepartmentCommandHandler : IRequestHandler<UpdateDepartmentCommand, DepartmentDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateDepartmentCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<DepartmentDto> Handle(UpdateDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments.FindAsync(new object[] { request.Id }, cancellationToken);
        if (department == null) throw new InvalidOperationException($"Department with ID '{request.Id}' not found.");

        // Check for duplicate name (excluding self)
        var duplicate = await _context.Departments.AnyAsync(
            d => d.Name.ToLower() == request.Name.ToLower() && d.Id != request.Id, cancellationToken);
        if (duplicate) throw new InvalidOperationException($"Department with name '{request.Name}' already exists.");

        department.Name = request.Name;
        department.Description = request.Description;
        department.HeadOfDepartmentId = request.HeadOfDepartmentId;
        department.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return (await new GetDepartmentByIdQueryHandler(_context).Handle(
            new GetDepartmentByIdQuery { Id = department.Id }, cancellationToken))!;
    }
}

public class DeleteDepartmentCommandHandler : IRequestHandler<DeleteDepartmentCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteDepartmentCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<bool> Handle(DeleteDepartmentCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .Include(d => d.StaffMembers)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

        if (department == null) throw new InvalidOperationException($"Department with ID '{request.Id}' not found.");
        if (department.StaffMembers.Any()) throw new InvalidOperationException("Cannot delete department with assigned staff members. Please reassign staff first.");

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
