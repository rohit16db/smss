using System;

namespace SMS.Domain.Entities;

public class TimeSlot : BaseEntity
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Name { get; set; } = string.Empty; // e.g. "Period 1", "Lunch Break"
    public bool IsBreak { get; set; }
    public Guid AcademicYearId { get; set; }
    public AcademicYear? AcademicYear { get; set; }

    public ICollection<TimetableEntry> TimetableEntries { get; set; } = new List<TimetableEntry>();
}
