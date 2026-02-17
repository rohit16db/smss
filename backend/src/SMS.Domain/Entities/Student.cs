namespace SMS.Domain.Entities;

public class Student : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string EnrollmentNumber { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? GuardianName { get; set; }
    public string? GuardianPhone { get; set; }
    public string? GuardianEmail { get; set; }

    /// <summary>
    /// Student's enrollment in sections (tracks current and historical sections)
    /// </summary>
    public ICollection<StudentSection> StudentSections { get; set; } = new List<StudentSection>();
}
