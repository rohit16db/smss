namespace SMS.Application.Features.Exams.DTOs;

/// <summary>
/// DTO for grade configuration
/// Single Responsibility: Transfer grade configuration data
/// </summary>
public class GradeConfigurationDto
{
    public Guid Id { get; set; }
    public string GradeName { get; set; } = string.Empty;
    public decimal MinPercentage { get; set; }
    public decimal MaxPercentage { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating grade configuration
/// Single Responsibility: Transfer grade update request
/// </summary>
public class UpdateGradeConfigurationDto
{
    public List<GradeConfigurationInputDto> Grades { get; set; } = new();
}

public class GradeConfigurationInputDto
{
    public string Name { get; set; } = string.Empty;
    public decimal MinPercentage { get; set; }
    public decimal MaxPercentage { get; set; }
    public string Description { get; set; } = string.Empty;
}
