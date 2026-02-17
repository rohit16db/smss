namespace SMS.Domain.Entities;

/// <summary>
/// Represents a section/division within a class
/// Students are enrolled in a specific section of a class
/// </summary>
public class Section : BaseEntity
{
    /// <summary>
    /// Class ID this section belongs to
    /// </summary>
    public Guid ClassId { get; set; }

    /// <summary>
    /// Section name/identifier (e.g., "A", "B", "Section Alpha")
    /// </summary>
    public string SectionName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the section is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property to the parent class
    /// </summary>
    public Class? Class { get; set; }

    /// <summary>
    /// Students enrolled in this section
    /// </summary>
    public ICollection<StudentSection> StudentSections { get; set; } = new List<StudentSection>();
}
