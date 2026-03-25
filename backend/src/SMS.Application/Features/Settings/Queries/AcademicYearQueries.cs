using MediatR;
using SMS.Application.Features.Settings.DTOs;

namespace SMS.Application.Features.Settings.Queries;

public record GetAcademicYearsQuery : IRequest<List<AcademicYearDto>>;

public record GetActiveAcademicYearQuery : IRequest<AcademicYearDto?>;
