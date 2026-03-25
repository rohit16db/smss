using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Students.Commands;
using SMS.Application.Students.DTOs;
using SMS.Application.Students.Queries;
using SMS.Domain.Entities;
using SMS.Domain.Exceptions;
using SMS.Domain.Interfaces;

namespace SMS.Application.Students.Handlers;

public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, StudentDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IStudentIdGenerator _studentIdGenerator;

    public CreateStudentCommandHandler(
        IApplicationDbContext context,
        IStudentIdGenerator studentIdGenerator)
    {
        _context = context;
        _studentIdGenerator = studentIdGenerator;
    }

    public async Task<StudentDto> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var existingStudent = await _context.Students
            .FirstOrDefaultAsync(s => s.Email == request.Email, cancellationToken);

        if (existingStudent != null)
        {
            throw new BusinessRuleValidationException($"Student with email {request.Email} already exists");
        }

        // Generate unique enrollment number using the StudentIdGenerator service
        var enrollmentNumber = await _studentIdGenerator.GenerateStudentIdAsync(cancellationToken);

        // Ensure DateOfBirth has UTC kind for PostgreSQL
        var dateOfBirth = request.DateOfBirth.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(request.DateOfBirth, DateTimeKind.Utc)
            : request.DateOfBirth.ToUniversalTime();

        var student = new Student
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            DateOfBirth = dateOfBirth,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            EnrollmentNumber = enrollmentNumber,
            EnrollmentDate = DateTime.UtcNow,
            IsActive = true,
            GuardianName = request.GuardianName,
            GuardianPhone = request.GuardianPhone,
            GuardianEmail = request.GuardianEmail,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Students.Add(student);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(student);
    }

    private static StudentDto MapToDto(Student student) => new()
    {
        Id = student.Id,
        FirstName = student.FirstName,
        LastName = student.LastName,
        Email = student.Email,
        Phone = student.PhoneNumber,
        DateOfBirth = student.DateOfBirth,
        Address = student.Address,
        City = student.City,
        State = student.State,
        PostalCode = student.PostalCode,
        EnrollmentNumber = student.EnrollmentNumber,
        EnrollmentDate = student.EnrollmentDate,
        IsActive = student.IsActive,
        ParentName = student.GuardianName,
        ParentPhone = student.GuardianPhone,
        ParentEmail = student.GuardianEmail,
        CreatedAt = student.CreatedAt,
        UpdatedAt = student.UpdatedAt,
        ImagePath = student.ImagePath
    };
}

public class UpdateStudentCommandHandler : IRequestHandler<UpdateStudentCommand, StudentDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentDto> Handle(UpdateStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (student == null)
        {
            throw new EntityNotFoundException(nameof(Student), request.Id);
        }

        // Check if email is being changed to an existing one
        if (student.Email != request.Email)
        {
            var existingStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.Email == request.Email && s.Id != request.Id, cancellationToken);

            if (existingStudent != null)
            {
                throw new BusinessRuleValidationException($"Student with email {request.Email} already exists");
            }
        }

        student.FirstName = request.FirstName;
        student.LastName = request.LastName;
        student.Email = request.Email;
        student.PhoneNumber = request.PhoneNumber;
        
        // Ensure DateOfBirth has UTC kind for PostgreSQL
        student.DateOfBirth = request.DateOfBirth.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(request.DateOfBirth, DateTimeKind.Utc)
            : request.DateOfBirth.ToUniversalTime();
            
        student.Address = request.Address;
        student.City = request.City;
        student.State = request.State;
        student.PostalCode = request.PostalCode;
        student.IsActive = request.IsActive;
        student.GuardianName = request.GuardianName;
        student.GuardianPhone = request.GuardianPhone;
        student.GuardianEmail = request.GuardianEmail;
        student.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(student);
    }

    private static StudentDto MapToDto(Student student) => new()
    {
        Id = student.Id,
        FirstName = student.FirstName,
        LastName = student.LastName,
        Email = student.Email,
        Phone = student.PhoneNumber,
        DateOfBirth = student.DateOfBirth,
        Address = student.Address,
        City = student.City,
        State = student.State,
        PostalCode = student.PostalCode,
        EnrollmentNumber = student.EnrollmentNumber,
        EnrollmentDate = student.EnrollmentDate,
        IsActive = student.IsActive,
        ParentName = student.GuardianName,
        ParentPhone = student.GuardianPhone,
        ParentEmail = student.GuardianEmail,
        CreatedAt = student.CreatedAt,
        UpdatedAt = student.UpdatedAt,
        ImagePath = student.ImagePath
    };
}

public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (student == null)
        {
            throw new EntityNotFoundException(nameof(Student), request.Id);
        }

        // Soft delete by setting IsActive to false
        student.IsActive = false;
        student.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

public class ActivateStudentCommandHandler : IRequestHandler<ActivateStudentCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ActivateStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ActivateStudentCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var studentId))
            throw new InvalidOperationException($"Invalid student ID format: {request.Id}");

        var student = await _context.Students.FindAsync(new object[] { studentId }, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Student with ID {request.Id} not found");

        student.IsActive = true;
        student.UpdatedAt = DateTime.UtcNow;

        _context.Students.Update(student);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

public class DeactivateStudentCommandHandler : IRequestHandler<DeactivateStudentCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeactivateStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeactivateStudentCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var studentId))
            throw new InvalidOperationException($"Invalid student ID format: {request.Id}");

        var student = await _context.Students.FindAsync(new object[] { studentId }, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException($"Student with ID {request.Id} not found");

        student.IsActive = false;
        student.UpdatedAt = DateTime.UtcNow;

        _context.Students.Update(student);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

public class GetStudentByIdQueryHandler : IRequestHandler<GetStudentByIdQuery, StudentDto>
{
    private readonly IApplicationDbContext _context;

    public GetStudentByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentDto> Handle(GetStudentByIdQuery request, CancellationToken cancellationToken)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken);

        if (student == null)
        {
            throw new EntityNotFoundException(nameof(Student), request.Id);
        }

        return MapToDto(student);
    }

    private static StudentDto MapToDto(Student student) => new()
    {
        Id = student.Id,
        FirstName = student.FirstName,
        LastName = student.LastName,
        Email = student.Email,
        Phone = student.PhoneNumber,
        DateOfBirth = student.DateOfBirth,
        Address = student.Address,
        City = student.City,
        State = student.State,
        PostalCode = student.PostalCode,
        EnrollmentNumber = student.EnrollmentNumber,
        EnrollmentDate = student.EnrollmentDate,
        IsActive = student.IsActive,
        ParentName = student.GuardianName,
        ParentPhone = student.GuardianPhone,
        ParentEmail = student.GuardianEmail,
        CreatedAt = student.CreatedAt,
        UpdatedAt = student.UpdatedAt,
        ImagePath = student.ImagePath
    };
}

public class GetAllStudentsQueryHandler : IRequestHandler<GetAllStudentsQuery, PagedResult<StudentDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllStudentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<StudentDto>> Handle(GetAllStudentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Students
            .Include(s => s.Enrollments
                .Where(ss => ss.Status == "Enrolled"))
                .ThenInclude(ss => ss.Section)
                .ThenInclude(sec => sec.Class)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(s =>
                s.FirstName.ToLower().Contains(searchTerm) ||
                s.LastName.ToLower().Contains(searchTerm) ||
                s.Email.ToLower().Contains(searchTerm) ||
                s.EnrollmentNumber.ToLower().Contains(searchTerm));
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(s => s.IsActive == request.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.City))
        {
            query = query.Where(s => s.City == request.City);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var students = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<StudentDto>
        {
            Items = students.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    private static StudentDto MapToDto(Student student)
    {
        var currentSection = student.Enrollments?.FirstOrDefault(ss => ss.Status == "Enrolled");
        
        return new StudentDto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            Email = student.Email,
            Phone = student.PhoneNumber,
            DateOfBirth = student.DateOfBirth,
            Address = student.Address,
            City = student.City,
            State = student.State,
            PostalCode = student.PostalCode,
            EnrollmentNumber = student.EnrollmentNumber,
            EnrollmentDate = student.EnrollmentDate,
            IsActive = student.IsActive,
            ParentName = student.GuardianName,
            ParentPhone = student.GuardianPhone,
            ParentEmail = student.GuardianEmail,
            CreatedAt = student.CreatedAt,
            UpdatedAt = student.UpdatedAt,
            CurrentSectionId = currentSection?.SectionId,
            CurrentSectionName = currentSection?.Section?.SectionName,
            CurrentClassName = currentSection?.Section?.Class?.Name,
            ImagePath = student.ImagePath
        };
    }
}
