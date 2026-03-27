using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Subjects.Commands;
using SMS.Application.Features.Subjects.DTOs;
using SMS.Domain.Entities;
using SMS.Domain.Exceptions;

namespace SMS.Application.Features.Subjects.Handlers.CommandHandlers;

/// <summary>
/// Handler for creating a new subject
/// </summary>
public class CreateSubjectCommandHandler : IRequestHandler<CreateSubjectCommand, SubjectDto>
{
    private readonly IApplicationDbContext _context;

    public CreateSubjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubjectDto> Handle(CreateSubjectCommand request, CancellationToken cancellationToken)
    {
        // Check if code already exists
        var exists = await _context.Subjects
            .AnyAsync(s => s.Code == request.Code, cancellationToken);

        if (exists)
        {
            throw new ValidationException($"Subject with code '{request.Code}' already exists");
        }

        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            Credits = request.Credits,
            DisplayOrder = request.DisplayOrder,
            IsActive = true
        };

        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync(cancellationToken);

        return new SubjectDto
        {
            Id = subject.Id.ToString(),
            Name = subject.Name,
            Code = subject.Code,
            Description = subject.Description,
            Credits = subject.Credits,
            IsActive = subject.IsActive,
            DisplayOrder = subject.DisplayOrder,
            CreatedAt = subject.CreatedAt,
            UpdatedAt = subject.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for updating an existing subject
/// </summary>
public class UpdateSubjectCommandHandler : IRequestHandler<UpdateSubjectCommand, SubjectDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateSubjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubjectDto> Handle(UpdateSubjectCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var subjectId))
        {
            throw new ValidationException("Invalid subject ID");
        }

        var subject = await _context.Subjects
            .FirstOrDefaultAsync(s => s.Id == subjectId, cancellationToken);

        if (subject == null)
        {
            throw new NotFoundException("Subject not found");
        }

        // Check if code is being changed and if new code already exists
        if (subject.Code != request.Code)
        {
            var codeExists = await _context.Subjects
                .AnyAsync(s => s.Code == request.Code && s.Id != subjectId, cancellationToken);

            if (codeExists)
            {
                throw new ValidationException($"Subject with code '{request.Code}' already exists");
            }
        }

        subject.Name = request.Name;
        subject.Code = request.Code;
        subject.Description = request.Description;
        subject.Credits = request.Credits;
        subject.IsActive = request.IsActive;
        subject.DisplayOrder = request.DisplayOrder;

        await _context.SaveChangesAsync(cancellationToken);

        return new SubjectDto
        {
            Id = subject.Id.ToString(),
            Name = subject.Name,
            Code = subject.Code,
            Description = subject.Description,
            Credits = subject.Credits,
            IsActive = subject.IsActive,
            DisplayOrder = subject.DisplayOrder,
            CreatedAt = subject.CreatedAt,
            UpdatedAt = subject.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for deleting a subject
/// </summary>
public class DeleteSubjectCommandHandler : IRequestHandler<DeleteSubjectCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteSubjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteSubjectCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var subjectId))
        {
            throw new ValidationException("Invalid subject ID");
        }

        var subject = await _context.Subjects
            .FirstOrDefaultAsync(s => s.Id == subjectId, cancellationToken);

        if (subject == null)
        {
            throw new NotFoundException("Subject not found");
        }

        // Check if subject is assigned to any teachers
        var hasAssignments = await _context.StaffAssignments
            .AnyAsync(ta => ta.SubjectId == subjectId && ta.RemovalDate == null, cancellationToken);

        if (hasAssignments)
        {
            throw new ValidationException("Cannot delete subject with active teacher assignments");
        }

        _context.Subjects.Remove(subject);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
