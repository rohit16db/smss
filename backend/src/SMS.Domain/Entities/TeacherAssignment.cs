namespace SMS.Domain.Entities;

/// <summary>
/// Represents a teacher assignment to a class/subject.
/// Tracks when a teacher is assigned and when the assignment ends.
/// Soft-delete via removal_date (NULL = active assignment).
/// </summary>
public class TeacherAssignment
{
    public Guid Id { get; set; }
    
    public Guid TeacherId { get; set; }
    
    /// <summary>Class ID (from existing class entity)</summary>
    public Guid ClassId { get; set; }
    
    /// <summary>Subject ID (from existing subject entity)</summary>
    public Guid SubjectId { get; set; }
    
    /// <summary>
    /// Academic year Id this assignment applies to
    /// </summary>
    public Guid AcademicYearId { get; set; }
    
    /// <summary>
    /// Academic year this assignment applies to
    /// </summary>
    public AcademicYear? AcademicYear { get; set; }
    
    /// <summary>Date when assignment started</summary>
    public DateOnly AssignmentDate { get; set; }
    
    /// <summary>Date when assignment ended (NULL = active assignment)</summary>
    public DateOnly? RemovalDate { get; set; }
    
    // Audit trail
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navigation properties
    public Teacher? Teacher { get; set; }
}
