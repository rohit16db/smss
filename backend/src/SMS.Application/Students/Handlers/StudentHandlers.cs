using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Students.Commands;
using SMS.Application.Students.DTOs;
using SMS.Application.Students.Queries;
using SMS.Domain.Entities;
using SMS.Domain.Exceptions;
using SMS.Domain.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using Microsoft.Extensions.Logging;

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

public class DeleteStudentCommandHandler : IRequestHandler<DeleteStudentCommand, MediatR.Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteStudentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MediatR.Unit> Handle(DeleteStudentCommand request, CancellationToken cancellationToken)
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

        return MediatR.Unit.Value;
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
                s.EnrollmentNumber.ToLower().Contains(searchTerm) ||
                (s.PhoneNumber != null && s.PhoneNumber.Contains(searchTerm)) ||
                (s.GuardianPhone != null && s.GuardianPhone.Contains(searchTerm)));
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

public class GenerateStudentRegistrationFormPdfCommandHandler : IRequestHandler<GenerateStudentRegistrationFormPdfCommand, byte[]>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<GenerateStudentRegistrationFormPdfCommandHandler> _logger;

    public GenerateStudentRegistrationFormPdfCommandHandler(IApplicationDbContext context, ILogger<GenerateStudentRegistrationFormPdfCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<byte[]> Handle(GenerateStudentRegistrationFormPdfCommand request, CancellationToken cancellationToken)
    {
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.Id == request.StudentId, cancellationToken);

        if (student == null)
        {
            throw new EntityNotFoundException(nameof(Student), request.StudentId);
        }

        var enrollment = await _context.Enrollments
            .Include(e => e.Section)
                .ThenInclude(sec => sec.Class)
            .Include(e => e.AcademicYear)
            .FirstOrDefaultAsync(e => e.StudentId == student.Id && e.Status == "Enrolled", cancellationToken);

        var school = await _context.Schools
            .Where(s => s.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        StudentTransportAssignment? transport = null;
        if (enrollment != null)
        {
            transport = await _context.StudentTransportAssignments
                .Include(t => t.Route)
                    .ThenInclude(r => r!.Vehicle)
                .Include(t => t.RouteStop)
                .FirstOrDefaultAsync(t => t.EnrollmentId == enrollment.Id && t.IsActive, cancellationToken);
        }

        byte[]? studentPhotoBytes = null;
        if (!string.IsNullOrEmpty(student.ImagePath))
        {
            try
            {
                if (student.ImagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                    student.ImagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    using (var httpClient = new System.Net.Http.HttpClient())
                    {
                        studentPhotoBytes = await httpClient.GetByteArrayAsync(student.ImagePath, cancellationToken);
                    }
                }
                else
                {
                    var pathsToTry = new[]
                    {
                        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", student.ImagePath.TrimStart('/')),
                        Path.Combine(Directory.GetCurrentDirectory(), student.ImagePath.TrimStart('/')),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", student.ImagePath.TrimStart('/')),
                        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, student.ImagePath.TrimStart('/'))
                    };

                    foreach (var path in pathsToTry)
                    {
                        if (File.Exists(path))
                        {
                            studentPhotoBytes = await File.ReadAllBytesAsync(path, cancellationToken);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load student photo from {ImagePath}", student.ImagePath);
            }
        }

        byte[]? schoolLogoBytes = null;
        if (school?.LogoImage != null)
        {
            schoolLogoBytes = school.LogoImage;
        }

        try
        {
            var pdf = Document.Create(container =>
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                    var primaryColor = "#1e293b"; // Slate 800
                    var accentColor = "#1D4ED8"; // Royal Blue
                    var grayBackground = "#F8FAFC"; // Slate 50
                    var borderColor = "#E2E8F0"; // Slate 200

                    // Header
                    page.Header().Column(col =>
                    {
                        col.Spacing(5);
                        col.Item().Row(row =>
                        {
                            // Left: School logo and details aligned horizontally
                            row.RelativeItem().Row(logoRow =>
                            {
                                if (schoolLogoBytes != null)
                                {
                                    try
                                    {
                                        logoRow.ConstantItem(60).Height(50).Image(schoolLogoBytes).FitHeight();
                                        logoRow.ConstantItem(12); // Spacer
                                    }
                                    catch { }
                                }

                                logoRow.RelativeItem().Column(coll =>
                                {
                                    coll.Item().Text(school?.Name ?? "School Management System").FontSize(18).Bold().FontColor(primaryColor);
                                    coll.Item().Text(school?.Code ?? "Educational Excellence Office").FontSize(9).SemiBold().FontColor("#64748b");
                                    coll.Item().PaddingTop(2).Text(school?.Address ?? "123 Education Street, City").FontSize(8).FontColor("#94a3b8");
                                    coll.Item().Text($"Phone: {school?.PhoneNumber ?? "N/A"} | Email: {school?.EmailAddress ?? "N/A"}").FontSize(8).FontColor("#94a3b8");
                                });
                            });

                            // Right: Title & Date
                            row.AutoItem().MinWidth(180).Column(rCol =>
                            {
                                rCol.Item().BorderLeft(3).BorderColor(accentColor).PaddingLeft(10).Column(rc =>
                                {
                                    rc.Item().Background(grayBackground).Padding(5).AlignCenter().Text("STUDENT REGISTRATION").FontSize(10).Bold().FontColor(primaryColor);
                                    rc.Item().PaddingTop(5).AlignRight().Text("FORM GENERATED").FontSize(7).FontColor("#94a3b8");
                                    rc.Item().AlignRight().Text(DateTime.Now.ToString("dd MMMM, yyyy")).FontSize(9).FontColor(primaryColor);
                                    rc.Item().PaddingTop(5).AlignRight().Text("ENROLLMENT NO.").FontSize(7).FontColor("#94a3b8");
                                    rc.Item().AlignRight().Text(student.EnrollmentNumber).FontSize(11).Bold().FontColor(accentColor);
                                });
                            });
                        });
                        
                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(borderColor);
                    });

                    page.Content().Column(col =>
                    {
                        col.Spacing(12);

                        // Row for Student General Info + Student Photograph
                        col.Item().Row(row =>
                        {
                            // Left: Primary details
                            row.RelativeItem().Column(details =>
                            {
                                details.Spacing(8);
                                details.Item().Text("ACADEMIC DETAILS").FontSize(10).Bold().FontColor(accentColor);
                                
                                details.Item().Row(r =>
                                {
                                    r.RelativeItem().Column(c => {
                                        c.Item().Text("Enrollment Number").FontSize(7).FontColor("#64748b");
                                        c.Item().Text(student.EnrollmentNumber).FontSize(9).FontColor(primaryColor);
                                    });
                                    r.RelativeItem().Column(c => {
                                        c.Item().Text("Enrollment Date").FontSize(7).FontColor("#64748b");
                                        c.Item().Text(student.EnrollmentDate.ToString("dd MMM yyyy")).FontSize(9).FontColor(primaryColor);
                                    });
                                });
                                details.Item().PaddingTop(4).Row(r =>
                                {
                                    r.RelativeItem().Column(c => {
                                        c.Item().Text("Academic Year").FontSize(7).FontColor("#64748b");
                                        c.Item().Text(enrollment?.AcademicYear?.Name ?? "N/A").FontSize(9).FontColor(primaryColor);
                                    });
                                    r.RelativeItem().Column(c => {
                                        c.Item().Text("Class - Section").FontSize(7).FontColor("#64748b");
                                        c.Item().Text(enrollment != null ? $"{enrollment.Section?.Class?.Name} - {enrollment.Section?.SectionName}" : "Not Assigned").FontSize(9).FontColor(primaryColor);
                                    });
                                });
                            });

                            // Right: Photo frame
                            row.ConstantItem(100).AlignRight().Column(photoCol =>
                            {
                                photoCol.Item().AlignRight().Width(85).Height(105).Border(1).BorderColor(borderColor).Background(grayBackground).AlignMiddle().AlignCenter().Column(pFrame =>
                                {
                                    if (studentPhotoBytes != null)
                                    {
                                        try
                                        {
                                            pFrame.Item().Width(85).Height(105).Image(studentPhotoBytes).FitArea();
                                        }
                                        catch
                                        {
                                            pFrame.Item().Padding(5).AlignCenter().AlignMiddle().Text("Affix\nPhotograph\nHere").FontSize(7).FontColor("#94a3b8").AlignCenter();
                                        }
                                    }
                                    else
                                    {
                                        pFrame.Item().Padding(5).AlignCenter().AlignMiddle().Text("Affix\nPhotograph\nHere").FontSize(7).FontColor("#94a3b8").AlignCenter();
                                    }
                                });
                            });
                        });

                        col.Item().LineHorizontal(0.5f).LineColor(borderColor);

                        // Student Personal Info
                        col.Item().Text("PERSONAL DETAILS").FontSize(10).Bold().FontColor(accentColor);
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Column(c => {
                                c.Item().Text("First Name").FontSize(7).FontColor("#64748b");
                                c.Item().Text(student.FirstName).FontSize(9).FontColor(primaryColor);
                            });
                            r.RelativeItem().Column(c => {
                                c.Item().Text("Last Name").FontSize(7).FontColor("#64748b");
                                c.Item().Text(student.LastName).FontSize(9).FontColor(primaryColor);
                            });
                            r.RelativeItem().Column(c => {
                                c.Item().Text("Date of Birth").FontSize(7).FontColor("#64748b");
                                c.Item().Text(student.DateOfBirth.ToString("dd MMM yyyy")).FontSize(9).FontColor(primaryColor);
                            });
                        });
                        col.Item().PaddingTop(4).Row(r =>
                        {
                            r.RelativeItem().Column(c => {
                                c.Item().Text("Email Address").FontSize(7).FontColor("#64748b");
                                c.Item().Text(student.Email).FontSize(9).FontColor(primaryColor);
                            });
                            r.RelativeItem().Column(c => {
                                c.Item().Text("Phone Number").FontSize(7).FontColor("#64748b");
                                c.Item().Text(student.PhoneNumber ?? "N/A").FontSize(9).FontColor(primaryColor);
                            });
                            r.RelativeItem().Column(c => {
                                c.Item().Text("Status").FontSize(7).FontColor("#64748b");
                                c.Item().Text(student.IsActive ? "Active" : "Inactive").FontSize(9).FontColor(primaryColor);
                            });
                        });

                        col.Item().LineHorizontal(0.5f).LineColor(borderColor);

                        // Address Details
                        col.Item().Text("RESIDENTIAL ADDRESS").FontSize(10).Bold().FontColor(accentColor);
                        col.Item().Row(r =>
                        {
                            r.RelativeItem(2).Column(c => {
                                c.Item().Text("Street Address").FontSize(7).FontColor("#64748b");
                                c.Item().Text(student.Address ?? "N/A").FontSize(9).FontColor(primaryColor);
                            });
                            r.RelativeItem().Column(c => {
                                c.Item().Text("City").FontSize(7).FontColor("#64748b");
                                c.Item().Text(student.City ?? "N/A").FontSize(9).FontColor(primaryColor);
                            });
                        });
                        col.Item().PaddingTop(4).Row(r =>
                        {
                            r.RelativeItem().Column(c => {
                                c.Item().Text("State").FontSize(7).FontColor("#64748b");
                                c.Item().Text(student.State ?? "N/A").FontSize(9).FontColor(primaryColor);
                            });
                            r.RelativeItem().Column(c => {
                                c.Item().Text("Postal Code").FontSize(7).FontColor("#64748b");
                                c.Item().Text(student.PostalCode ?? "N/A").FontSize(9).FontColor(primaryColor);
                            });
                            r.RelativeItem();
                        });

                        col.Item().LineHorizontal(0.5f).LineColor(borderColor);

                        // Guardian Details
                        col.Item().Text("GUARDIAN / PARENT DETAILS").FontSize(10).Bold().FontColor(accentColor);
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Column(c => {
                                c.Item().Text("Guardian Name").FontSize(7).FontColor("#64748b");
                                c.Item().Text(student.GuardianName ?? "N/A").FontSize(9).FontColor(primaryColor);
                            });
                            r.RelativeItem().Column(c => {
                                c.Item().Text("Guardian Phone").FontSize(7).FontColor("#64748b");
                                c.Item().Text(student.GuardianPhone ?? "N/A").FontSize(9).FontColor(primaryColor);
                            });
                            r.RelativeItem().Column(c => {
                                c.Item().Text("Guardian Email").FontSize(7).FontColor("#64748b");
                                c.Item().Text(student.GuardianEmail ?? "N/A").FontSize(9).FontColor(primaryColor);
                            });
                        });

                        col.Item().LineHorizontal(0.5f).LineColor(borderColor);

                        // Transport Details
                        col.Item().Text("TRANSPORT DETAILS").FontSize(10).Bold().FontColor(accentColor);
                        if (transport != null)
                        {
                            col.Item().Row(r =>
                            {
                                r.RelativeItem().Column(c => {
                                    c.Item().Text("Assigned Route").FontSize(7).FontColor("#64748b");
                                    c.Item().Text(transport.Route?.RouteName ?? "N/A").FontSize(9).FontColor(primaryColor);
                                });
                                r.RelativeItem().Column(c => {
                                    c.Item().Text("Pick-up / Drop-off Stop").FontSize(7).FontColor("#64748b");
                                    c.Item().Text(transport.RouteStop?.StopName ?? "N/A").FontSize(9).FontColor(primaryColor);
                                });
                                r.RelativeItem().Column(c => {
                                    c.Item().Text("Vehicle Registry").FontSize(7).FontColor("#64748b");
                                    c.Item().Text(transport.Route?.Vehicle?.RegistrationNumber ?? "N/A").FontSize(9).FontColor(primaryColor);
                                });
                            });
                        }
                        else
                        {
                            col.Item().Text("No transport services assigned. Student travels independently.").FontSize(9).Italic().FontColor("#64748b");
                        }

                        col.Item().LineHorizontal(0.5f).LineColor(borderColor);

                        // Undertaking Declaration
                        col.Item().Background(grayBackground).Border(0.5f).BorderColor(borderColor).Padding(10).Column(decl =>
                        {
                            decl.Spacing(4);
                            decl.Item().Text("UNDERTAKING & DECLARATION").FontSize(8).Bold().FontColor(primaryColor);
                            decl.Item().Text("I hereby declare that all the information provided in this registration sheet is correct and true to the best of my knowledge. I understand that the school administration reserves the right to verify all student data and cancel the enrollment in case of discrepancies. This sheet will be maintained for official records and offline processing.").FontSize(7.5f).FontColor("#475569").LineHeight(1.2f);
                        });

                        // Signature Fields
                        col.Item().PaddingTop(30).Row(row =>
                        {
                            row.RelativeItem().Column(sigCol =>
                            {
                                sigCol.Item().PaddingTop(25).LineHorizontal(0.5f).LineColor("#94a3b8");
                                sigCol.Item().PaddingTop(2).Text("Parent / Guardian's Signature").FontSize(8).Bold().FontColor(primaryColor);
                                sigCol.Item().Text("Date: ________________________").FontSize(7).FontColor("#64748b");
                            });

                            row.ConstantItem(80); // spacer

                            row.RelativeItem().Column(sigCol =>
                            {
                                sigCol.Item().PaddingTop(25).LineHorizontal(0.5f).LineColor("#94a3b8");
                                sigCol.Item().PaddingTop(2).Text("Principal / Authorized Signature").FontSize(8).Bold().FontColor(primaryColor);
                                sigCol.Item().Text("Date: ________________________").FontSize(7).FontColor("#64748b");
                            });
                        });
                    });

                    page.Footer().AlignMiddle().AlignCenter().Text(t =>
                    {
                        t.Span("Page 1 of 1 • System Generated Registration Form • School Management System • Offline Copy").FontSize(7).FontColor("#cbd5e1");
                    });
                })
            ).GeneratePdf();

            return pdf;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building PDF document for student {StudentId}", request.StudentId);
            throw new InvalidOperationException("Failed to generate PDF document", ex);
        }
    }
}
