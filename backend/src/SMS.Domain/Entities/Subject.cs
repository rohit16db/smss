namespace SMS.Domain.Entities;

/// <summary>
/// Represents a subject/course that can be taught in the school
/// </summary>
public class Subject : BaseEntity
{
    /// <summary>Subject name (e.g., Mathematics, English, Science)</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Subject code (e.g., MATH101, ENG201)</summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>Subject description</summary>
    public string? Description { get; set; }
    
    /// <summary>Credit hours or weightage</summary>
    public int? Credits { get; set; }
    
    /// <summary>Subject is active or inactive</summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>Display order for sorting</summary>
    public int DisplayOrder { get; set; }
    
    // Navigation properties
    public ICollection<TeacherAssignment> TeacherAssignments { get; set; } = new List<TeacherAssignment>();
}
