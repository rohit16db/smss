namespace SMS.Application.Features.Fees.DTOs;

/// <summary>
/// DTO for fee structure category
/// </summary>
public class FeeStructureCategoryDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

/// <summary>
/// DTO for creating a fee structure
/// </summary>
public class CreateFeeStructureDto
{
    public required string Name { get; set; }
    public required int AcademicYear { get; set; }
    public required string Frequency { get; set; }
    public required List<FeeStructureCategoryDto> Categories { get; set; }
}

/// <summary>
/// DTO for updating a fee structure
/// </summary>
public class UpdateFeeStructureDto
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required int AcademicYear { get; set; }
    public required string Frequency { get; set; }
    public required bool IsActive { get; set; }
    public required List<FeeStructureCategoryDto> Categories { get; set; }
}

/// <summary>
/// DTO for reading fee structure information
/// </summary>
public class FeeStructureDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int AcademicYear { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsActive { get; set; }
    public List<FeeStructureCategoryDto> Categories { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for fee structure list with pagination
/// </summary>
public class FeeStructureListDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int AcademicYear { get; set; }
    public string Frequency { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool IsActive { get; set; }
    public int CategoryCount { get; set; }
}

/// <summary>
/// DTO for paginated fee structure list response
/// </summary>
public class PaginatedFeeStructureListDto
{
    public List<FeeStructureListDto> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}

/// <summary>
/// DTO for assigning fee to student
/// </summary>
public class AssignStudentFeeDto
{
    public required string StudentId { get; set; }
    public required string FeeStructureId { get; set; }
    public required DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// DTO for terminating student fee assignment
/// </summary>
public class TerminateStudentFeeDto
{
    public required DateTime EndDate { get; set; }
}

/// <summary>
/// <summary>
/// DTO for student fee information
/// Includes section context (student's current enrolled section)
/// </summary>
public class StudentFeeDto
{
    public string Id { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string EnrollmentNumber { get; set; } = string.Empty;
    public string FeeStructureId { get; set; } = string.Empty;
    public string FeeStructureName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceAmount { get; set; }
    public bool IsActive { get; set; }
    
    // Section context (read-only - from student's current enrollment)
    public string? SectionId { get; set; }
    public string? SectionName { get; set; }
    
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for paginated student fee list
/// </summary>
public class PaginatedStudentFeeListDto
{
    public List<StudentFeeDto> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}

/// <summary>
/// DTO for recording fee payment
/// </summary>
public class RecordFeePaymentDto
{
    public required string StudentFeeId { get; set; }
    public required decimal AmountPaid { get; set; }
    public required DateTime PaymentDate { get; set; }
    public required string PaymentMethod { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for fee payment information
/// </summary>
public class FeePaymentDto
{
    public string Id { get; set; } = string.Empty;
    public string StudentFeeId { get; set; } = string.Empty;
    public decimal AmountPaid { get; set; }
    public DateTime PaymentDate { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
/// <summary>
/// DTO for paginated fee payment list
/// </summary>
public class PaginatedFeePaymentListDto
{
    public List<FeePaymentDto> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}