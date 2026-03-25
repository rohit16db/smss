namespace SMS.Domain.Entities;

/// <summary>
/// Represents a fee structure defining how much students pay and in what frequency.
/// Multiple categories (tuition, transport, etc.) can be assigned to a structure.
/// </summary>
public class FeeStructure
{
    public Guid Id { get; set; }
    
    /// <summary>Name of the fee structure (e.g., "Regular Monthly 2026")</summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>Academic year Id this structure applies to</summary>
    public Guid AcademicYearId { get; set; }
    
    /// <summary>Academic year this structure applies to</summary>
    public AcademicYear? AcademicYear { get; set; }
    
    /// <summary>Payment frequency: monthly, quarterly, yearly</summary>
    public string Frequency { get; set; } = string.Empty;
    
    /// <summary>Total amount (sum of all categories)</summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>Whether this structure is currently active</summary>
    public bool IsActive { get; set; } = true;
    
    // Audit trail
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navigation properties
    public ICollection<FeeStructureCategory> Categories { get; set; } = new List<FeeStructureCategory>();
    public ICollection<StudentFee> StudentFees { get; set; } = new List<StudentFee>();
}
