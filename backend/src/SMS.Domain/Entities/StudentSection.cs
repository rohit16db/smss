namespace SMS.Domain.Entities;

/// <summary>
/// Represents the enrollment of a student in a specific section
/// Tracks section history - when a student moves to a different section
/// </summary>
public class StudentSection : BaseEntity
{
    /// <summary>
    /// Student ID
    /// </summary>
    public Guid StudentId { get; set; }

    /// <summary>
    /// Section ID
    /// </summary>
    public Guid SectionId { get; set; }

    /// <summary>
    /// Date when student joined this section
    /// </summary>
    public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when student left this section (null if currently enrolled)
    /// </summary>
    public DateTime? LeftDate { get; set; }

    /// <summary>
    /// Flag indicating if this is the student's current section
    /// </summary>
    public bool IsCurrent { get; set; } = true;

    /// <summary>
    /// Navigation property to the section
    /// </summary>
    public Section? Section { get; set; }

    /// <summary>
    /// Navigation property to the student
    /// </summary>
    public Student? Student { get; set; }
}
