namespace SMS.Domain.Entities;

/// <summary>
/// Represents attendance record for a teacher on a specific date.
/// Tracks status for payroll and compliance purposes.
/// </summary>
public class TeacherAttendance
{
    public Guid Id { get; set; }
    
    public Guid TeacherId { get; set; }
    
    /// <summary>Date of attendance</summary>
    public DateOnly AttendanceDate { get; set; }
    
    /// <summary>Attendance status: present, absent, leave</summary>
    public string Status { get; set; } = string.Empty;
    
    /// <summary>Optional reason for absence/leave</summary>
    public string? Reason { get; set; }
    
    /// <summary>User ID who recorded attendance (from Phase 2)</summary>
    public Guid? RecordedByUserId { get; set; }
    
    /// <summary>When attendance was recorded</summary>
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    
    // Audit trail
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navigation properties
    public Teacher? Teacher { get; set; }
}
