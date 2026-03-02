using MediatR;
using SMS.Application.Features.Exams.DTOs;

namespace SMS.Application.Features.Exams.Queries;

/// <summary>
/// Query to get all exams with pagination and filtering
/// Single Responsibility: Request exam list with filters
/// </summary>
public class GetExamsQuery : IRequest<PaginatedResult<ExamDto>>
{
    public string? Status { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? SubjectId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string SortBy { get; set; } = "date"; // date, name
    public string SortOrder { get; set; } = "desc"; // asc, desc
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Query to get a specific exam with all details
/// Single Responsibility: Request detailed exam information
/// </summary>
public class GetExamByIdQuery : IRequest<ExamDetailDto>
{
    public Guid ExamId { get; set; }
}

/// <summary>
/// Paginated result wrapper
/// Single Responsibility: Encapsulate paginated data
/// </summary>
public class PaginatedResult<T>
{
    public List<T> Data { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}
