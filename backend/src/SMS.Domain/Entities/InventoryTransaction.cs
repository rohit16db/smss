using SMS.Domain.Entities;

namespace SMS.Domain.Entities;

/// <summary>
/// Logs stock movements (In/Out) for inventory items
/// </summary>
public class InventoryTransaction : BaseEntity
{
    public Guid ItemId { get; set; }
    public InventoryItem? Item { get; set; }

    public string TransactionType { get; set; } = "StockIn"; // StockIn, StockOut, Adjust
    public int Quantity { get; set; }
    
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    
    // Who received it (optional, can be Staff/Department name)
    public string? ReceivedBy { get; set; }
    
    public string? Remarks { get; set; }
    
    // Potential link to specific academic year
    public Guid AcademicYearId { get; set; }
}
