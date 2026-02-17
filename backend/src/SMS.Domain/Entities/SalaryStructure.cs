namespace SMS.Domain.Entities;

/// <summary>
/// Represents a salary structure/scale that defines base pay and allowances
/// Teachers are linked to a salary structure based on qualifications and experience
/// </summary>
public class SalaryStructure : BaseEntity
{
    /// <summary>
    /// Name of the salary structure (e.g., "Grade A", "Senior Teacher")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the salary structure
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Base salary amount for this structure
    /// </summary>
    public decimal BaseSalary { get; set; }

    /// <summary>
    /// House Rent Allowance (HRA)
    /// </summary>
    public decimal HRA { get; set; } = 0;

    /// <summary>
    /// Dearness Allowance (DA)
    /// </summary>
    public decimal DA { get; set; } = 0;

    /// <summary>
    /// Medical Allowance
    /// </summary>
    public decimal MedicalAllowance { get; set; } = 0;

    /// <summary>
    /// Conveyance/Transport Allowance
    /// </summary>
    public decimal ConveyanceAllowance { get; set; } = 0;

    /// <summary>
    /// Other allowances (if any)
    /// </summary>
    public decimal OtherAllowances { get; set; } = 0;

    /// <summary>
    /// Standard deduction for provident fund or other mandatory deductions
    /// </summary>
    public decimal StandardDeduction { get; set; } = 0;

    /// <summary>
    /// Total salary = BaseSalary + Allowances - StandardDeduction
    /// </summary>
    public decimal GrossSalary => BaseSalary + TotalAllowances - StandardDeduction;

    /// <summary>
    /// Computed total allowances
    /// </summary>
    public decimal TotalAllowances => HRA + DA + MedicalAllowance + ConveyanceAllowance + OtherAllowances;

    /// <summary>
    /// Applicable for these experience years (minimum)
    /// </summary>
    public int MinExperienceYears { get; set; } = 0;

    /// <summary>
    /// Applicable for these qualification types (comma-separated, e.g., "B.Sc, B.Ed")
    /// </summary>
    public string? ApplicableQualifications { get; set; }

    /// <summary>
    /// Whether this salary structure is active for new assignments
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Effective from date
    /// </summary>
    public DateOnly EffectiveFromDate { get; set; }

    /// <summary>
    /// Effective to date (null means still active)
    /// </summary>
    public DateOnly? EffectiveToDate { get; set; }

    /// <summary>
    /// Audit trail
    /// </summary>
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
}
