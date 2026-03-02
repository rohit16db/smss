using MediatR;
using SMS.Application.Features.Exams.DTOs;

namespace SMS.Application.Features.Exams.Commands;

/// <summary>
/// Command to configure grade scale
/// Single Responsibility: Request update of grade configuration
/// </summary>
public class ConfigureGradesCommand : IRequest<List<GradeConfigurationDto>>
{
    public List<GradeConfigurationInputDto> Grades { get; set; } = new();
}
