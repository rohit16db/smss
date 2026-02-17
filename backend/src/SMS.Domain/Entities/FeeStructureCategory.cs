namespace SMS.Domain.Entities;

/// <summary>
/// Represents a category within a fee structure (e.g., tuition, transport, uniform).
/// Multiple categories can belong to a single fee structure.
/// </summary>
public class FeeStructureCategory
{
    public Guid Id { get; set; }
    
    public Guid FeeStructureId { get; set; }
    
    /// <summary>Category name (e.g., "tuition", "transport", "activities")</summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>Amount for this category</summary>
    public decimal Amount { get; set; }
    
    // Audit trail
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navigation properties
    public FeeStructure? FeeStructure { get; set; }
}
