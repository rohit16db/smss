using MediatR;
using SMS.Application.Features.Exams.DTOs;

namespace SMS.Application.Features.Exams.Queries;

/// <summary>
/// Query to get grade configuration
/// Single Responsibility: Request current grade configuration
/// </summary>
public class GetGradeConfigurationQuery : IRequest<List<GradeConfigurationDto>>
{
}
