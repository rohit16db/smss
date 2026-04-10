using SMS.Domain.Entities;

namespace SMS.Domain.Entities;

/// <summary>
/// Records every notification sent via the system for auditing and tracking
/// </summary>
public class NotificationHistory : BaseEntity
{
    /// <summary>
    /// The phone number the message was sent to
    /// </summary>
    public string RecipientPhone { get; set; } = string.Empty;

    /// <summary>
    /// The actual message sent (after placeholder replacement)
    /// </summary>
    public string MessageContent { get; set; } = string.Empty;

    /// <summary>
    /// SMS or WhatsApp
    /// </summary>
    public string Channel { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the template used
    /// </summary>
    public Guid? TemplateId { get; set; }
    public NotificationTemplate? Template { get; set; }

    /// <summary>
    /// Status of the delivery (Sent, Failed, Pending, Mocked)
    /// </summary>
    public string Status { get; set; } = "Sent";

    /// <summary>
    /// Any error message returned by the provider
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Type of entity related to this notification (Student, Staff, etc.)
    /// </summary>
    public string? RelatedEntityType { get; set; }

    /// <summary>
    /// ID of the entity related to this notification
    /// </summary>
    public Guid? RelatedEntityId { get; set; }
}
