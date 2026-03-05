using MediatR;
using SMS.Application.Features.Salary.DTOs;

namespace SMS.Application.Features.Salary.Commands;

/// <summary>
/// Create a new salary structure
/// </summary>
public class CreateSalaryStructureCommand : IRequest<SalaryStructureDto>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal HRA { get; set; }
    public decimal DA { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal ConveyanceAllowance { get; set; }
    public decimal OtherAllowances { get; set; }
    public decimal StandardDeduction { get; set; }
    public int MinExperienceYears { get; set; }
    public string? ApplicableQualifications { get; set; }
    public DateOnly EffectiveFromDate { get; set; }
    public DateOnly? EffectiveToDate { get; set; }
}

/// <summary>
/// Update an existing salary structure
/// </summary>
public class UpdateSalaryStructureCommand : IRequest<SalaryStructureDto>
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
    public int MinExperienceYears { get; set; }
    public string? ApplicableQualifications { get; set; }
    public DateOnly EffectiveFromDate { get; set; }
    public DateOnly? EffectiveToDate { get; set; }
}

/// <summary>
/// Delete a salary structure
/// </summary>
public class DeleteSalaryStructureCommand : IRequest<bool>
{
    public Guid Id { get; set; }
}

/// <summary>
/// Assign salary structure to a teacher
/// </summary>
public class AssignSalaryStructureToTeacherCommand : IRequest<TeacherSalaryAssignmentDto>
{
    public Guid TeacherId { get; set; }
    public Guid SalaryStructureId { get; set; }
    public DateOnly EffectiveDate { get; set; }
}

/// <summary>
/// Remove salary structure assignment from a teacher
/// </summary>
public class RemoveSalaryStructureAssignmentCommand : IRequest<bool>
{
    public Guid TeacherId { get; set; }
}

/// <summary>
/// Bulk create salary payments for all teachers with assigned salary structures
/// </summary>
public class BulkCreateSalaryFromStructuresCommand : IRequest<SalaryPaymentReportDto>
{
    public DateOnly PeriodStartDate { get; set; }
    public DateOnly PeriodEndDate { get; set; }
    public decimal FixedDeductions { get; set; }
}
