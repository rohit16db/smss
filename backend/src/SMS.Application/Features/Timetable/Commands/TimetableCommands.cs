using MediatR;
using SMS.Application.Features.Timetable.DTOs;
using System.Collections.Generic;

namespace SMS.Application.Features.Timetable.Commands;

public record CreateTimeSlotCommand(CreateTimeSlotDto TimeSlot) : IRequest<Guid>;
public record UpdateTimeSlotCommand(Guid Id, CreateTimeSlotDto TimeSlot) : IRequest<bool>;
public record DeleteTimeSlotCommand(Guid Id) : IRequest<bool>;

public record CreateTimetableEntryCommand(CreateTimetableEntryDto Entry) : IRequest<Guid>;
public record UpdateTimetableEntryCommand(Guid Id, CreateTimetableEntryDto Entry) : IRequest<bool>;
public record DeleteTimetableEntryCommand(Guid Id) : IRequest<bool>;

public record BulkCreateTimeSlotsCommand(Guid AcademicYearId, int SourceDay, List<int> TargetDays) : IRequest<bool>;

public record BulkCopyRoutineCommand(Guid AcademicYearId, Guid? SectionId, Guid? StaffId, int SourceDay, List<int> TargetDays) : IRequest<BulkCopyResultDto>;

public class BulkCopyResultDto
{
    public int SuccessCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}
