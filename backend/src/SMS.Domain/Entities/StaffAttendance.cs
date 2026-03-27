namespace SMS.Domain.Entities;

/// <summary>
/// Represents attendance record for a staff member on a specific date.
/// Tracks status for payroll and compliance purposes.
/// </summary>
public class StaffAttendance : BaseEntity
{

    public Guid StaffId { get; set; }
    
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
    

    
    // Navigation properties
    public Staff Staff { get; set; } = null!;
}
