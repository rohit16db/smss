using System.ComponentModel.DataAnnotations;

namespace SMS.Application.Features.Salary.DTOs;

/// <summary>
/// DTO for creating a new salary structure
/// </summary>
public class CreateSalaryStructureDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Base salary must be greater than 0")]
    public decimal BaseSalary { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "HRA cannot be negative")]
    public decimal HRA { get; set; } = 0;

    [Range(0, double.MaxValue, ErrorMessage = "DA cannot be negative")]
    public decimal DA { get; set; } = 0;

    [Range(0, double.MaxValue, ErrorMessage = "Medical allowance cannot be negative")]
    public decimal MedicalAllowance { get; set; } = 0;

    [Range(0, double.MaxValue, ErrorMessage = "Conveyance allowance cannot be negative")]
    public decimal ConveyanceAllowance { get; set; } = 0;

    [Range(0, double.MaxValue, ErrorMessage = "Other allowances cannot be negative")]
    public decimal OtherAllowances { get; set; } = 0;

    [Range(0, double.MaxValue, ErrorMessage = "Standard deduction cannot be negative")]
    public decimal StandardDeduction { get; set; } = 0;

    [Range(0, int.MaxValue, ErrorMessage = "Minimum experience years cannot be negative")]
    public int MinExperienceYears { get; set; } = 0;

    [StringLength(500, ErrorMessage = "Qualifications cannot exceed 500 characters")]
    public string? ApplicableQualifications { get; set; }

    [Required(ErrorMessage = "Effective from date is required")]
    public DateOnly EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }
}

/// <summary>
/// DTO for updating a salary structure
/// </summary>
public class UpdateSalaryStructureDto : CreateSalaryStructureDto
{
    [Required(ErrorMessage = "Salary structure ID is required")]
    public Guid Id { get; set; }
}

/// <summary>
/// DTO for salary structure response
/// </summary>
public class SalaryStructureDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal DA { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal ConveyanceAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal StandardDeduction { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public int MinExperienceYears { get; set; }
    public string? ApplicableQualifications { get; set; }
    public bool IsActive { get; set; }
    public DateOnly EffectiveFromDate { get; set; }
    public DateOnly? EffectiveToDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for assigning salary structure to teacher
/// </summary>
public class AssignSalaryStructureDto
{
    [Required(ErrorMessage = "Teacher ID is required")]
    public Guid TeacherId { get; set; }

    [Required(ErrorMessage = "Salary structure ID is required")]
    public Guid SalaryStructureId { get; set; }

    [Required(ErrorMessage = "Effective date is required")]
    public DateOnly EffectiveDate { get; set; }
}

/// <summary>
/// DTO for teacher salary assignment response
/// </summary>
public class TeacherSalaryAssignmentDto
{
    public Guid TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherEmail { get; set; } = string.Empty;
    public string? TeacherImagePath { get; set; }
    public Guid SalaryStructureId { get; set; }
    public string SalaryStructureName { get; set; } = string.Empty;
    public decimal GrossSalary { get; set; }
    public DateOnly EffectiveDate { get; set; }
    public DateTime AssignedAt { get; set; }
}

/// <summary>
/// DTO for bulk salary creation from structures
/// </summary>
public class BulkCreateFromStructureDto
{
    [Required(ErrorMessage = "Period start date is required")]
    public DateOnly PeriodStartDate { get; set; }

    [Required(ErrorMessage = "Period end date is required")]
    public DateOnly PeriodEndDate { get; set; }

    public decimal? FixedDeductions { get; set; } = 0;
}
