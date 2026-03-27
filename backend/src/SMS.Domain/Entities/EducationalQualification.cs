namespace SMS.Domain.Entities;

/// <summary>
/// Represents an educational degree or certification achieved by a staff member.
/// Enables 1:N tracking of multiple qualifications per staff.
/// </summary>
public class EducationalQualification : BaseEntity
{
    public Guid StaffId { get; set; }
    public Staff Staff { get; set; } = null!;
    
    /// <summary>e.g., "B.Sc. Mathematics", "M.Ed."</summary>
    public string DegreeName { get; set; } = string.Empty;
    
    /// <summary>e.g., "Harvard University"</summary>
    public string Institution { get; set; } = string.Empty;
    
    public int YearOfPassing { get; set; }
    
    public string? GradeOrPercentage { get; set; }
}
