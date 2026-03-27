using System;

namespace SMS.Application.Features.Timetable.DTOs;

public class TimetableEntryDto
{
    public Guid Id { get; set; }
    public Guid TimeSlotId { get; set; }
    public string TimeSlotName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public DayOfWeek DayOfWeek { get; set; }

    public Guid SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;

    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;

    public Guid StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;

    public string? RoomNumber { get; set; }
    public Guid AcademicYearId { get; set; }
}

public class CreateTimetableEntryDto
{
    public Guid TimeSlotId { get; set; }
    public Guid SectionId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid StaffId { get; set; }
    public string? RoomNumber { get; set; }
    public Guid AcademicYearId { get; set; }
}
