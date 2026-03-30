using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Timetable.DTOs;
using SMS.Application.Features.Timetable.Queries;
using System.Collections.Generic;
using System.Linq;

namespace SMS.Application.Features.Timetable.Handlers.QueryHandlers;

public class TimetableQueryHandlers : 
    IRequestHandler<GetTimeSlotsQuery, List<TimeSlotDto>>,
    IRequestHandler<GetSectionTimetableQuery, List<TimetableEntryDto>>,
    IRequestHandler<GetStaffTimetableQuery, List<TimetableEntryDto>>
{
    private readonly IApplicationDbContext _context;

    public TimetableQueryHandlers(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<TimeSlotDto>> Handle(GetTimeSlotsQuery request, CancellationToken cancellationToken)
    {
        return await _context.TimeSlots
            .Where(t => t.AcademicYearId == request.AcademicYearId)
            .OrderBy(t => t.DayOfWeek)
            .ThenBy(t => t.StartTime)
            .Select(t => new TimeSlotDto
            {
                Id = t.Id,
                DayOfWeek = t.DayOfWeek,
                StartTime = t.StartTime,
                EndTime = t.EndTime,
                Name = t.Name,
                IsBreak = t.IsBreak,
                AcademicYearId = t.AcademicYearId
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TimetableEntryDto>> Handle(GetSectionTimetableQuery request, CancellationToken cancellationToken)
    {
        return await _context.TimetableEntries
            .Include(t => t.TimeSlot)
            .Include(t => t.StaffAssignment)
                .ThenInclude(a => a!.Staff)
                    .ThenInclude(s => s.UserProfile)
            .Include(t => t.StaffAssignment)
                .ThenInclude(a => a!.Subject)
            .Include(t => t.StaffAssignment)
                .ThenInclude(a => a!.Section)
            .Include(t => t.StaffAssignment)
                .ThenInclude(a => a!.Class)
            .Where(t => t.AcademicYearId == request.AcademicYearId && t.StaffAssignment!.SectionId == request.SectionId)
            .Select(t => new TimetableEntryDto
            {
                Id = t.Id,
                TimeSlotId = t.TimeSlotId,
                TimeSlotName = t.TimeSlot!.Name,
                StartTime = t.TimeSlot.StartTime,
                EndTime = t.TimeSlot.EndTime,
                DayOfWeek = t.TimeSlot.DayOfWeek,
                SectionId = t.StaffAssignment!.SectionId,
                SectionName = t.StaffAssignment.Section!.SectionName,
                ClassId = t.StaffAssignment.ClassId,
                ClassName = t.StaffAssignment.Class!.Name,
                StaffAssignmentId = t.StaffAssignmentId,
                SubjectId = t.StaffAssignment!.SubjectId,
                SubjectName = t.StaffAssignment.Subject.Name,
                StaffId = t.StaffAssignment!.StaffId,
                StaffName = t.StaffAssignment.Staff!.FullName,
                RoomNumber = t.RoomNumber,
                AcademicYearId = t.AcademicYearId
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TimetableEntryDto>> Handle(GetStaffTimetableQuery request, CancellationToken cancellationToken)
    {
        return await _context.TimetableEntries
            .Include(t => t.TimeSlot)
            .Include(t => t.StaffAssignment)
                .ThenInclude(a => a!.Staff)
                    .ThenInclude(s => s.UserProfile)
            .Include(t => t.StaffAssignment)
                .ThenInclude(a => a!.Subject)
            .Include(t => t.StaffAssignment)
                .ThenInclude(a => a!.Section)
            .Include(t => t.StaffAssignment)
                .ThenInclude(a => a!.Class)
            .Where(t => t.AcademicYearId == request.AcademicYearId && t.StaffAssignment!.StaffId == request.StaffId)
            .Select(t => new TimetableEntryDto
            {
                Id = t.Id,
                TimeSlotId = t.TimeSlotId,
                TimeSlotName = t.TimeSlot!.Name,
                StartTime = t.TimeSlot.StartTime,
                EndTime = t.TimeSlot.EndTime,
                DayOfWeek = t.TimeSlot.DayOfWeek,
                SectionId = t.StaffAssignment!.SectionId,
                SectionName = t.StaffAssignment.Section!.SectionName,
                ClassId = t.StaffAssignment.ClassId,
                ClassName = t.StaffAssignment.Class!.Name,
                StaffAssignmentId = t.StaffAssignmentId,
                SubjectId = t.StaffAssignment!.SubjectId,
                SubjectName = t.StaffAssignment.Subject.Name,
                StaffId = t.StaffAssignment!.StaffId,
                StaffName = t.StaffAssignment.Staff!.FullName,
                RoomNumber = t.RoomNumber,
                AcademicYearId = t.AcademicYearId
            })
            .ToListAsync(cancellationToken);
    }
}
