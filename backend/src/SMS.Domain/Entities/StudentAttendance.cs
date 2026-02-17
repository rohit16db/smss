namespace SMS.Domain.Entities;

/// <summary>
/// Represents attendance record for a student in a section on a specific date.
/// Tracks status (present, absent, leave, unexcused) and full edit history.
/// </summary>
public class StudentAttendance
{
    public Guid Id { get; set; }
    
    /// <summary>Student ID (from Phase 2)</summary>
    public Guid StudentId { get; set; }
    
    /// <summary>Section ID - attendance is tracked at section level</summary>
    public Guid SectionId { get; set; }
    
    /// <summary>Date of attendance</summary>
    public DateOnly AttendanceDate { get; set; }
    
    /// <summary>Attendance status: present, absent, leave, unexcused</summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>Optional reason for absence/leave</summary>
    public string? Reason { get; set; }
    
    /// <summary>User ID who marked attendance (from Phase 2)</summary>
    public Guid? MarkedByUserId { get; set; }
    
    /// <summary>When attendance was marked</summary>
    public DateTime MarkedAt { get; set; } = DateTime.UtcNow;
    
    // Audit trail
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navigation property
    public Section? Section { get; set; }
}
