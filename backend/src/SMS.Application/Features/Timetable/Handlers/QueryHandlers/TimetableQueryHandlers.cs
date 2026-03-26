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
    IRequestHandler<GetTeacherTimetableQuery, List<TimetableEntryDto>>
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
            .Include(t => t.Subject)
            .Include(t => t.Teacher)
            .Include(t => t.Section)
                .ThenInclude(s => s!.Class)
            .Where(t => t.AcademicYearId == request.AcademicYearId && t.SectionId == request.SectionId)
            .Select(t => new TimetableEntryDto
            {
                Id = t.Id,
                TimeSlotId = t.TimeSlotId,
                TimeSlotName = t.TimeSlot!.Name,
                StartTime = t.TimeSlot.StartTime,
                EndTime = t.TimeSlot.EndTime,
                DayOfWeek = t.TimeSlot.DayOfWeek,
                SectionId = t.SectionId,
                SectionName = t.Section!.SectionName,
                ClassName = t.Section.Class!.Name,
                SubjectId = t.SubjectId,
                SubjectName = t.Subject!.Name,
                TeacherId = t.TeacherId,
                TeacherName = $"{t.Teacher!.FirstName} {t.Teacher.LastName}",
                RoomNumber = t.RoomNumber,
                AcademicYearId = t.AcademicYearId
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TimetableEntryDto>> Handle(GetTeacherTimetableQuery request, CancellationToken cancellationToken)
    {
        return await _context.TimetableEntries
            .Include(t => t.TimeSlot)
            .Include(t => t.Subject)
            .Include(t => t.Teacher)
            .Include(t => t.Section)
                .ThenInclude(s => s!.Class)
            .Where(t => t.AcademicYearId == request.AcademicYearId && t.TeacherId == request.TeacherId)
            .Select(t => new TimetableEntryDto
            {
                Id = t.Id,
                TimeSlotId = t.TimeSlotId,
                TimeSlotName = t.TimeSlot!.Name,
                StartTime = t.TimeSlot.StartTime,
                EndTime = t.TimeSlot.EndTime,
                DayOfWeek = t.TimeSlot.DayOfWeek,
                SectionId = t.SectionId,
                SectionName = t.Section!.SectionName,
                ClassName = t.Section.Class!.Name,
                SubjectId = t.SubjectId,
                SubjectName = t.Subject!.Name,
                TeacherId = t.TeacherId,
                TeacherName = $"{t.Teacher!.FirstName} {t.Teacher.LastName}",
                RoomNumber = t.RoomNumber,
                AcademicYearId = t.AcademicYearId
            })
            .ToListAsync(cancellationToken);
    }
}
