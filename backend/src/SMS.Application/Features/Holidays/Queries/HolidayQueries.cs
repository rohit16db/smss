using MediatR;
using SMS.Application.Features.Holidays.DTOs;

namespace SMS.Application.Features.Holidays.Queries;

/// <summary>
/// Query to get all holidays with pagination
/// </summary>
public class GetAllHolidaysQuery : IRequest<PaginatedHolidayListDto>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? AcademicYear { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Type { get; set; }
}

/// <summary>
/// Query to get a single holiday by ID
/// </summary>
public class GetHolidayByIdQuery : IRequest<HolidayDto?>
{
    public string Id { get; set; } = string.Empty;
}

/// <summary>
/// Query to get holidays for a specific month
/// </summary>
public class GetHolidaysByMonthQuery : IRequest<List<HolidayDto>>
{
    public int Year { get; set; }
    public int Month { get; set; }
}
