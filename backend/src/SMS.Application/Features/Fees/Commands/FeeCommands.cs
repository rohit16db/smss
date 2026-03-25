using MediatR;
using SMS.Application.Features.Fees.DTOs;

namespace SMS.Application.Features.Fees.Commands;

/// <summary>
/// Command to create a new fee structure
/// </summary>
public class CreateFeeStructureCommand : IRequest<FeeStructureDto>
{
    public required string Name { get; set; }
    public required string AcademicYearId { get; set; }
    public required string Frequency { get; set; }
    public required List<FeeStructureCategoryDto> Categories { get; set; }
    public required string CreatedByUserId { get; set; }
}

/// <summary>
/// Command to update a fee structure
/// </summary>
public class UpdateFeeStructureCommand : IRequest<FeeStructureDto>
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string AcademicYearId { get; set; }
    public required string Frequency { get; set; }
    public required bool IsActive { get; set; }
    public required List<FeeStructureCategoryDto> Categories { get; set; }
    public required string UpdatedByUserId { get; set; }
}

/// <summary>
/// Command to delete a fee structure
/// </summary>
public class DeleteFeeStructureCommand : IRequest<bool>
{
    public required string Id { get; set; }
}

/// <summary>
/// Command to assign fee structure to student
/// </summary>
public class AssignStudentFeeCommand : IRequest<StudentFeeDto>
{
    public required string StudentId { get; set; }
    public required string FeeStructureId { get; set; }
    public required DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public required string CreatedByUserId { get; set; }
}

/// <summary>
/// Command to terminate a student fee assignment
/// </summary>
public class TerminateStudentFeeCommand : IRequest<StudentFeeDto>
{
    public required string Id { get; set; }
    public required DateTime EndDate { get; set; }
}

/// <summary>
/// Command to record fee payment
/// </summary>
public class RecordFeePaymentCommand : IRequest<FeePaymentDto>
{
    public required string StudentFeeId { get; set; }
    public required decimal AmountPaid { get; set; }
    public required DateTime PaymentDate { get; set; }
    public required string PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public required string CreatedByUserId { get; set; }
}

/// <summary>
/// Command to bulk assign fee structure to all students in a class/section
/// </summary>
public class BulkAssignStudentFeeCommand : IRequest<BulkAssignmentResultDto>
{
    public required string FeeStructureId { get; set; }
    public required string SectionId { get; set; }
    public required DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public required bool SkipAlreadyAssigned { get; set; }
    public required string CreatedByUserId { get; set; }
}
/// <summary>
/// Command to generate fee receipt PDF
/// </summary>
public class GenerateFeeReceiptPdfCommand : IRequest<byte[]>
{
    public required string PaymentId { get; set; }
}