namespace SMS.Application.Features.Subjects.DTOs;

/// <summary>
/// DTO for creating a new subject
/// </summary>
public class CreateSubjectDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Credits { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>
/// DTO for updating an existing subject
/// </summary>
public class UpdateSubjectDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Credits { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>
/// DTO for subject details
/// </summary>
public class SubjectDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? Credits { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for subject list items (minimal info)
/// </summary>
public class SubjectListDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int? Credits { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>
/// DTO for paginated subject list
/// </summary>
public class PaginatedSubjectListDto
{
    public List<SubjectListDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}
