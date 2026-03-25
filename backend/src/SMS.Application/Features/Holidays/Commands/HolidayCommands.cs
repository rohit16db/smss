using MediatR;
using SMS.Application.Features.Holidays.DTOs;

namespace SMS.Application.Features.Holidays.Commands;

/// <summary>
/// Command to create a new holiday
/// </summary>
public class CreateHolidayCommand : IRequest<HolidayDto>
{
    public string Name { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string AcademicYearId { get; set; } = string.Empty;
}

/// <summary>
/// Command to update an existing holiday
/// </summary>
public class UpdateHolidayCommand : IRequest<HolidayDto>
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public string? Description { get; set; }
    public string? Type { get; set; }
    public string AcademicYearId { get; set; } = string.Empty;
}

/// <summary>
/// Command to delete a holiday
/// </summary>
public class DeleteHolidayCommand : IRequest<Unit>
{
    public string Id { get; set; } = string.Empty;
}
