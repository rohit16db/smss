using SMS.Domain.Enums;

namespace SMS.Domain.Entities;

/// <summary>
/// Represents the central PII (Personally Identifiable Information) profile for a person in the system.
/// Separated from employment or enrollment logic to allow multiple roles.
/// </summary>
public class UserProfile : BaseEntity
{
    /// <summary>Foreign key to Identity User table (from Phase 2)</summary>
    public Guid UserId { get; set; }
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    
    public DateOnly? DateOfBirth { get; set; }
    public string? BloodGroup { get; set; }
    public string? Gender { get; set; }
    
    public string? CurrentAddress { get; set; }
    public string? PermanentAddress { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    
    /// <summary>Path to profile image</summary>
    public string? ImagePath { get; set; }
    
    /// <summary>Computed property for full name</summary>
    public string FullName => $"{FirstName} {LastName}";
    
    // Navigation Properties
    public ICollection<Staff> StaffRoles { get; set; } = new List<Staff>();
}
