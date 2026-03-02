using MediatR;
using SMS.Application.Common.Interfaces;
using SMS.Application.Features.Exams.Commands;
using SMS.Application.Features.Exams.DTOs;

namespace SMS.Application.Features.Exams.Handlers;

public class GradeCommandHandlers
{
    public class ConfigureGradesCommandHandler : IRequestHandler<ConfigureGradesCommand, List<GradeConfigurationDto>>
    {
        private readonly IApplicationDbContext _context;
        public ConfigureGradesCommandHandler(IApplicationDbContext context) => _context = context;
        
        public async Task<List<GradeConfigurationDto>> Handle(ConfigureGradesCommand request, CancellationToken cancellationToken)
        {
            // Grade configuration is typically stored in application settings or constants
            // For now, returning standard grade configuration
            var gradeConfiguration = new List<GradeConfigurationDto>
            {
                new() { GradeName = "A+", MinPercentage = 90, MaxPercentage = 100, Description = "Excellent" },
                new() { GradeName = "A", MinPercentage = 80, MaxPercentage = 89, Description = "Very Good" },
                new() { GradeName = "B+", MinPercentage = 70, MaxPercentage = 79, Description = "Good" },
                new() { GradeName = "B", MinPercentage = 60, MaxPercentage = 69, Description = "Fair" },
                new() { GradeName = "C", MinPercentage = 50, MaxPercentage = 59, Description = "Satisfactory" },
                new() { GradeName = "D", MinPercentage = 40, MaxPercentage = 49, Description = "Poor" },
                new() { GradeName = "F", MinPercentage = 0, MaxPercentage = 39, Description = "Fail" }
            };

            return await Task.FromResult(gradeConfiguration);
        }
    }
}
