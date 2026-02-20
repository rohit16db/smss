namespace SMS.Domain.Entities;

/// <summary>
/// Represents a teacher/staff member in the school system.
/// Tracks personal information, qualifications, experience, and employment status.
/// </summary>
public class Teacher : BaseEntity
{
    /// <summary>Foreign key to User table (from Phase 2)</summary>
    public Guid UserId { get; set; }
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    
    /// <summary>Academic qualifications (e.g., "M.Sc. Mathematics, B.Ed.")</summary>
    public string? Qualification { get; set; }
    
    /// <summary>Years of teaching experience</summary>
    public int ExperienceYears { get; set; }
    
    /// <summary>Date when teacher joined the school</summary>
    public DateOnly JoiningDate { get; set; }
    
    /// <summary>Current employment status</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Foreign key to SalaryStructure (Phase 3 Salary Module)</summary>
    public Guid? SalaryStructureId { get; set; }

    /// <summary>Date when salary structure was assigned</summary>
    public DateOnly? SalaryStructureEffectiveDate { get; set; }
    
    /// <summary>
    /// Path to teacher's profile image (stored in /uploads/teachers/)
    /// </summary>
    public string? ImagePath { get; set; }
    
    // Navigation properties
    public SalaryStructure? SalaryStructure { get; set; }
    public ICollection<TeacherAssignment> Assignments { get; set; } = new List<TeacherAssignment>();
    public ICollection<TeacherAttendance> AttendanceRecords { get; set; } = new List<TeacherAttendance>();
    
    /// <summary>Computed property for full name</summary>
    public string FullName => $"{FirstName} {LastName}";
}
