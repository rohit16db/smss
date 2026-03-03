namespace SMS.Application.Features.Exams.DTOs;

/// <summary>
/// Data Transfer Object for creating or updating an exam
/// Single Responsibility: Transfer exam input data with validation
/// </summary>
public class CreateExamDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalMarks { get; set; } = 100;
    public decimal PassMarks { get; set; } = 40;
    public List<ExamSubjectInputDto> Subjects { get; set; } = new();
    public List<Guid> ClassIds { get; set; } = new();
}

/// <summary>
/// DTO for exam subject with max marks
/// </summary>
public class ExamSubjectInputDto
{
    public Guid SubjectId { get; set; }
    public decimal MaxMarks { get; set; }
    public decimal PassMarks { get; set; } = 40;
}

/// <summary>
/// Data Transfer Object for exam details response
/// Single Responsibility: Transfer exam data to client
/// </summary>
public class ExamDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal PassMarks { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Detailed exam DTO with subjects and classes
/// Single Responsibility: Transfer complete exam information
/// </summary>
public class ExamDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalMarks { get; set; }
    public decimal PassMarks { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<ExamSubjectDto> Subjects { get; set; } = new();
    public List<ExamClassDto> Classes { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ExamSubjectDto
{
    public Guid SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public decimal MaxMarks { get; set; }
    public decimal PassMarks { get; set; }
}

public class ExamClassDto
{
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public string MarksEntryStatus { get; set; } = string.Empty;
    public DateTime? SubmittedAt { get; set; }
}
