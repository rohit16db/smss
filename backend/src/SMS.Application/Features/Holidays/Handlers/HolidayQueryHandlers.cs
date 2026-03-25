using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Holidays.DTOs;
using SMS.Application.Features.Holidays.Queries;

namespace SMS.Application.Features.Holidays.Handlers;

/// <summary>
/// Handler for GetAllHolidaysQuery
/// </summary>
public class GetAllHolidaysQueryHandler : IRequestHandler<GetAllHolidaysQuery, PaginatedHolidayListDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IAcademicYearContext _academicYearContext;

    public GetAllHolidaysQueryHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
    {
        _context = context;
        _academicYearContext = academicYearContext;
    }

    public async Task<PaginatedHolidayListDto> Handle(GetAllHolidaysQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Holidays.AsQueryable();

        // Apply filters
        query = query.Where(h => h.AcademicYearId == _academicYearContext.RequiredAcademicYearId);

        if (!string.IsNullOrEmpty(request.AcademicYearId) && Guid.TryParse(request.AcademicYearId, out var ayId))
            query = query.Where(h => h.AcademicYearId == ayId);

        if (request.StartDate.HasValue)
        {
            var startDate = DateOnly.FromDateTime(request.StartDate.Value);
            query = query.Where(h => h.HolidayDate >= startDate);
        }

        if (request.EndDate.HasValue)
        {
            var endDate = DateOnly.FromDateTime(request.EndDate.Value);
            query = query.Where(h => h.HolidayDate <= endDate);
        }

        if (!string.IsNullOrEmpty(request.Type))
            query = query.Where(h => h.Type == request.Type);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var holidays = await query
            .OrderBy(h => h.HolidayDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(h => new HolidayDto
            {
                Id = h.Id.ToString(),
                Name = h.Name,
                HolidayDate = h.HolidayDate.ToDateTime(TimeOnly.MinValue),
                Description = h.Description,
                Type = h.Type,
                AcademicYearId = h.AcademicYearId.ToString(),
                AcademicYearName = h.AcademicYear != null ? h.AcademicYear.Name : null
            })
            .ToListAsync(cancellationToken);

        return new PaginatedHolidayListDto
        {
            Items = holidays,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

/// <summary>
/// Handler for GetHolidayByIdQuery
/// </summary>
public class GetHolidayByIdQueryHandler : IRequestHandler<GetHolidayByIdQuery, HolidayDto?>
{
    private readonly IApplicationDbContext _context;

    public GetHolidayByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HolidayDto?> Handle(GetHolidayByIdQuery request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var holidayId))
            return null;

        var holiday = await _context.Holidays
            .FirstOrDefaultAsync(h => h.Id == holidayId, cancellationToken);

        if (holiday == null)
            return null;

        return new HolidayDto
        {
            Id = holiday.Id.ToString(),
            Name = holiday.Name,
            HolidayDate = holiday.HolidayDate.ToDateTime(TimeOnly.MinValue),
            Description = holiday.Description,
            Type = holiday.Type,
            AcademicYearId = holiday.AcademicYearId.ToString(),
            AcademicYearName = holiday.AcademicYear != null ? holiday.AcademicYear.Name : null
        };
    }
}

/// <summary>
/// Handler for GetHolidaysByMonthQuery
/// </summary>
public class GetHolidaysByMonthQueryHandler : IRequestHandler<GetHolidaysByMonthQuery, List<HolidayDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IAcademicYearContext _academicYearContext;

    public GetHolidaysByMonthQueryHandler(IApplicationDbContext context, IAcademicYearContext academicYearContext)
    {
        _context = context;
        _academicYearContext = academicYearContext;
    }

    public async Task<List<HolidayDto>> Handle(GetHolidaysByMonthQuery request, CancellationToken cancellationToken)
    {
        var startDate = new DateOnly(request.Year, request.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var holidays = await _context.Holidays
            .Where(h => h.AcademicYearId == _academicYearContext.RequiredAcademicYearId &&
                        h.HolidayDate >= startDate && h.HolidayDate <= endDate)
            .OrderBy(h => h.HolidayDate)
            .Select(h => new HolidayDto
            {
                Id = h.Id.ToString(),
                Name = h.Name,
                HolidayDate = h.HolidayDate.ToDateTime(TimeOnly.MinValue),
                Description = h.Description,
                Type = h.Type,
                AcademicYearId = h.AcademicYearId.ToString(),
                AcademicYearName = h.AcademicYear != null ? h.AcademicYear.Name : null
            })
            .ToListAsync(cancellationToken);

        return holidays;
    }
}
