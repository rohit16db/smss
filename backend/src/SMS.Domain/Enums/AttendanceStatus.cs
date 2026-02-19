namespace SMS.Domain.Enums;

/// <summary>
/// Attendance status for students and teachers.
/// </summary>
public static class AttendanceStatus
{
    public const string Present = "present";
    public const string Absent = "absent";
    public const string Late = "late";
    public const string Leave = "leave";
    public const string Unexcused = "unexcused";
    
    public static readonly string[] ValidStatuses = { Present, Absent, Late, Leave, Unexcused };
    
    public static bool IsValid(string? status) => 
        !string.IsNullOrEmpty(status) && ValidStatuses.Contains(status.ToLower());
}
