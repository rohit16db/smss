using MediatR;
using SMS.Application.Features.Fees.DTOs;

namespace SMS.Application.Features.Fees.Queries;

/// <summary>
/// Query to get a fee structure by ID
/// </summary>
public class GetFeeStructureByIdQuery : IRequest<FeeStructureDto?>
{
    public required string Id { get; set; }
}

/// <summary>
/// Query to get all fee structures with pagination
/// </summary>
public class GetAllFeeStructuresQuery : IRequest<PaginatedFeeStructureListDto>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public int? AcademicYear { get; set; }
}

/// <summary>
/// Query to get active fee structures
/// </summary>
public class GetActiveFeeStructuresQuery : IRequest<List<FeeStructureListDto>>
{
    public int? AcademicYear { get; set; }
}

/// <summary>
/// Query to get student fees by student ID
/// </summary>
public class GetStudentFeesByStudentIdQuery : IRequest<List<StudentFeeDto>>
{
    public required string StudentId { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Query to get all student fees with pagination
/// </summary>
public class GetAllStudentFeesQuery : IRequest<PaginatedStudentFeeListDto>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? StudentId { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Query to get student fee by ID
/// </summary>
public class GetStudentFeeByIdQuery : IRequest<StudentFeeDto?>
{
    public required string Id { get; set; }
}

/// <summary>
/// Query to get fee payments by student fee ID
/// </summary>
public class GetFeePaymentsByStudentFeeIdQuery : IRequest<List<FeePaymentDto>>
{
    public required string StudentFeeId { get; set; }
}
/// <summary>
/// Query to get all fee payments with pagination
/// </summary>
public class GetAllFeePaymentsQuery : IRequest<PaginatedFeePaymentListDto>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? StudentFeeId { get; set; }
}

/// <summary>
/// Query to get student fees by section ID
/// Shows all students in a section and their fee status
/// </summary>
public class GetFeesBySectionQuery : IRequest<List<StudentFeeDto>>
{
    public required string SectionId { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Query to get fee report with payment status and filters
/// </summary>
public class GetFeeReportQuery : IRequest<PaginatedFeeReportListDto>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? StudentId { get; set; }
    public string? SectionId { get; set; }
    public string? Status { get; set; } // "Paid", "Partial", "Due", "Overdue", null = all
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}