namespace SMS.Application.Common.Interfaces;

/// <summary>
/// Service for managing roll numbers for students in sections
/// </summary>
public interface IRollNumberService
{
    /// <summary>
    /// Auto-assign sequential roll numbers (1, 2, 3...) to all students in a section
    /// </summary>
    Task AssignSequentialRollNumbersAsync(Guid sectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a student's roll number in a section with duplicate validation
    /// </summary>
    Task UpdateRollNumberAsync(Guid studentSectionId, int rollNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the next available roll number for a section
    /// </summary>
    Task<int> GetNextAvailableRollNumberAsync(Guid sectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all students in a section with their roll numbers
    /// </summary>
    Task<List<StudentSectionForRollManagement>> GetStudentsWithRollNumbersAsync(Guid sectionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk update roll numbers for multiple students
    /// </summary>
    Task BulkUpdateRollNumbersAsync(Guid sectionId, Dictionary<Guid, int> rollNumberUpdates, CancellationToken cancellationToken = default);
}

/// <summary>
/// DTO for roll number management
/// </summary>
public class StudentSectionForRollManagement
{
    public string StudentSectionId { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public int? CurrentRollNumber { get; set; }
    public DateTime JoinedDate { get; set; }
}
