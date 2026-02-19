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

    public GetAllHolidaysQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedHolidayListDto> Handle(GetAllHolidaysQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Holidays.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.AcademicYear))
            query = query.Where(h => h.AcademicYear == request.AcademicYear);

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
                AcademicYear = h.AcademicYear
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
            AcademicYear = holiday.AcademicYear
        };
    }
}

/// <summary>
/// Handler for GetHolidaysByMonthQuery
/// </summary>
public class GetHolidaysByMonthQueryHandler : IRequestHandler<GetHolidaysByMonthQuery, List<HolidayDto>>
{
    private readonly IApplicationDbContext _context;

    public GetHolidaysByMonthQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<HolidayDto>> Handle(GetHolidaysByMonthQuery request, CancellationToken cancellationToken)
    {
        var startDate = new DateOnly(request.Year, request.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var holidays = await _context.Holidays
            .Where(h => h.HolidayDate >= startDate && h.HolidayDate <= endDate)
            .OrderBy(h => h.HolidayDate)
            .Select(h => new HolidayDto
            {
                Id = h.Id.ToString(),
                Name = h.Name,
                HolidayDate = h.HolidayDate.ToDateTime(TimeOnly.MinValue),
                Description = h.Description,
                Type = h.Type,
                AcademicYear = h.AcademicYear
            })
            .ToListAsync(cancellationToken);

        return holidays;
    }
}
