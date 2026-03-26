using System;

namespace SMS.Application.Features.Timetable.DTOs;

public class TimeSlotDto
{
    public Guid Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsBreak { get; set; }
    public Guid AcademicYearId { get; set; }
}

public class CreateTimeSlotDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsBreak { get; set; }
    public Guid AcademicYearId { get; set; }
}
