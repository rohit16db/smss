using SMS.Domain.Entities;

namespace SMS.Application.Features.Teachers.DTOs;

/// <summary>
/// DTO for creating a new teacher
/// </summary>
public class CreateTeacherDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string Qualification { get; set; }
    public required int ExperienceYears { get; set; }
    public required DateTime JoiningDate { get; set; }
    public string? ImagePath { get; set; }
}

/// <summary>
/// DTO for updating teacher information
/// </summary>
public class UpdateTeacherDto
{
    public required string Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public required string Qualification { get; set; }
    public required int ExperienceYears { get; set; }
    public required bool IsActive { get; set; }
    public string? ImagePath { get; set; }
}

/// <summary>
/// DTO for reading teacher information
/// </summary>
public class TeacherDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Qualification { get; set; }
    public int ExperienceYears { get; set; }
    public DateTime JoiningDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ImagePath { get; set; }
}

/// <summary>
/// DTO for teacher list with pagination
/// </summary>
public class TeacherListDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public DateTime JoiningDate { get; set; }
    public bool IsActive { get; set; }
    public string? ImagePath { get; set; }
}

/// <summary>
/// DTO for paginated teacher list response
/// </summary>
public class PaginatedTeacherListDto
{
    public List<TeacherListDto> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}
