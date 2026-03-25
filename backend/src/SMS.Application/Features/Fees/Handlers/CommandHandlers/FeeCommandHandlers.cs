using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Fees.Commands;
using SMS.Application.Features.Fees.DTOs;
using SMS.Application.Features.Fees.Queries;
using SMS.Domain.Entities;

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
            TotalAmount = feeStructure.TotalAmount,
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
/// Handler for TerminateStudentFeeCommand
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
                    TotalAmount = feeStructure.TotalAmount,
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
                    page.Size(595, 842); // A4 size in points
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Professional Header
                    page.Header().Column(col =>
                    {
                        col.Spacing(12);
                        
                        // School Header - Centered
                        col.Item().AlignCenter().Column(header =>
                        {
                            header.Spacing(2);
                            header.Item().Text(receiptData.SchoolName ?? "School Management System")
                                .FontSize(16).Bold();
                            if (!string.IsNullOrEmpty(receiptData.SchoolAddress))
                                header.Item().Text(receiptData.SchoolAddress).FontSize(9);
                            if (!string.IsNullOrEmpty(receiptData.SchoolPhone))
                                header.Item().Text($"Phone: {receiptData.SchoolPhone}").FontSize(8);
                        });
                        
                        // Divider line
                        col.Item().BorderBottom(2);
                        
                        // Receipt title and number - Centered
                        col.Item().PaddingVertical(8).AlignCenter().Column(title =>
                        {
                            title.Spacing(3);
                            title.Item().Text("FEE RECEIPT").FontSize(16).Bold();
                            title.Item().Text(receiptData.ReceiptNumber).FontSize(12).Bold();
                        });
                        
                        // Divider line
                        col.Item().BorderBottom(2);
                    });

                    page.Content().Column(col =>
                    {
                        col.Spacing(14);

                        // Student Information Section
                        col.Item().Column(section =>
                        {
                            section.Spacing(4);
                            section.Item().Text("STUDENT INFORMATION").FontSize(11).Bold();
                            section.Item().PaddingVertical(2).BorderBottom(1);
                            
                            section.Item().PaddingVertical(6).Row(row =>
                            {
                                row.RelativeItem(1).Column(col1 =>
                                {
                                    col1.Item().Text("Name :").FontSize(8);
                                    col1.Item().Text(receiptData.StudentName).FontSize(11).Bold();
                                });
                                row.RelativeItem(1).Column(col2 =>
                                {
                                    col2.Item().AlignRight().Text("Enrollment # :").FontSize(8);
                                    col2.Item().AlignRight().Text(receiptData.EnrollmentNumber).FontSize(11).Bold();
                                });
                            });
                            
                            section.Item().PaddingVertical(4).Row(row =>
                            {
                                row.RelativeItem(1).Column(col1 =>
                                {
                                    col1.Item().Text("Class :").FontSize(8);
                                    col1.Item().Text(receiptData.ClassName).FontSize(10);
                                });
                                row.RelativeItem(1).Column(col2 =>
                                {
                                    col2.Item().AlignRight().Text("Section :").FontSize(8);
                                    col2.Item().AlignRight().Text(receiptData.SectionName).FontSize(10);
                                });
                            });
                            
                            section.Item().PaddingVertical(4).Row(row =>
                            {
                                row.RelativeItem().Column(col1 =>
                                {
                                    col1.Item().Text("Fee Type :").FontSize(8);
                                    col1.Item().Text(receiptData.FeeStructureName).FontSize(10).Bold();
                                });
                            });
                        });

                        // Payment Details Section
                        col.Item().Column(section =>
                        {
                            section.Spacing(4);
                            section.Item().Text("PAYMENT DETAILS").FontSize(11).Bold();
                            section.Item().PaddingVertical(2).BorderBottom(1);
                            
                            section.Item().PaddingVertical(10).Column(details =>
                            {
                                details.Spacing(8);
                                
                                // Previous Balance
                                details.Item().Row(row =>
                                {
                                    row.RelativeItem(1).Text("Previous Balance :").FontSize(9);
                                    row.RelativeItem(1).AlignRight().Text($"₹ {receiptData.PreviousBalance:N2}").FontSize(10);
                                });
                                
                                // Amount Paid - Emphasized with larger font
                                details.Item().PaddingHorizontal(8).PaddingVertical(8).Row(row =>
                                {
                                    row.RelativeItem(1).Text("Amount Paid :").FontSize(11).Bold();
                                    row.RelativeItem(1).AlignRight().Text($"₹ {receiptData.AmountPaid:N2}").FontSize(13).Bold();
                                });
                                
                                // Current Balance - Emphasized with larger font
                                details.Item().PaddingHorizontal(8).PaddingVertical(8).Row(row =>
                                {
                                    row.RelativeItem(1).Text("Current Balance :").FontSize(11).Bold();
                                    row.RelativeItem(1).AlignRight().Text($"₹ {receiptData.CurrentBalance:N2}").FontSize(13).Bold();
                                });
                                
                                // Total Due
                                details.Item().Row(row =>
                                {
                                    row.RelativeItem(1).Text("Total Due Amount :").FontSize(9);
                                    row.RelativeItem(1).AlignRight().Text($"₹ {receiptData.TotalDueAmount:N2}").FontSize(10);
                                });
                            });
                        });

                        // Transaction Details
                        col.Item().Column(section =>
                        {
                            section.Spacing(4);
                            section.Item().Text("TRANSACTION DETAILS").FontSize(11).Bold();
                            section.Item().PaddingVertical(2).BorderBottom(1);
                            
                            section.Item().PaddingVertical(6).Row(row =>
                            {
                                row.RelativeItem(1).Column(col1 =>
                                {
                                    col1.Item().Text("Payment Date :").FontSize(8);
                                    col1.Item().Text($"{receiptData.PaymentDate:dd/MM/yyyy}").FontSize(10).Bold();
                                });
                                row.RelativeItem(1).Column(col2 =>
                                {
                                    col2.Item().AlignRight().Text("Payment Method :").FontSize(8);
                                    col2.Item().AlignRight().Text(receiptData.PaymentMethod).FontSize(10).Bold();
                                });
                            });
                        });

                        if (!string.IsNullOrEmpty(receiptData.Notes))
                        {
                            col.Item().Column(section =>
                            {
                                section.Spacing(4);
                                section.Item().Text("NOTES").FontSize(11).Bold();
                                section.Item().PaddingVertical(2).BorderBottom(1);
                                section.Item().PaddingVertical(5).Text(receiptData.Notes).FontSize(9);
                            });
                        }

                        // Footer spacer
                        col.Item().Height(20);
                        col.Item().BorderTop(1);
                        
                        // Footer
                        col.Item().PaddingVertical(10).AlignCenter().Column(footer =>
                        {
                            footer.Spacing(3);
                            footer.Item().Text("Thank You for the Payment!").Bold().FontSize(11);
                            footer.Item().Text("This is an electronically generated receipt").FontSize(8);
                            footer.Item().Text($"Generated on: {DateTime.Now:dd/MM/yyyy HH:mm:ss}").FontSize(8);
                        });
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
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
}
