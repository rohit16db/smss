namespace SMS.Domain.Entities;

/// <summary>
/// Represents a fee payment by a student.
/// Immutable record (not edited, reversed via new reversal transaction).
/// </summary>
public class FeePayment : BaseEntity
{
    public Guid StudentFeeId { get; set; }
    
    /// <summary>Amount paid in this transaction</summary>
    public decimal AmountPaid { get; set; }
    
    /// <summary>Date of payment (can be past date)</summary>
    public DateOnly PaymentDate { get; set; }
    
    /// <summary>Unique receipt number</summary>
    public string ReceiptNumber { get; set; } = string.Empty;
    
    /// <summary>Payment method: cash, check, bank_transfer</summary>
    public string PaymentMethod { get; set; } = string.Empty;
    
    /// <summary>Optional notes about payment</summary>
    public string? Notes { get; set; }
    
    // Navigation properties
    public StudentFee? StudentFee { get; set; }
}
