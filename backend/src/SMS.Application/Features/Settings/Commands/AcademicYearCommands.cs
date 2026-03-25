using MediatR;
using SMS.Application.Features.Settings.DTOs;

namespace SMS.Application.Features.Settings.Commands;

public record CreateAcademicYearCommand : IRequest<AcademicYearDto>
{
    public string Name { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsActive { get; init; } = true;
}

public record ToggleAcademicYearStatusCommand : IRequest<bool>
{
    public Guid Id { get; init; }
}
