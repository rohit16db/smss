namespace SMS.Domain.Entities;

/// <summary>
/// Base entity class for all domain entities with common audit fields
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Primary key identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Timestamp when the entity was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the entity was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// User who created the entity
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// User who last updated the entity
    /// </summary>
    public string? UpdatedBy { get; set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
