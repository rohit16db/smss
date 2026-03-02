namespace SMS.Domain.Enums;

/// <summary>
/// Status of marks entry for a class in an exam
/// </summary>
public enum MarksEntryStatus
{
    Pending = 0,       // Marks entry not started
    InProgress = 1,    // Marks are being entered, saved as draft
    Submitted = 2      // Marks finalized, report cards generated
}
