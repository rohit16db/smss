using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Domain.Entities;
using SMS.Application.Features.Timetable.Commands;

namespace SMS.Application.Features.Timetable.Handlers.CommandHandlers;

public class TimeSlotCommandHandlers : 
    IRequestHandler<CreateTimeSlotCommand, Guid>,
    IRequestHandler<UpdateTimeSlotCommand, bool>,
    IRequestHandler<DeleteTimeSlotCommand, bool>
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
}
