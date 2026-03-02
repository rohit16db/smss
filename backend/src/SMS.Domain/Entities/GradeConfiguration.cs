namespace SMS.Domain.Entities;

/// <summary>
/// Represents the grading scale (A, B, C, D, F) configuration for the school
/// Single Responsibility: Defines grade boundaries and criteria
/// </summary>
public class GradeConfiguration : BaseEntity
{
    public string GradeName { get; set; } = string.Empty;
    public decimal MinPercentage { get; set; }
    public decimal MaxPercentage { get; set; }
    public string? Description { get; set; }
    public Guid SchoolId { get; set; }

    // Navigation properties - Add this when School entity is available
    // public virtual School? School { get; set; }
}
