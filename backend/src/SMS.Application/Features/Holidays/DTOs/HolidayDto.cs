namespace SMS.Application.Features.Holidays.DTOs;

/// <summary>
/// DTO for Holiday details
/// </summary>
public class HolidayDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
}

/// <summary>
/// DTO for creating a holiday
/// </summary>
public class CreateHolidayDto
{
    public string Name { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating a holiday
/// </summary>
public class UpdateHolidayDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
}

/// <summary>
/// DTO for paginated holiday list
/// </summary>
public class PaginatedHolidayListDto
{
    public List<HolidayDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
