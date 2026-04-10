namespace SMS.Application.Common.Interfaces;

/// <summary>
/// Generic interface for sending messages via SMS or WhatsApp
/// </summary>
public interface INotificationProvider
{
    /// <summary>
    /// Channel name supported by this provider (e.g., "SMS", "WhatsApp")
    /// </summary>
    string Channel { get; }

    /// <summary>
    /// Provider name (e.g., "Twilio", "Mock")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Sends a message to a recipient
    /// </summary>
    /// <param name="recipientPhone">Phone number in E.164 format or as required by provider</param>
    /// <param name="message">The parsed message content</param>
    /// <returns>A tuple indicating success and any error message</returns>
    Task<(bool Success, string? ErrorMessage)> SendAsync(string recipientPhone, string message);
}
