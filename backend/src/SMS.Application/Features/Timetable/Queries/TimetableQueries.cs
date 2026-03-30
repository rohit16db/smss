using MediatR;
using SMS.Application.Features.Timetable.DTOs;
using System.Collections.Generic;

namespace SMS.Application.Features.Timetable.Queries;

public record GetTimeSlotsQuery(Guid AcademicYearId) : IRequest<List<TimeSlotDto>>;

public record GetSectionTimetableQuery(Guid SectionId, Guid AcademicYearId) : IRequest<List<TimetableEntryDto>>;

public record GetStaffTimetableQuery(Guid StaffId, Guid AcademicYearId) : IRequest<List<TimetableEntryDto>>;
public record GetSectionTimetablePdfQuery(Guid SectionId, Guid AcademicYearId) : IRequest<byte[]>;
public record GetStaffTimetablePdfQuery(Guid StaffId, Guid AcademicYearId) : IRequest<byte[]>;
