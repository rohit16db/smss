using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Domain.Entities;
using SMS.Application.Features.Timetable.Commands;

namespace SMS.Application.Features.Timetable.Handlers.CommandHandlers;

public class TimeSlotCommandHandlers : 
    IRequestHandler<CreateTimeSlotCommand, Guid>,
    IRequestHandler<UpdateTimeSlotCommand, bool>,
    IRequestHandler<DeleteTimeSlotCommand, bool>,
    IRequestHandler<BulkCreateTimeSlotsCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public TimeSlotCommandHandlers(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateTimeSlotCommand request, CancellationToken cancellationToken)
    {
        var entity = new TimeSlot
        {
            DayOfWeek = request.TimeSlot.DayOfWeek,
            StartTime = request.TimeSlot.StartTime,
            EndTime = request.TimeSlot.EndTime,
            Name = request.TimeSlot.Name,
            IsBreak = request.TimeSlot.IsBreak,
            AcademicYearId = request.TimeSlot.AcademicYearId
        };

        await ValidateTimeSlot(entity, null, cancellationToken);

        _context.TimeSlots.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task<bool> Handle(UpdateTimeSlotCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TimeSlots
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (entity == null) return false;

        entity.DayOfWeek = request.TimeSlot.DayOfWeek;
        entity.StartTime = request.TimeSlot.StartTime;
        entity.EndTime = request.TimeSlot.EndTime;
        entity.Name = request.TimeSlot.Name;
        entity.IsBreak = request.TimeSlot.IsBreak;
        entity.AcademicYearId = request.TimeSlot.AcademicYearId;

        await ValidateTimeSlot(entity, request.Id, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(BulkCreateTimeSlotsCommand request, CancellationToken cancellationToken)
    {
        var sourceSlots = await _context.TimeSlots
            .Where(t => t.AcademicYearId == request.AcademicYearId && (int)t.DayOfWeek == request.SourceDay)
            .ToListAsync(cancellationToken);

        if (!sourceSlots.Any()) return false;

        foreach (var targetDay in request.TargetDays)
        {
            foreach (var source in sourceSlots)
            {
                var overlapping = await _context.TimeSlots.AnyAsync(t => 
                    t.AcademicYearId == request.AcademicYearId && 
                    (int)t.DayOfWeek == targetDay &&
                    t.StartTime < source.EndTime && 
                    source.StartTime < t.EndTime, cancellationToken);

                if (overlapping)
                {
                    // If it's an exact match (same start/end), we skip it as it's already "synced"
                    var exactMatch = await _context.TimeSlots.AnyAsync(t => 
                        t.AcademicYearId == request.AcademicYearId && 
                        (int)t.DayOfWeek == targetDay &&
                        t.StartTime == source.StartTime &&
                        t.EndTime == source.EndTime, cancellationToken);

                    if (!exactMatch)
                    {
                        throw new InvalidOperationException($"Cannot sync structure: {source.Name} overlaps with an existing slot on {((DayOfWeek)targetDay)}.");
                    }
                    continue;
                }

                var newSlot = new TimeSlot
                {
                    Name = source.Name,
                    StartTime = source.StartTime,
                    EndTime = source.EndTime,
                    IsBreak = source.IsBreak,
                    DayOfWeek = (DayOfWeek)targetDay,
                    AcademicYearId = request.AcademicYearId
                };
                _context.TimeSlots.Add(newSlot);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(DeleteTimeSlotCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TimeSlots
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (entity == null) return false;

        _context.TimeSlots.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task ValidateTimeSlot(TimeSlot slot, Guid? currentId, CancellationToken cancellationToken)
    {
        if (slot.StartTime >= slot.EndTime)
        {
            throw new InvalidOperationException("Start time must be before end time.");
        }

        var overlapping = await _context.TimeSlots
            .AnyAsync(t => t.AcademicYearId == slot.AcademicYearId &&
                           t.DayOfWeek == slot.DayOfWeek &&
                           t.Id != currentId &&
                           t.StartTime < slot.EndTime && 
                           slot.StartTime < t.EndTime, cancellationToken);

        if (overlapping)
        {
            throw new InvalidOperationException("This time slot overlaps with an existing one on the same day.");
        }
    }
}
