using MediatR;
using Microsoft.EntityFrameworkCore;
using SMS.Application.Common;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Holidays.Commands;
using SMS.Application.Features.Holidays.DTOs;
using SMS.Domain.Entities;

namespace SMS.Application.Features.Holidays.Handlers;

/// <summary>
/// Handler for CreateHolidayCommand
/// </summary>
public class CreateHolidayCommandHandler : IRequestHandler<CreateHolidayCommand, HolidayDto>
{
    private readonly IApplicationDbContext _context;

    public CreateHolidayCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HolidayDto> Handle(CreateHolidayCommand request, CancellationToken cancellationToken)
    {
        var holiday = new Holiday
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            HolidayDate = DateOnly.FromDateTime(request.HolidayDate),
            Description = request.Description,
            Type = request.Type,
            AcademicYear = request.AcademicYear,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Holidays.Add(holiday);
        await _context.SaveChangesAsync(cancellationToken);

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
/// Handler for UpdateHolidayCommand
/// </summary>
public class UpdateHolidayCommandHandler : IRequestHandler<UpdateHolidayCommand, HolidayDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateHolidayCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HolidayDto> Handle(UpdateHolidayCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var holidayId))
            throw new InvalidOperationException($"Invalid holiday ID format: {request.Id}");

        var holiday = await _context.Holidays
            .FirstOrDefaultAsync(h => h.Id == holidayId, cancellationToken);

        if (holiday == null)
            throw new KeyNotFoundException($"Holiday with ID {request.Id} not found");

        holiday.Name = request.Name;
        holiday.HolidayDate = DateOnly.FromDateTime(request.HolidayDate);
        holiday.Description = request.Description;
        holiday.Type = request.Type;
        holiday.AcademicYear = request.AcademicYear;
        holiday.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

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
/// Handler for DeleteHolidayCommand
/// </summary>
public class DeleteHolidayCommandHandler : IRequestHandler<DeleteHolidayCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteHolidayCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteHolidayCommand request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.Id, out var holidayId))
            throw new InvalidOperationException($"Invalid holiday ID format: {request.Id}");

        var holiday = await _context.Holidays
            .FirstOrDefaultAsync(h => h.Id == holidayId, cancellationToken);

        if (holiday == null)
            throw new KeyNotFoundException($"Holiday with ID {request.Id} not found");

        _context.Holidays.Remove(holiday);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
