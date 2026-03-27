namespace SMS.Domain.Entities;

/// <summary>
/// Represents a staff assignment to a class/subject.
/// Tracks when a staff member is assigned and when the assignment ends.
/// Soft-delete via removal_date (NULL = active assignment).
/// </summary>
public class StaffAssignment : BaseEntity
{

    public Guid StaffId { get; set; }
    public Staff Staff { get; set; } = null!;
    
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
    

    
}
