namespace SMS.Application.Features.Classes.DTOs;

/// <summary>
/// DTO for creating a new class
/// </summary>
public class CreateClassDto
{
    public string Name { get; set; } = string.Empty;
    public string? AcademicYear { get; set; }
}

/// <summary>
/// DTO for updating an existing class
/// </summary>
public class UpdateClassDto
{
    public string Name { get; set; } = string.Empty;
    public string? AcademicYear { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for class details
/// </summary>
public class ClassDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AcademicYear { get; set; }
    public bool IsActive { get; set; }
    public List<SectionDto> Sections { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for class list items (minimal info)
/// </summary>
public class ClassListDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AcademicYear { get; set; }
    public bool IsActive { get; set; }
    public int SectionCount { get; set; }
}

/// <summary>
/// DTO for paginated class list
/// </summary>
public class PaginatedClassListDto
{
    public List<ClassListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

/// <summary>
/// DTO for creating a new section
/// </summary>
public class CreateSectionDto
{
    public string ClassId { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating an existing section
/// </summary>
public class UpdateSectionDto
{
    public string SectionName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO for section details
/// </summary>
public class SectionDto
{
    public string Id { get; set; } = string.Empty;
    public string ClassId { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int StudentCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for section list items (minimal info)
/// </summary>
public class SectionListDto
{
    public string Id { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int StudentCount { get; set; }
}

/// <summary>
/// DTO for student section enrollment with history
/// </summary>
public class StudentSectionDto
{
    public string Id { get; set; } = string.Empty;
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string EnrollmentNumber { get; set; } = string.Empty;
    public string SectionId { get; set; } = string.Empty;
    public string SectionName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public DateTime JoinedDate { get; set; }
    public DateTime? LeftDate { get; set; }
    public bool IsCurrent { get; set; }
    public int? RollNumber { get; set; }
}

/// <summary>
/// DTO for student section history list
/// </summary>
public class StudentSectionHistoryDto
{
    public List<StudentSectionDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
