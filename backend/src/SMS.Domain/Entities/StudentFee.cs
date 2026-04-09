namespace SMS.Domain.Entities;

/// <summary>
/// Represents a fee assignment to a specific student.
/// Tracks the fee structure, period, and total amount owed.
/// </summary>
public class StudentFee : BaseEntity
{
    /// <summary>Enrollment ID (links to Academic Year, Student, Class, Section)</summary>
    public Guid EnrollmentId { get; set; }
    
    public Guid FeeStructureId { get; set; }
    
    /// <summary>Start date of this fee assignment</summary>
    public DateOnly StartDate { get; set; }
    
    /// <summary>End date of this fee assignment</summary>
    public DateOnly? EndDate { get; set; }
    
    /// <summary>Whether this fee assignment is currently active</summary>
    public bool IsActive { get; set; } = true;
    
    public decimal StructureAmount { get; set; }
    public decimal TransportFeeAmount { get; set; }
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public decimal TotalAmount => StructureAmount + TransportFeeAmount;
    
    public decimal PaidAmount { get; set; }
    
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public decimal BalanceAmount => TotalAmount - PaidAmount;

    // Navigation properties
    public Enrollment? Enrollment { get; set; }
    public FeeStructure? FeeStructure { get; set; }
    public ICollection<FeePayment> Payments { get; set; } = new List<FeePayment>();
}
