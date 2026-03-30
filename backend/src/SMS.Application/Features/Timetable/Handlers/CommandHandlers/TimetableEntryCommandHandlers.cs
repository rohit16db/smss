using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Domain.Entities;
using SMS.Domain.Enums;
using SMS.Application.Features.Timetable.Commands;

namespace SMS.Application.Features.Timetable.Handlers.CommandHandlers;

public class TimetableEntryCommandHandlers : 
    IRequestHandler<CreateTimetableEntryCommand, Guid>,
    IRequestHandler<UpdateTimetableEntryCommand, bool>,
    IRequestHandler<DeleteTimetableEntryCommand, bool>,
    IRequestHandler<BulkCopyRoutineCommand, BulkCopyResultDto>
{
    private readonly IApplicationDbContext _context;

    public TimetableEntryCommandHandlers(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateTimetableEntryCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _context.StaffAssignments
            .FirstOrDefaultAsync(a => a.Id == request.Entry.StaffAssignmentId, cancellationToken);

        if (assignment == null)
            throw new KeyNotFoundException($"Staff Assignment with ID {request.Entry.StaffAssignmentId} not found");

        if (assignment.AcademicYearId != request.Entry.AcademicYearId)
            throw new InvalidOperationException("Staff Assignment academic year does not match the timetable entry academic year.");

        if (assignment.RemovalDate != null)
            throw new InvalidOperationException("Cannot schedule for a removed staff assignment.");

        // Check for conflicts
        await CheckForConflicts(request.Entry.AcademicYearId, request.Entry.TimeSlotId, 
            assignment.StaffId, assignment.SectionId, request.Entry.RoomNumber, null, cancellationToken);

        var entity = new TimetableEntry
        {
            TimeSlotId = request.Entry.TimeSlotId,
            StaffAssignmentId = request.Entry.StaffAssignmentId,
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

        var assignment = await _context.StaffAssignments
            .FirstOrDefaultAsync(a => a.Id == request.Entry.StaffAssignmentId, cancellationToken);

        if (assignment == null)
            throw new KeyNotFoundException($"Staff Assignment with ID {request.Entry.StaffAssignmentId} not found");

        if (assignment.AcademicYearId != request.Entry.AcademicYearId)
            throw new InvalidOperationException("Staff Assignment academic year does not match the timetable entry academic year.");

        if (assignment.RemovalDate != null)
            throw new InvalidOperationException("Cannot schedule for a removed staff assignment.");

        // Check for conflicts
        await CheckForConflicts(request.Entry.AcademicYearId, request.Entry.TimeSlotId, 
            assignment.StaffId, assignment.SectionId, request.Entry.RoomNumber, request.Id, cancellationToken);

        entity.TimeSlotId = request.Entry.TimeSlotId;
        entity.StaffAssignmentId = request.Entry.StaffAssignmentId;
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

    public async Task<BulkCopyResultDto> Handle(BulkCopyRoutineCommand request, CancellationToken cancellationToken)
    {
        var result = new BulkCopyResultDto();

        // 1. Fetch source entries
        var sourceQuery = _context.TimetableEntries
            .Include(t => t.TimeSlot)
            .Include(t => t.StaffAssignment)
                .ThenInclude(a => a!.Subject)
            .Where(t => t.AcademicYearId == request.AcademicYearId && (int)t.TimeSlot!.DayOfWeek == request.SourceDay);

        if (request.SectionId.HasValue)
            sourceQuery = sourceQuery.Where(t => t.StaffAssignment!.SectionId == request.SectionId.Value);
        
        if (request.StaffId.HasValue)
            sourceQuery = sourceQuery.Where(t => t.StaffAssignment!.StaffId == request.StaffId.Value);

        var sourceEntries = await sourceQuery.ToListAsync(cancellationToken);

        if (!sourceEntries.Any())
        {
            result.Errors.Add("No routine found on the source day to copy.");
            return result;
        }

        // 2. Process Target Days
        foreach (var targetDay in request.TargetDays)
        {
            var dayLabel = ((DayOfWeek)targetDay).ToString();

            foreach (var source in sourceEntries)
            {
                var sourceSlot = source.TimeSlot!;
                
                try 
                {
                    // Find or Create TimeSlot on Target Day
                    var targetSlot = await _context.TimeSlots
                        .FirstOrDefaultAsync(t => t.AcademicYearId == request.AcademicYearId && 
                                               (int)t.DayOfWeek == targetDay &&
                                               t.StartTime == sourceSlot.StartTime &&
                                               t.EndTime == sourceSlot.EndTime, cancellationToken);
                    
                    if (targetSlot == null)
                    {
                        targetSlot = new TimeSlot
                        {
                            Name = sourceSlot.Name,
                            StartTime = sourceSlot.StartTime,
                            EndTime = sourceSlot.EndTime,
                            DayOfWeek = (DayOfWeek)targetDay,
                            IsBreak = sourceSlot.IsBreak,
                            AcademicYearId = request.AcademicYearId
                        };
                        _context.TimeSlots.Add(targetSlot);
                        await _context.SaveChangesAsync(cancellationToken); // Save to get ID
                    }

                    // Check for conflicts
                    await CheckForConflicts(request.AcademicYearId, targetSlot.Id, 
                        source.StaffAssignment!.StaffId, source.StaffAssignment.SectionId, source.RoomNumber, null, cancellationToken);

                    var newEntry = new TimetableEntry
                    {
                        TimeSlotId = targetSlot.Id,
                        StaffAssignmentId = source.StaffAssignmentId,
                        RoomNumber = source.RoomNumber,
                        AcademicYearId = request.AcademicYearId
                    };

                    _context.TimetableEntries.Add(newEntry);
                    result.SuccessCount++;
                }
                catch (InvalidOperationException ex)
                {
                    result.SkippedCount++;
                    result.Errors.Add($"{dayLabel} {sourceSlot.StartTime:hh\\:mm} ({source.StaffAssignment!.Subject?.Name}): {ex.Message}");
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return result;
    }

    private async Task CheckForConflicts(Guid academicYearId, Guid timeSlotId, Guid staffId, Guid sectionId, string? roomNumber, Guid? currentEntryId, CancellationToken cancellationToken)
    {
        // 1. Staff Conflict: Is this staff member already assigned to another class at the same time slot?
        // Note: Join with StaffAssignment to get the StaffId for conflict check
        var staffConflict = await _context.TimetableEntries
            .Include(t => t.StaffAssignment)
            .AnyAsync(t => t.AcademicYearId == academicYearId && 
                          t.TimeSlotId == timeSlotId && 
                          t.StaffAssignment!.StaffId == staffId && 
                          t.Id != currentEntryId, cancellationToken);

        if (staffConflict)
        {
            throw new InvalidOperationException("Staff member is already assigned to another section during this time slot.");
        }

        // 2. Section Conflict: Does this section already have a subject assigned at this time slot?
        var sectionConflict = await _context.TimetableEntries
            .Include(t => t.StaffAssignment)
            .AnyAsync(t => t.AcademicYearId == academicYearId && 
                          t.TimeSlotId == timeSlotId && 
                          t.StaffAssignment!.SectionId == sectionId && 
                          t.Id != currentEntryId, cancellationToken);

        if (sectionConflict)
        {
            throw new InvalidOperationException("This section is already scheduled for another subject during this time slot.");
        }

        // 3. Room Conflict: Is this room already occupied during this time slot?
        if (!string.IsNullOrWhiteSpace(roomNumber))
        {
            var roomConflict = await _context.TimetableEntries
                .AnyAsync(t => t.AcademicYearId == academicYearId && 
                              t.TimeSlotId == timeSlotId && 
                              t.RoomNumber == roomNumber && 
                              t.Id != currentEntryId, cancellationToken);

            if (roomConflict)
            {
                throw new InvalidOperationException($"The room '{roomNumber}' is already occupied during this time slot.");
            }
        }
    }
}
