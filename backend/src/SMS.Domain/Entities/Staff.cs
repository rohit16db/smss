using SMS.Domain.Enums;

namespace SMS.Domain.Entities;

/// <summary>
/// Represents an employee in the school system.
/// This entity ties their employment records together and links to their core Personal Information Profile.
/// </summary>
public class Staff : BaseEntity
{
    /// <summary>Foreign key to the PII Profile table</summary>
    public Guid UserProfileId { get; set; }
    public UserProfile UserProfile { get; set; } = null!;
    public string FullName => UserProfile != null ? $"{UserProfile.FirstName} {UserProfile.LastName}" : string.Empty;
    
    /// <summary>Foreign key to their Department</summary>
    public Guid? DepartmentId { get; set; }
    public Department? Department { get; set; }
    
    /// <summary>e.g., "Senior Mathematics Teacher", "Lab Assistant"</summary>
    public string Designation { get; set; } = string.Empty;
    
    /// <summary>Using UserRole enum (Admin, Accountant, Clerk, Teacher)</summary>
    public UserRole RoleType { get; set; }
    
    /// <summary>Years of professional experience</summary>
    public int ExperienceYears { get; set; }
    
    /// <summary>Date when staff member joined the school</summary>
    public DateOnly JoiningDate { get; set; }
    
    /// <summary>Current employment status</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Starting or current base salary for payroll calculation</summary>
    public decimal BasicSalary { get; set; }

    /// <summary>Foreign key to SalaryStructure (Phase 3 Salary Module)</summary>
    public Guid? SalaryStructureId { get; set; }

    /// <summary>Date when salary structure was assigned</summary>
    public DateOnly? SalaryStructureEffectiveDate { get; set; }
    
    // Navigation properties
    public SalaryStructure? SalaryStructure { get; set; }
    public ICollection<EducationalQualification> Qualifications { get; set; } = new List<EducationalQualification>();
    public ICollection<StaffAssignment> Assignments { get; set; } = new List<StaffAssignment>();
    public ICollection<StaffAttendance> AttendanceRecords { get; set; } = new List<StaffAttendance>();
}
