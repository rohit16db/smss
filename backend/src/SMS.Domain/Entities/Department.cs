namespace SMS.Domain.Entities;

/// <summary>
/// Represents a structured division within the school (e.g. Science Faculty, Administration, Transport)
/// </summary>
public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    /// <summary>Optional FK to the Staff member heading this department</summary>
    public Guid? HeadOfDepartmentId { get; set; }
    public Staff? HeadOfDepartment { get; set; }
    
    public ICollection<Staff> StaffMembers { get; set; } = new List<Staff>();
}
