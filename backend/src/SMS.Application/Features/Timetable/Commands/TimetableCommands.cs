using MediatR;
using SMS.Application.Features.Timetable.DTOs;

namespace SMS.Application.Features.Timetable.Commands;

public record CreateTimeSlotCommand(CreateTimeSlotDto TimeSlot) : IRequest<Guid>;
public record UpdateTimeSlotCommand(Guid Id, CreateTimeSlotDto TimeSlot) : IRequest<bool>;
public record DeleteTimeSlotCommand(Guid Id) : IRequest<bool>;

public record CreateTimetableEntryCommand(CreateTimetableEntryDto Entry) : IRequest<Guid>;
public record UpdateTimetableEntryCommand(Guid Id, CreateTimetableEntryDto Entry) : IRequest<bool>;
public record DeleteTimetableEntryCommand(Guid Id) : IRequest<bool>;
