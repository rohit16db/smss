namespace SMS.Domain.Entities;

/// <summary>
/// Represents a holiday in the school calendar
/// </summary>
public class Holiday : BaseEntity
{
    /// <summary>
    /// Name of the holiday
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Date of the holiday
    /// </summary>
    public DateOnly HolidayDate { get; set; }

    /// <summary>
    /// Optional description of the holiday
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Type/Category of holiday (e.g., National, Religious, School Event)
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Academic year for the holiday (e.g., "2025-2026")
    /// </summary>
    public string AcademicYear { get; set; } = string.Empty;
}
