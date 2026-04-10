using SMS.Domain.Entities;

namespace SMS.Application.Common.Interfaces;

public interface INotificationService
{
    /// <summary>
    /// Sends a notification using a saved template
    /// </summary>
    /// <param name="templateName">Name of the template to use</param>
    /// <param name="recipientPhone">Phone number of the guardian</param>
    /// <param name="placeholders">Data to fill in the template</param>
    /// <param name="relatedEntityType">Optional related entity type for logging</param>
    /// <param name="relatedEntityId">Optional related entity ID for logging</param>
    /// <returns>Execution result</returns>
    Task<(bool Success, string? ErrorMessage)> SendTemplateNotificationAsync(
        string templateName, 
        string recipientPhone, 
        Dictionary<string, string> placeholders,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null);
}
