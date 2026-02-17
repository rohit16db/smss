namespace SMS.Domain.Interfaces;

/// <summary>
/// Service for generating unique student IDs
/// </summary>
public interface IStudentIdGenerator
{
    /// <summary>
    /// Generates a unique student ID with the format: PREFIX-NNNNNN
    /// Example: STU-000001
    /// </summary>
    Task<string> GenerateStudentIdAsync(CancellationToken cancellationToken = default);
}
