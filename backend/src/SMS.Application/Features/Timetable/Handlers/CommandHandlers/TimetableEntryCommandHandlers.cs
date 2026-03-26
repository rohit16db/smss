using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Domain.Entities;
using SMS.Application.Features.Timetable.Commands;

namespace SMS.Application.Features.Timetable.Handlers.CommandHandlers;

public class TimetableEntryCommandHandlers : 
    IRequestHandler<CreateTimetableEntryCommand, Guid>,
    IRequestHandler<UpdateTimetableEntryCommand, bool>,
    IRequestHandler<DeleteTimetableEntryCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public TimetableEntryCommandHandlers(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateTimetableEntryCommand request, CancellationToken cancellationToken)
    {
        // Check for conflicts
        await CheckForConflicts(request.Entry.AcademicYearId, request.Entry.TimeSlotId, 
            request.Entry.TeacherId, request.Entry.SectionId, null, cancellationToken);

        var entity = new TimetableEntry
        {
            TimeSlotId = request.Entry.TimeSlotId,
            SectionId = request.Entry.SectionId,
            SubjectId = request.Entry.SubjectId,
            TeacherId = request.Entry.TeacherId,
            RoomNumber = request.Entry.RoomNumber,
            AcademicYearId = request.Entry.AcademicYearId
        };

        _context.TimetableEntries.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }

    public async Task<bool> Handle(UpdateTimetableEntryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TimetableEntries
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (entity == null) return false;

        // Check for conflicts
        await CheckForConflicts(request.Entry.AcademicYearId, request.Entry.TimeSlotId, 
            request.Entry.TeacherId, request.Entry.SectionId, request.Id, cancellationToken);

        entity.TimeSlotId = request.Entry.TimeSlotId;
        entity.SectionId = request.Entry.SectionId;
        entity.SubjectId = request.Entry.SubjectId;
        entity.TeacherId = request.Entry.TeacherId;
        entity.RoomNumber = request.Entry.RoomNumber;
        entity.AcademicYearId = request.Entry.AcademicYearId;

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Handle(DeleteTimetableEntryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TimetableEntries
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);

        if (entity == null) return false;

        _context.TimetableEntries.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task CheckForConflicts(Guid academicYearId, Guid timeSlotId, Guid teacherId, Guid sectionId, Guid? currentEntryId, CancellationToken cancellationToken)
    {
        // 1. Teacher Conflict: Is this teacher already assigned to another class at the same time slot?
        var teacherConflict = await _context.TimetableEntries
            .AnyAsync(t => t.AcademicYearId == academicYearId && 
                          t.TimeSlotId == timeSlotId && 
                          t.TeacherId == teacherId && 
                          t.Id != currentEntryId, cancellationToken);

        if (teacherConflict)
        {
            throw new InvalidOperationException("Teacher is already assigned to another section during this time slot.");
        }

        // 2. Section Conflict: Does this section already have a subject assigned at this time slot?
        var sectionConflict = await _context.TimetableEntries
            .AnyAsync(t => t.AcademicYearId == academicYearId && 
                          t.TimeSlotId == timeSlotId && 
                          t.SectionId == sectionId && 
                          t.Id != currentEntryId, cancellationToken);

        if (sectionConflict)
        {
            throw new InvalidOperationException("This section is already scheduled for another subject during this time slot.");
        }
    }
}
