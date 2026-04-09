using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Fees.Commands;
using SMS.Application.Features.Fees.DTOs;
using SMS.Application.Features.Fees.Queries;
using SMS.Domain.Entities;
using System.IO;

namespace SMS.Application.Features.Fees.Handlers.CommandHandlers;

/// <summary>
/// Handler for CreateFeeStructureCommand
/// </summary>
public class CreateFeeStructureCommandHandler : IRequestHandler<CreateFeeStructureCommand, FeeStructureDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAcademicYearContext _academicYearContext;

    public CreateFeeStructureCommandHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
    {
        _context = context;
        _academicYearContext = academicYearContext;
    }

    public async Task<FeeStructureDto> Handle(CreateFeeStructureCommand request, CancellationToken cancellationToken)
    {
        var feeStructureId = Guid.NewGuid();
        
        // Calculate total amount
        var totalAmount = request.Categories.Sum(c => c.Amount);

        var feeStructure = new FeeStructure
        {
            Id = feeStructureId,
            Name = request.Name,
            AcademicYearId = Guid.TryParse(request.AcademicYearId, out var ayId) ? ayId : _academicYearContext.RequiredAcademicYearId,
            Frequency = request.Frequency,
            TotalAmount = totalAmount,
            IsActive = true,
            CreatedBy = request.CreatedByUserId,
            UpdatedBy = request.CreatedByUserId
        };

        // Create categories
        foreach (var categoryDto in request.Categories)
        {
            var category = new FeeStructureCategory
            {
                Id = Guid.NewGuid(),
                FeeStructureId = feeStructureId,
                Category = categoryDto.Category,
                Amount = categoryDto.Amount,
                CreatedBy = request.CreatedByUserId,
                UpdatedBy = request.CreatedByUserId
            };
            feeStructure.Categories.Add(category);
        }

        _context.FeeStructures.Add(feeStructure);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(feeStructure);
    }

    private static FeeStructureDto MapToDto(FeeStructure feeStructure)
    {
        return new FeeStructureDto
        {
            Id = feeStructure.Id.ToString(),
            Name = feeStructure.Name,
            AcademicYearId = feeStructure.AcademicYearId.ToString(),
            Frequency = feeStructure.Frequency,
            TotalAmount = feeStructure.TotalAmount,
            IsActive = feeStructure.IsActive,
            Categories = feeStructure.Categories.Select(c => new FeeStructureCategoryDto
            {
                Category = c.Category,
                Amount = c.Amount
            }).ToList(),
            CreatedAt = feeStructure.CreatedAt,
            UpdatedAt = feeStructure.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for UpdateFeeStructureCommand
/// </summary>
public class UpdateFeeStructureCommandHandler : IRequestHandler<UpdateFeeStructureCommand, FeeStructureDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAcademicYearContext _academicYearContext;

    public UpdateFeeStructureCommandHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
    {
        _context = context;
        _academicYearContext = academicYearContext;
    }

    public async Task<FeeStructureDto> Handle(UpdateFeeStructureCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var feeStructureId))
            throw new InvalidOperationException($"Invalid fee structure ID format: {request.Id}");

        var feeStructure = await _context.FeeStructures
            .Include(f => f.Categories)
            .FirstOrDefaultAsync(f => f.Id == feeStructureId, cancellationToken)
            ?? throw new InvalidOperationException($"Fee structure with ID {request.Id} not found");

        // Update basic properties
        feeStructure.Name = request.Name;
        feeStructure.AcademicYearId = Guid.TryParse(request.AcademicYearId, out var updateAyId) ? updateAyId : _academicYearContext.RequiredAcademicYearId;
        feeStructure.Frequency = request.Frequency;
        feeStructure.IsActive = request.IsActive;
        feeStructure.UpdatedBy = request.UpdatedByUserId;

        // Remove existing categories
        _context.FeeStructureCategories.RemoveRange(feeStructure.Categories);

        // Add new categories
        feeStructure.Categories.Clear();
        foreach (var categoryDto in request.Categories)
        {
            var category = new FeeStructureCategory
            {
                Id = Guid.NewGuid(),
                FeeStructureId = feeStructureId,
                Category = categoryDto.Category,
                Amount = categoryDto.Amount,
                CreatedBy = request.UpdatedByUserId,
                UpdatedBy = request.UpdatedByUserId
            };
            feeStructure.Categories.Add(category);
        }

        // Recalculate total amount
        feeStructure.TotalAmount = request.Categories.Sum(c => c.Amount);

        _context.FeeStructures.Update(feeStructure);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(feeStructure);
    }

    private static FeeStructureDto MapToDto(FeeStructure feeStructure)
    {
        return new FeeStructureDto
        {
            Id = feeStructure.Id.ToString(),
            Name = feeStructure.Name,
            AcademicYearId = feeStructure.AcademicYearId.ToString(),
            Frequency = feeStructure.Frequency,
            TotalAmount = feeStructure.TotalAmount,
            IsActive = feeStructure.IsActive,
            Categories = feeStructure.Categories.Select(c => new FeeStructureCategoryDto
            {
                Category = c.Category,
                Amount = c.Amount
            }).ToList(),
            CreatedAt = feeStructure.CreatedAt,
            UpdatedAt = feeStructure.UpdatedAt
        };
    }
}

/// <summary>
/// Handler for DeleteFeeStructureCommand
/// </summary>
public class DeleteFeeStructureCommandHandler : IRequestHandler<DeleteFeeStructureCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteFeeStructureCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteFeeStructureCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var feeStructureId))
            throw new InvalidOperationException($"Invalid fee structure ID format: {request.Id}");

        var feeStructure = await _context.FeeStructures
            .Include(f => f.Categories)
            .FirstOrDefaultAsync(f => f.Id == feeStructureId, cancellationToken)
            ?? throw new InvalidOperationException($"Fee structure with ID {request.Id} not found");

        _context.FeeStructureCategories.RemoveRange(feeStructure.Categories);
        _context.FeeStructures.Remove(feeStructure);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}

/// <summary>
/// Handler for AssignStudentFeeCommand
/// </summary>
public class AssignStudentFeeCommandHandler : IRequestHandler<AssignStudentFeeCommand, StudentFeeDto>
{
    private readonly IApplicationDbContext _context;

    public AssignStudentFeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentFeeDto> Handle(AssignStudentFeeCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Assigning fee: Student={request.StudentId}, Structure={request.FeeStructureId}");
        Guid studentId;
        Student? student;
        
        // Try to parse as GUID, otherwise try to find by EnrollmentNumber
        if (!Guid.TryParse(request.StudentId, out studentId))
        {
            student = await _context.Students
                .FirstOrDefaultAsync(s => s.EnrollmentNumber == request.StudentId, cancellationToken);
            
            if (student == null)
                throw new InvalidOperationException($"Student with ID or Enrollment Number '{request.StudentId}' not found");
                
            studentId = student.Id;
        }
        else
        {
             student = await _context.Students
                 .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);
             
             if (student == null)
                 throw new InvalidOperationException($"Student with ID '{studentId}' not found");
        }

        if (!Guid.TryParse(request.FeeStructureId, out var feeStructureId))
            throw new InvalidOperationException($"Invalid fee structure ID format: {request.FeeStructureId}");

        var feeStructure = await _context.FeeStructures
            .FirstOrDefaultAsync(f => f.Id == feeStructureId, cancellationToken)
            ?? throw new InvalidOperationException($"Fee structure with ID {request.FeeStructureId} not found");

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId && e.Status == "Enrolled", cancellationToken)
            ?? throw new InvalidOperationException($"No active enrollment found for student.");

        var studentFee = new StudentFee
        {
            EnrollmentId = enrollment.Id,
            FeeStructureId = feeStructureId,
            StartDate = DateOnly.FromDateTime(request.StartDate),
            EndDate = request.EndDate.HasValue ? DateOnly.FromDateTime(request.EndDate.Value) : null,
            StructureAmount = feeStructure.TotalAmount,
            TransportFeeAmount = 0, // Default to 0, will be updated by sync
            IsActive = true,
            CreatedBy = request.CreatedByUserId,
            UpdatedBy = request.CreatedByUserId
        };

        _context.StudentFees.Add(studentFee);
        Console.WriteLine("Saving student fee to database...");
        await _context.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"Student fee saved successfully with ID: {studentFee.Id}");

        return new StudentFeeDto
        {
            Id = studentFee.Id.ToString(),
            StudentId = student.Id.ToString(),
            StudentName = $"{student.FirstName} {student.LastName}",
            EnrollmentNumber = student.EnrollmentNumber,
            FeeStructureId = studentFee.FeeStructureId.ToString(),
            FeeStructureName = feeStructure.Name,
            StartDate = studentFee.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            EndDate = studentFee.EndDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            TotalAmount = studentFee.TotalAmount,
            PaidAmount = 0,
            BalanceAmount = studentFee.TotalAmount,
            IsActive = studentFee.IsActive,
            CreatedAt = studentFee.CreatedAt
        };
    }
}

/// <summary>
/// Handler for TerminateStudentFeeCommandHandler
/// </summary>
public class TerminateStudentFeeCommandHandler : IRequestHandler<TerminateStudentFeeCommand, StudentFeeDto>
{
    private readonly IApplicationDbContext _context;

    public TerminateStudentFeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentFeeDto> Handle(TerminateStudentFeeCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var studentFeeId))
            throw new InvalidOperationException($"Invalid student fee ID format: {request.Id}");

        var studentFee = await _context.StudentFees
            .Include(sf => sf.Enrollment)
            .ThenInclude(e => e.Student)
            .Include(sf => sf.FeeStructure)
            .Include(sf => sf.Payments)
            .FirstOrDefaultAsync(sf => sf.Id == studentFeeId, cancellationToken)
            ?? throw new InvalidOperationException($"Student fee with ID {request.Id} not found");

        // Set end date and mark as inactive
        studentFee.EndDate = DateOnly.FromDateTime(request.EndDate);
        studentFee.IsActive = false;
        studentFee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Calculate paid amount
        var paidAmount = studentFee.Payments.Sum(p => p.AmountPaid);
        var balanceAmount = studentFee.TotalAmount - paidAmount;

        return new StudentFeeDto
        {
            Id = studentFee.Id.ToString(),
            StudentId = studentFee.Enrollment?.StudentId.ToString() ?? "",
            StudentName = studentFee.Enrollment?.Student != null ? $"{studentFee.Enrollment.Student.FirstName} {studentFee.Enrollment.Student.LastName}" : "Unknown",
            EnrollmentNumber = studentFee.Enrollment?.Student?.EnrollmentNumber ?? "Unknown",
            FeeStructureId = studentFee.FeeStructureId.ToString(),
            FeeStructureName = studentFee.FeeStructure?.Name ?? "Unknown",
            StartDate = studentFee.StartDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            EndDate = studentFee.EndDate?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            TotalAmount = studentFee.TotalAmount,
            PaidAmount = paidAmount,
            BalanceAmount = balanceAmount,
            IsActive = studentFee.IsActive,
            CreatedAt = studentFee.CreatedAt
        };
    }
}

/// <summary>
/// Handler for RecordFeePaymentCommand
/// </summary>
public class RecordFeePaymentCommandHandler : IRequestHandler<RecordFeePaymentCommand, FeePaymentDto>
{
    private readonly IApplicationDbContext _context;

    public RecordFeePaymentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FeePaymentDto> Handle(RecordFeePaymentCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.StudentFeeId, out var studentFeeId))
            throw new InvalidOperationException($"Invalid student fee ID format: {request.StudentFeeId}");

        var studentFee = await _context.StudentFees
            .FirstOrDefaultAsync(sf => sf.Id == studentFeeId, cancellationToken)
            ?? throw new InvalidOperationException($"Student fee with ID {request.StudentFeeId} not found");

        // Generate sequential receipt number format: RCP-0001, RCP-0002, etc.
        var paymentCount = await _context.FeePayments.CountAsync(cancellationToken);
        var receiptNumber = $"RCP-{(paymentCount + 1):D4}";

        var payment = new FeePayment
        {
            Id = Guid.NewGuid(),
            StudentFeeId = studentFeeId,
            AmountPaid = request.AmountPaid,
            PaymentDate = DateOnly.FromDateTime(request.PaymentDate),
            ReceiptNumber = receiptNumber,
            PaymentMethod = request.PaymentMethod,
            Notes = request.Notes,
            CreatedBy = request.CreatedByUserId,
            UpdatedBy = request.CreatedByUserId
        };

        _context.FeePayments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return new FeePaymentDto
        {
            Id = payment.Id.ToString(),
            StudentFeeId = payment.StudentFeeId.ToString(),
            AmountPaid = payment.AmountPaid,
            PaymentDate = payment.PaymentDate.ToDateTime(TimeOnly.MinValue),
            ReceiptNumber = payment.ReceiptNumber,
            PaymentMethod = payment.PaymentMethod,
            Notes = payment.Notes,
            CreatedAt = payment.CreatedAt
        };
    }
}

/// <summary>
/// Handler for BulkAssignStudentFeeCommand
/// </summary>
public class BulkAssignStudentFeeCommandHandler : IRequestHandler<BulkAssignStudentFeeCommand, BulkAssignmentResultDto>
{
    private readonly IApplicationDbContext _context;

    public BulkAssignStudentFeeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BulkAssignmentResultDto> Handle(BulkAssignStudentFeeCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.FeeStructureId, out var feeStructureId))
            throw new InvalidOperationException($"Invalid fee structure ID format: {request.FeeStructureId}");

        if (!Guid.TryParse(request.SectionId, out var sectionId))
            throw new InvalidOperationException($"Invalid section ID format: {request.SectionId}");

        // Verify fee structure exists and is active
        var feeStructure = await _context.FeeStructures
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == feeStructureId && f.IsActive, cancellationToken);

        if (feeStructure == null)
            throw new InvalidOperationException($"Fee structure not found or inactive: {request.FeeStructureId}");

        // Get all students currently enrolled in the section
        var enrollmentsInSection = await _context.Enrollments
            .Include(e => e.Student)
            .AsNoTracking()
            .Where(ss => ss.SectionId == sectionId && ss.Status == "Enrolled")
            .Select(ss => new { EnrollmentId = ss.Id, StudentId = ss.StudentId, StudentName = ss.Student!.FirstName + " " + ss.Student!.LastName })
            .ToListAsync(cancellationToken);

        if (!enrollmentsInSection.Any())
            throw new InvalidOperationException($"No students found in section: {request.SectionId}");

        var startDateOnly = DateOnly.FromDateTime(request.StartDate);
        var endDateOnly = request.EndDate.HasValue ? DateOnly.FromDateTime(request.EndDate.Value) : (DateOnly?)null;

        var result = new BulkAssignmentResultDto
        {
            SuccessCount = 0,
            SkippedCount = 0,
            FailureCount = 0,
            TotalAssignedAmount = 0,
            Errors = new(),
            AssignedAt = DateTime.UtcNow
        };

        // Get all existing assignments for this fee structure in this period
        var existingAssignments = await _context.StudentFees
            .Include(sf => sf.Enrollment)
            .Where(sf => sf.FeeStructureId == feeStructureId &&
                   enrollmentsInSection.Select(s => s.EnrollmentId).Contains(sf.EnrollmentId) &&
                   (sf.EndDate == null || sf.EndDate > startDateOnly))
            .ToListAsync(cancellationToken);

        var assignmentsToCreate = new List<StudentFee>();
        var assignmentsToUpdate = new List<StudentFee>();

        foreach (var studentData in enrollmentsInSection)
        {
            try
            {
                var enrollmentId = studentData.EnrollmentId;
                
                // Check if student already has an active assignment for this fee structure
                var existingAssignment = existingAssignments.FirstOrDefault(sf => sf.EnrollmentId == enrollmentId);

                if (existingAssignment != null)
                {
                    if (request.SkipAlreadyAssigned)
                    {
                        result.SkippedCount++;
                        continue;
                    }
                    else
                    {
                        // Terminate existing assignment
                        existingAssignment.EndDate = startDateOnly.AddDays(-1);
                        existingAssignment.UpdatedBy = request.CreatedByUserId;
                        assignmentsToUpdate.Add(existingAssignment);
                    }
                }

                // Create new student fee assignment
                var studentFee = new StudentFee
                {
                    Id = Guid.NewGuid(),
                    EnrollmentId = enrollmentId,
                    FeeStructureId = feeStructureId,
                    StartDate = startDateOnly,
                    EndDate = endDateOnly,
                    StructureAmount = feeStructure.TotalAmount,
                    TransportFeeAmount = 0, // Default to 0, will be updated by sync
                    CreatedBy = request.CreatedByUserId,
                    UpdatedBy = request.CreatedByUserId
                };

                assignmentsToCreate.Add(studentFee);
                result.SuccessCount++;
                result.TotalAssignedAmount += feeStructure.TotalAmount;
            }
            catch (Exception ex)
            {
                result.FailureCount++;
                result.Errors.Add(new AssignmentErrorDto
                {
                    StudentId = studentData.StudentId.ToString(),
                    StudentName = studentData.StudentName ?? "Unknown",
                    ErrorMessage = ex.Message
                });
            }
        }

        if (assignmentsToUpdate.Any())
        {
            _context.StudentFees.UpdateRange(assignmentsToUpdate);
        }

        if (assignmentsToCreate.Any())
        {
            _context.StudentFees.AddRange(assignmentsToCreate);
        }

        if (assignmentsToCreate.Any() || assignmentsToUpdate.Any())
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}

/// <summary>
/// Handler for GenerateFeeReceiptPdfCommand
/// Generates a PDF receipt for a fee payment using QuestPDF
/// </summary>
public class GenerateFeeReceiptPdfCommandHandler : IRequestHandler<GenerateFeeReceiptPdfCommand, byte[]>
{
    private readonly IMediator _mediator;

    public GenerateFeeReceiptPdfCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<byte[]> Handle(GenerateFeeReceiptPdfCommand request, CancellationToken cancellationToken)
    {
        // Get receipt data from query
        var receiptData = await _mediator.Send(new GetFeeReceiptDataQuery { PaymentId = request.PaymentId }, cancellationToken);

        if (receiptData == null)
            throw new InvalidOperationException("Payment not found");

        try
        {
            // Generate PDF using QuestPDF
            var pdf = Document.Create(container =>
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                    // COLORS
                    var primaryColor = "#1e293b"; // Slate 800
                    var accentColor = "#1D4ED8"; // Royal Blue
                    var grayBackground = "#F8FAFC"; // Slate 50
                    var borderColor = "#E2E8F0"; // Slate 200

                    page.Header().PaddingBottom(20).Row(row =>
                    {
                        // Left: School Info
                        row.RelativeItem().Column(col =>
                        {
                            if (!string.IsNullOrEmpty(receiptData.SchoolLogoBase64))
                            {
                                try
                                {
                                    var logoBytes = Convert.FromBase64String(receiptData.SchoolLogoBase64!);
                                    col.Item().PaddingBottom(10).Height(50).Image(logoBytes).FitHeight();
                                }
                                catch { }
                            }

                            col.Item().Text(receiptData.SchoolName ?? "LORIGIN KIDS GARDEN").FontSize(22).Bold().FontColor(primaryColor);
                            col.Item().Text(receiptData.SchoolCode ?? "Institutional Excellence Office").FontSize(11).SemiBold().FontColor("#64748b");
                            col.Item().PaddingTop(4).Text(receiptData.SchoolAddress ?? "").FontSize(8).FontColor("#94a3b8");
                            col.Item().Text($"Phone: {receiptData.SchoolPhone ?? "N/A"} | Email: {receiptData.SchoolEmail ?? "N/A"}").FontSize(8).FontColor("#94a3b8");
                        });

                        // Right: Receipt Badge
                        row.AutoItem().MinWidth(180).Column(col =>
                        {
                            col.Item().BorderLeft(3).BorderColor(primaryColor).PaddingLeft(10).Column(rCol =>
                            {
                                rCol.Item().Background(grayBackground).Padding(5).AlignCenter().Text("OFFICIAL FEE RECEIPT").FontSize(11).Bold().FontColor(primaryColor);
                                
                                rCol.Item().PaddingTop(10).AlignRight().Text("RECEIPT NUMBER").FontSize(7).FontColor("#94a3b8");
                                rCol.Item().AlignRight().Text(receiptData.ReceiptNumber).FontSize(10).Bold().FontColor(primaryColor);
                                
                                rCol.Item().PaddingTop(5).AlignRight().Text("ISSUE DATE").FontSize(7).FontColor("#94a3b8");
                                rCol.Item().AlignRight().Text(receiptData.PaymentDate.ToString("dd MMMM, yyyy")).FontSize(9).FontColor(primaryColor);
                            });
                        });
                    });

                    page.Content().Column(col =>
                    {
                        // Student Details Block
                        col.Item().PaddingVertical(10).Background(grayBackground).Padding(15).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("STUDENT NAME").FontSize(7).FontColor("#64748b");
                                c.Item().Text(receiptData.StudentName).FontSize(11).Bold().FontColor(primaryColor);
                                
                                c.Item().PaddingTop(10).Text("ACADEMIC YEAR").FontSize(7).FontColor("#64748b");
                                c.Item().Text(receiptData.AcademicYear).FontSize(10).FontColor(primaryColor);
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("ENROLLMENT NO.").FontSize(7).FontColor("#64748b");
                                c.Item().Text(receiptData.EnrollmentNumber).FontSize(11).Bold().FontColor(primaryColor);
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("CLASS / SECTION").FontSize(7).FontColor("#64748b");
                                c.Item().Text($"{receiptData.ClassName} - {receiptData.SectionName}").FontSize(11).Bold().FontColor(primaryColor);
                            });
                        });

                        col.Item().PaddingTop(30).Row(row =>
                        {
                            row.RelativeItem().Text("PARTICULARS").FontSize(9).Bold().FontColor("#64748b");
                            row.AutoItem().Text("AMOUNT (INR)").FontSize(9).Bold().FontColor("#64748b");
                        });
                        
                        col.Item().PaddingVertical(5).LineHorizontal(0.5f).LineColor(borderColor);

                        // Items
                        col.Item().Column(list =>
                        {
                            foreach (var cat in receiptData.Categories)
                            {
                                list.Item().PaddingVertical(8).Row(row =>
                                {
                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text(cat.Category).FontSize(10).Bold().FontColor(primaryColor);
                                        // Specific description mapping based on category name
                                        var desc = GetDescriptionForCategory(cat.Category, receiptData.FeeStructureName);
                                        if (!string.IsNullOrEmpty(desc))
                                            c.Item().Text(desc).FontSize(8).FontColor("#94a3b8");
                                    });
                                    row.AutoItem().AlignBottom().Text(cat.Amount.ToString("N2")).FontSize(11).Bold().FontColor(primaryColor);
                                });
                                list.Item().LineHorizontal(0.5f).LineColor(grayBackground);
                            }
                        });

                        // Summary Section
                        col.Item().PaddingTop(20).AlignRight().Column(sum =>
                        {
                            sum.Spacing(5);
                            sum.Item().MinWidth(250).Row(row =>
                            {
                                row.RelativeItem().Text("Subtotal").FontSize(10).FontColor("#64748b");
                                row.AutoItem().Text($"₹ {receiptData.TotalDueAmount:N2}").FontSize(10).Bold().FontColor(primaryColor);
                            });

                            // Only show discount if it's the Merit Scholarship image case or if we had actual discount data
                            // sum.Item().Row(row => {
                            //     row.RelativeItem().Text("Merit Scholarship Credit (15%)").FontSize(9).Italic().FontColor("#EF4444");
                            //     row.AutoItem().Text("- 3,300.00").FontSize(9).Bold().FontColor("#EF4444");
                            // });

                            sum.Item().PaddingTop(10).Background(primaryColor).Padding(10).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("TOTAL AMOUNT PAID").FontSize(8).Bold().FontColor(Colors.White);
                                    c.Item().Text("Payment Status: Confirmed").FontSize(7).FontColor("#94a3b8");
                                });
                                row.AutoItem().AlignMiddle().Text($"₹ {receiptData.AmountPaid:N2}").FontSize(18).Bold().FontColor(Colors.White);
                            });
                        });

                        // Amount in words
                        col.Item().PaddingTop(40).Text(text =>
                        {
                            text.Span("AMOUNT IN WORDS: ").FontSize(8).FontColor("#64748b");
                            text.Span(AmountInWords((long)receiptData.AmountPaid) + " RUPEES ONLY.").FontSize(9).Bold().Italic().FontColor(primaryColor);
                        });

                        // Legal Footer
                        col.Item().PaddingTop(40).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Width(80).Height(80).Border(0.5f).BorderColor(borderColor).AlignCenter().AlignMiddle().Text("INSTITUTIONAL\nSTAMP\nVALIDATION").FontSize(6).FontColor("#cbd5e1").AlignCenter();
                                c.Item().PaddingTop(5).Text("REGISTRAR SEAL").FontSize(7).Bold().FontColor("#cbd5e1");
                            });

                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().PaddingTop(45).LineHorizontal(0.5f).LineColor("#94a3b8");
                                c.Item().PaddingTop(2).Text("Authorized Signature").FontSize(8).Bold().FontColor(primaryColor);
                            });
                        });
                    });

                    page.Footer().PaddingTop(20).Column(f =>
                    {
                        f.Item().AlignCenter().Text("This is a computer-generated document.").FontSize(7).FontColor("#cbd5e1");
                    });
                })
            ).GeneratePdf();

            return pdf;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to generate PDF", ex);
        }
    }

    private string GetDescriptionForCategory(string category, string feeStructureName)
    {
        if (category.Contains("Tuition")) return $"Standard academic instruction for {feeStructureName}";
        if (category.Contains("Transport")) return "Route-based doorstep pickup and drop-off services";
        if (category.Contains("Activity")) return "Co-curricular activities, lab access and sports";
        if (category.Contains("Learning") || category.Contains("Digital")) return "E-library and student dashboard access";
        return string.Empty;
    }

    private string AmountInWords(long amount)
    {
        if (amount == 0) return "Zero";
        if (amount < 0) return "Minus " + AmountInWords(Math.Abs(amount));

        string words = "";

        if ((amount / 10000000) > 0)
        {
            words += AmountInWords(amount / 10000000) + " Crore ";
            amount %= 10000000;
        }

        if ((amount / 100000) > 0)
        {
            words += AmountInWords(amount / 100000) + " Lakh ";
            amount %= 100000;
        }

        if ((amount / 1000) > 0)
        {
            words += AmountInWords(amount / 1000) + " Thousand ";
            amount %= 1000;
        }

        if ((amount / 100) > 0)
        {
            words += AmountInWords(amount / 100) + " Hundred ";
            amount %= 100;
        }

        if (amount > 0)
        {
            if (words != "") words += "and ";

            string[] unitsMap = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            string[] tensMap = { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            if (amount < 20)
                words += unitsMap[amount];
            else
            {
                words += tensMap[amount / 10];
                if ((amount % 10) > 0)
                    words += "-" + unitsMap[amount % 10];
            }
        }

        return words.ToUpper().Trim();
    }
}

/// <summary>
/// Handler for GenerateStudentFeePdfCommand
/// Generates a professional fee statement/schedule for a student
/// </summary>
public class GenerateStudentFeePdfCommandHandler : IRequestHandler<GenerateStudentFeePdfCommand, byte[]>
{
    private readonly IMediator _mediator;

    public GenerateStudentFeePdfCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<byte[]> Handle(GenerateStudentFeePdfCommand request, CancellationToken cancellationToken)
    {
        var data = await _mediator.Send(new GetStudentFeeStatementDataQuery { StudentFeeId = request.StudentFeeId }, cancellationToken);

        if (data == null)
            throw new InvalidOperationException("Student fee assignment not found");

        try
        {
            // COLORS
            var primaryColor = "#1e293b"; // Slate 800
            var accentColor = "#1D4ED8"; // Royal Blue
            var grayBackground = "#F8FAFC"; // Slate 50
            var borderColor = "#E2E8F0"; // Slate 200

            var pdf = Document.Create(container =>
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Verdana));

                    // Header
                    page.Header().Column(col =>
                    {
                        col.Spacing(10);
                        col.Item().Row(row =>
                        {
                            // Left: School Info
                            row.RelativeItem().Column(coll =>
                            {
                                if (!string.IsNullOrEmpty(data.SchoolLogoBase64))
                                {
                                    try
                                    {
                                        var logoBytes = Convert.FromBase64String(data.SchoolLogoBase64!);
                                        coll.Item().PaddingBottom(10).Height(50).Image(logoBytes).FitHeight();
                                    }
                                    catch { }
                                }

                                coll.Item().Text(data.SchoolName ?? "LORIGIN KIDS GARDEN").FontSize(22).Bold().FontColor(primaryColor);
                                coll.Item().Text(data.SchoolCode ?? "Institutional Excellence Office").FontSize(11).SemiBold().FontColor("#64748b");
                                coll.Item().PaddingTop(4).Text(data.SchoolAddress ?? "").FontSize(8).FontColor("#94a3b8");
                                coll.Item().Text($"Phone: {data.SchoolPhone ?? "N/A"} | Email: {data.SchoolEmail ?? "N/A"}").FontSize(8).FontColor("#94a3b8");
                            });

                            // Right: Statement Badge
                            row.AutoItem().MinWidth(220).Column(rColl =>
                            {
                                rColl.Item().BorderLeft(3).BorderColor(primaryColor).PaddingLeft(10).Column(rCol =>
                                {
                                    rCol.Item().Background(grayBackground).Padding(5).AlignCenter().Text("FEE SCHEDULE / STATEMENT").FontSize(11).Bold().FontColor(primaryColor);
                                    
                                    rCol.Item().PaddingTop(10).AlignRight().Text("DATE GENERATED").FontSize(7).FontColor("#94a3b8");
                                    rCol.Item().AlignRight().Text(DateTime.Now.ToString("dd MMMM, yyyy")).FontSize(9).FontColor(primaryColor);
                                    
                                    rCol.Item().PaddingTop(5).AlignRight().Text("REFERENCE").FontSize(7).FontColor("#94a3b8");
                                    rCol.Item().AlignRight().Text(data.FeeStructureName).FontSize(9).Italic().FontColor(primaryColor);
                                });
                            });
                        });
                    });

                    page.Content().Column(col =>
                    {
                        // Student Details Block
                        col.Item().PaddingVertical(10).Background(grayBackground).Padding(15).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("STUDENT NAME").FontSize(7).FontColor("#64748b");
                                c.Item().Text(data.StudentName).FontSize(11).Bold().FontColor(primaryColor);
                                
                                c.Item().PaddingTop(10).Text("ACADEMIC YEAR").FontSize(7).FontColor("#64748b");
                                c.Item().Text(data.AcademicYear).FontSize(10).FontColor(primaryColor);
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("ENROLLMENT NO.").FontSize(7).FontColor("#64748b");
                                c.Item().Text(data.EnrollmentNumber).FontSize(11).Bold().FontColor(primaryColor);
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("CLASS / SECTION").FontSize(7).FontColor("#64748b");
                                c.Item().Text($"{data.ClassName} - {data.SectionName}").FontSize(11).Bold().FontColor(primaryColor);
                            });
                        });

                        col.Item().PaddingTop(30).Row(row =>
                        {
                            row.RelativeItem().Text("PARTICULARS").FontSize(9).Bold().FontColor("#64748b");
                            row.AutoItem().Text("AMOUNT (INR)").FontSize(9).Bold().FontColor("#64748b");
                        });
                        
                        col.Item().PaddingVertical(5).LineHorizontal(0.5f).LineColor(borderColor);

                        // Items
                        col.Item().Column(list =>
                        {
                            foreach (var cat in data.Categories)
                            {
                                list.Item().PaddingVertical(8).Row(row =>
                                {
                                    row.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text(cat.Category).FontSize(10).Bold().FontColor(primaryColor);
                                        // Specific description mapping based on category name
                                        var desc = GetDescriptionForCategory(cat.Category, data.FeeStructureName);
                                        if (!string.IsNullOrEmpty(desc))
                                            c.Item().Text(desc).FontSize(8).FontColor("#94a3b8");
                                    });
                                    row.AutoItem().AlignBottom().Text(cat.Amount.ToString("N2")).FontSize(11).Bold().FontColor(primaryColor);
                                });
                                list.Item().LineHorizontal(0.5f).LineColor(grayBackground);
                            }
                        });

                        // Summary Section
                        col.Item().PaddingTop(20).AlignRight().Column(sum =>
                        {
                            sum.Spacing(5);
                            sum.Item().MinWidth(280).Row(row =>
                            {
                                row.RelativeItem().Text("Total Structure Amount").FontSize(10).FontColor("#64748b");
                                row.AutoItem().Text($"₹ {data.TotalAmount:N2}").FontSize(10).Bold().FontColor(primaryColor);
                            });

                            sum.Item().MinWidth(280).Row(row =>
                            {
                                row.RelativeItem().Text("Total Amount Paid").FontSize(10).FontColor("#10B981");
                                row.AutoItem().Text($"₹ {data.PaidAmount:N2}").FontSize(10).Bold().FontColor("#10B981");
                            });

                            sum.Item().PaddingTop(10).Background(primaryColor).Padding(10).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("TOTAL REMAINING BALANCE").FontSize(8).Bold().FontColor(Colors.White);
                                    c.Item().Text("Account Status: Active").FontSize(7).FontColor("#94a3b8");
                                });
                                row.AutoItem().AlignMiddle().Text($"₹ {data.BalanceAmount:N2}").FontSize(18).Bold().FontColor(Colors.White);
                            });
                        });

                        // Amount in words
                        col.Item().PaddingTop(40).Text(text =>
                        {
                            text.Span("BALANCE IN WORDS: ").FontSize(8).FontColor("#64748b");
                            text.Span(AmountInWords((long)data.BalanceAmount) + " RUPEES ONLY.").FontSize(9).Bold().Italic().FontColor(primaryColor);
                        });

                        // Legal Footer
                        col.Item().PaddingTop(40).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Width(80).Height(80).Border(0.5f).BorderColor(borderColor).AlignCenter().AlignMiddle().Text("INSTITUTIONAL\nSTAMP\nVALIDATION").FontSize(6).FontColor("#cbd5e1").AlignCenter();
                                c.Item().PaddingTop(5).Text("REGISTRAR SEAL").FontSize(7).Bold().FontColor("#cbd5e1");
                            });

                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().PaddingTop(45).LineHorizontal(0.5f).LineColor("#94a3b8");
                                c.Item().PaddingTop(2).Text("Authorized Signature").FontSize(8).Bold().FontColor(primaryColor);
                            });
                        });
                    });

                    page.Footer().PaddingTop(20).Column(f =>
                    {
                        f.Item().AlignCenter().Text("This is a computer-generated document.").FontSize(7).FontColor("#cbd5e1");
                    });
                })
            ).GeneratePdf();

            return pdf;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to generate Fee Schedule PDF", ex);
        }
    }

    private string GetDescriptionForCategory(string category, string feeStructureName)
    {
        if (category.Contains("Tuition")) return $"Standard academic instruction for {feeStructureName}";
        if (category.Contains("Transport")) return "Route-based doorstep pickup and drop-off services";
        if (category.Contains("Activity")) return "Co-curricular activities, lab access and sports";
        if (category.Contains("Learning") || category.Contains("Digital")) return "E-library and student dashboard access";
        return string.Empty;
    }

    private string AmountInWords(long amount)
    {
        if (amount == 0) return "Zero";
        if (amount < 0) return "Minus " + AmountInWords(Math.Abs(amount));

        string words = "";

        if ((amount / 10000000) > 0)
        {
            words += AmountInWords(amount / 10000000) + " Crore ";
            amount %= 10000000;
        }

        if ((amount / 100000) > 0)
        {
            words += AmountInWords(amount / 100000) + " Lakh ";
            amount %= 100000;
        }

        if ((amount / 1000) > 0)
        {
            words += AmountInWords(amount / 1000) + " Thousand ";
            amount %= 1000;
        }

        if ((amount / 100) > 0)
        {
            words += AmountInWords(amount / 100) + " Hundred ";
            amount %= 100;
        }

        if (amount > 0)
        {
            if (words != "") words += "and ";

            string[] unitsMap = { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            string[] tensMap = { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            if (amount < 20)
                words += unitsMap[amount];
            else
            {
                words += tensMap[amount / 10];
                if ((amount % 10) > 0)
                    words += "-" + unitsMap[amount % 10];
            }
        }

        return words.ToUpper().Trim();
    }
}
