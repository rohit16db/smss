using SMS.Domain.Entities;

namespace SMS.Domain.Entities;

/// <summary>
/// Represents a quantity-based inventory item (e.g., Summer Uniform, Class 10 Math Book)
/// </summary>
public class InventoryItem : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public Guid CategoryId { get; set; }
    public InventoryCategory? Category { get; set; }

    public int TotalQuantity { get; set; }
    public int ReorderLevel { get; set; } = 5; // Alert when stock falls below this
    
    public decimal UnitPrice { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
