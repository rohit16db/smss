namespace SMS.Domain.Entities;

/// <summary>
/// Represents a class/grade in the school (e.g., Grade 10, Grade 12)
/// A class can have multiple sections
/// </summary>
public class Class : BaseEntity
{
    /// <summary>
    /// Name of the class (e.g., "Grade 10", "Class XII")
    /// </summary>
    public string Name { get; set; } = string.Empty;


    /// <summary>
    /// Whether the class is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Sections within this class
    /// </summary>
    public ICollection<Section> Sections { get; set; } = new List<Section>();

    /// <summary>
    /// Enrollments linked to this class
    /// </summary>
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    /// <summary>
    /// Staff assignments linked to this class
    /// </summary>
    public ICollection<StaffAssignment> StaffAssignments { get; set; } = new List<StaffAssignment>();
}
