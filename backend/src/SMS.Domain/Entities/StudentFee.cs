namespace SMS.Domain.Entities;

/// <summary>
/// Represents a fee assignment to a specific student.
/// Tracks the fee structure, period, and total amount owed.
/// </summary>
public class StudentFee : BaseEntity
{
    /// <summary>Student ID (from Phase 2)</summary>
    public Guid StudentId { get; set; }
    
    public Guid FeeStructureId { get; set; }
    
    /// <summary>Start date of this fee assignment</summary>
    public DateOnly StartDate { get; set; }
    
    /// <summary>End date of this fee assignment</summary>
    public DateOnly? EndDate { get; set; }
    
    /// <summary>Total amount due for this fee assignment (cached from structure)</summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>Whether this fee assignment is currently active</summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Student? Student { get; set; }
    public FeeStructure? FeeStructure { get; set; }
    public ICollection<FeePayment> Payments { get; set; } = new List<FeePayment>();
}
