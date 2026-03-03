namespace SMS.Application.Features.Exams.DTOs;

/// <summary>
/// DTO for student marks entry
/// Single Responsibility: Transfer student marks data
/// </summary>
public class StudentMarksDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public Guid SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public Dictionary<Guid, SubjectMarkDto> SubjectMarks { get; set; } = new();
    public decimal? Total { get; set; }
    public decimal? Percentage { get; set; }
    public string? Grade { get; set; }
}

public class SubjectMarkDto
{
    public decimal? Obtained { get; set; }
    public bool IsAbsent { get; set; } = false;
}

/// <summary>
/// DTO for marks entry form (all students in class)
/// Single Responsibility: Transfer marks form data for marks entry page
/// </summary>
public class MarksEntryFormDto
{
    public Guid ExamId { get; set; }
    public string ExamName { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public string MarksEntryStatus { get; set; } = string.Empty;
    public List<SubjectForMarksDto> Subjects { get; set; } = new();
    public List<StudentMarksDto> Students { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class SubjectForMarksDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MaxMarks { get; set; }
}

/// <summary>
/// DTO for saving marks
/// Single Responsibility: Transfer marks save request data
/// </summary>
public class SaveMarksDto
{
    public Guid ExamId { get; set; }
    public Guid ClassId { get; set; }
    public List<StudentMarksEntryDto> MarksData { get; set; } = new();
}

public class StudentMarksEntryDto
{
    public Guid StudentId { get; set; }
    public Dictionary<Guid, SubjectMarkEntryDto> SubjectMarks { get; set; } = new();
}

public class SubjectMarkEntryDto
{
    public decimal? Obtained { get; set; }
    public bool IsAbsent { get; set; } = false;
}

/// <summary>
/// Response DTO for marks save operation
/// Single Responsibility: Transfer marks save result
/// </summary>
public class SaveMarksResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int MarksCount { get; set; }
    public ValidationResultsDto ValidationResults { get; set; } = new();
}

public class ValidationResultsDto
{
    public int StudentCount { get; set; }
    public int MarkedCount { get; set; }
    public int UnmarkedCount { get; set; }
    public decimal TotalMarksObtained { get; set; }
    public decimal AveragePercentage { get; set; }
}
