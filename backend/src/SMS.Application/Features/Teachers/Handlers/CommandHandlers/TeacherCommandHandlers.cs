using MediatR;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Teachers.Commands;
using SMS.Application.Features.Teachers.DTOs;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Teachers.Handlers.CommandHandlers;

/// <summary>
/// Handler for CreateTeacherCommand
/// </summary>
public class CreateTeacherCommandHandler : IRequestHandler<CreateTeacherCommand, TeacherDto>
{
    private readonly IApplicationDbContext _context;

    public CreateTeacherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherDto> Handle(CreateTeacherCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var teacher = new Teacher
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(), // Generate a new UserId for now (Phase 2 integration)
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Qualification = request.Qualification,
            ExperienceYears = request.ExperienceYears,
            JoiningDate = DateOnly.FromDateTime(request.JoiningDate),
            IsActive = true,
            CreatedBy = request.CreatedByUserId,
            UpdatedBy = request.CreatedByUserId
        };

        _context.Teachers.Add(teacher);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(teacher);
    }

    private static TeacherDto MapToDto(Teacher teacher)
    {
        return new TeacherDto
        {
            Id = teacher.Id.ToString(),
            UserId = teacher.UserId.ToString(),
            FirstName = teacher.FirstName,
            LastName = teacher.LastName,
            Email = teacher.Email,
            Phone = teacher.Phone ?? string.Empty,
            Qualification = teacher.Qualification ?? string.Empty,
            ExperienceYears = teacher.ExperienceYears,
            JoiningDate = teacher.JoiningDate.ToDateTime(TimeOnly.MinValue),
            IsActive = teacher.IsActive,
            CreatedAt = teacher.CreatedAt,
            UpdatedAt = teacher.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for UpdateTeacherCommand
/// </summary>
public class UpdateTeacherCommandHandler : IRequestHandler<UpdateTeacherCommand, TeacherDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateTeacherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherDto> Handle(UpdateTeacherCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var teacherId))
            throw new InvalidOperationException($"Invalid teacher ID format: {request.Id}");

        var teacher = await _context.Teachers.FindAsync(new object[] { teacherId }, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Teacher with ID {request.Id} not found");

        teacher.FirstName = request.FirstName;
        teacher.LastName = request.LastName;
        teacher.Email = request.Email;
        teacher.Phone = request.Phone;
        teacher.Qualification = request.Qualification;
        teacher.ExperienceYears = request.ExperienceYears;
        teacher.IsActive = request.IsActive;
        teacher.UpdatedBy = request.UpdatedByUserId;

        _context.Teachers.Update(teacher);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(teacher);
    }

    private static TeacherDto MapToDto(Teacher teacher)
    {
        return new TeacherDto
        {
            Id = teacher.Id.ToString(),
            UserId = teacher.UserId.ToString(),
            FirstName = teacher.FirstName,
            LastName = teacher.LastName,
            Email = teacher.Email,
            Phone = teacher.Phone ?? string.Empty,
            Qualification = teacher.Qualification ?? string.Empty,
            ExperienceYears = teacher.ExperienceYears,
            JoiningDate = teacher.JoiningDate.ToDateTime(TimeOnly.MinValue),
            IsActive = teacher.IsActive,
            CreatedAt = teacher.CreatedAt,
            UpdatedAt = teacher.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for DeactivateTeacherCommand
/// </summary>
public class DeactivateTeacherCommandHandler : IRequestHandler<DeactivateTeacherCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeactivateTeacherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeactivateTeacherCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var teacherId))
            throw new InvalidOperationException($"Invalid teacher ID format: {request.Id}");

        var teacher = await _context.Teachers.FindAsync(new object[] { teacherId }, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Teacher with ID {request.Id} not found");

        teacher.IsActive = false;
        teacher.UpdatedBy = request.UpdatedByUserId;

        _context.Teachers.Update(teacher);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

/// <summary>
/// Handler for ActivateTeacherCommand
/// </summary>
public class ActivateTeacherCommandHandler : IRequestHandler<ActivateTeacherCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ActivateTeacherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ActivateTeacherCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var teacherId))
            throw new InvalidOperationException($"Invalid teacher ID format: {request.Id}");

        var teacher = await _context.Teachers.FindAsync(new object[] { teacherId }, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Teacher with ID {request.Id} not found");

        teacher.IsActive = true;
        teacher.UpdatedBy = request.UpdatedByUserId;

        _context.Teachers.Update(teacher);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

/// <summary>
/// Handler for DeleteTeacherCommand
/// </summary>
public class DeleteTeacherCommandHandler : IRequestHandler<DeleteTeacherCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteTeacherCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteTeacherCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var teacherId))
            throw new InvalidOperationException($"Invalid teacher ID format: {request.Id}");

        var teacher = await _context.Teachers.FindAsync(new object[] { teacherId }, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Teacher with ID {request.Id} not found");

        _context.Teachers.Remove(teacher);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
