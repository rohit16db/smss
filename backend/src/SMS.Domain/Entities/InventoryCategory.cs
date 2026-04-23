using SMS.Domain.Entities;

namespace SMS.Domain.Entities;

/// <summary>
/// Groups inventory items (e.g., Stationary, Uniforms, Books)
/// </summary>
public class InventoryCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
