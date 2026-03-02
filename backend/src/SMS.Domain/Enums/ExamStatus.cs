namespace SMS.Domain.Enums;

/// <summary>
/// Status of an examination
/// </summary>
public enum ExamStatus
{
    Draft = 0,         // Can be edited, not available for marks entry
    Published = 1,     // Marks entry can begin
    Completed = 2,     // Marks submission finished
    Archived = 3       // Old exams
}
