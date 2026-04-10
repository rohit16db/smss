using SMS.Domain.Entities;

namespace SMS.Domain.Entities;

/// <summary>
/// Defines templates for SMS and WhatsApp notifications
/// </summary>
public class NotificationTemplate : BaseEntity
{
    /// <summary>
    /// Unique name/code for the template (e.g., FEE_RECEIPT, ABSENCE_ALERT)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of what this template is for
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The message content with placeholders like {{StudentName}}
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Channel type: SMS or WhatsApp
    /// </summary>
    public string Channel { get; set; } = "SMS"; // SMS, WhatsApp

    /// <summary>
    /// Category of notification for easy filtering
    /// </summary>
    public string Category { get; set; } = "General"; // Fees, Transport, Attendance, General

    /// <summary>
    /// Whether this template is active and ready for use
    /// </summary>
    public bool IsActive { get; set; } = true;
}
