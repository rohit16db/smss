using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Fees.Commands;
using SMS.Application.Features.Fees.DTOs;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Fees.Handlers.CommandHandlers;

/// <summary>
/// Handler for CreateFeeStructureCommand
/// </summary>
public class CreateFeeStructureCommandHandler : IRequestHandler<CreateFeeStructureCommand, FeeStructureDto>
{
    private readonly IApplicationDbContext _context;

    public CreateFeeStructureCommandHandler(IApplicationDbContext context)
    {
        _context = context;
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
            AcademicYear = request.AcademicYear,
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
            AcademicYear = feeStructure.AcademicYear,
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

    public UpdateFeeStructureCommandHandler(IApplicationDbContext context)
    {
        _context = context;
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
        feeStructure.AcademicYear = request.AcademicYear;
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
            AcademicYear = feeStructure.AcademicYear,
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

        var studentFee = new StudentFee
        {
            StudentId = studentId,
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
            StudentId = studentFee.StudentId.ToString(),
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
            .Include(sf => sf.Student)
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
            StudentId = studentFee.StudentId.ToString(),
            StudentName = studentFee.Student != null ? $"{studentFee.Student.FirstName} {studentFee.Student.LastName}" : "Unknown",
            EnrollmentNumber = studentFee.Student?.EnrollmentNumber ?? "Unknown",
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

        // Generate unique receipt number
        var receiptNumber = $"RCP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";

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
