using Microsoft.EntityFrameworkCore;
using SMS.Application.Common.Interfaces;
using SMS.Domain.Entities;

namespace SMS.Infrastructure.Services.Messaging;

public class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;
    private readonly IEnumerable<INotificationProvider> _providers;

    public NotificationService(IApplicationDbContext context, IEnumerable<INotificationProvider> providers)
    {
        _context = context;
        _providers = providers;
    }

    public async Task<(bool Success, string? ErrorMessage)> SendTemplateNotificationAsync(
        string templateName, 
        string recipientPhone, 
        Dictionary<string, string> placeholders,
        string? relatedEntityType = null,
        Guid? relatedEntityId = null)
    {
        // 1. Fetch the template
        var template = await _context.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Name == templateName && t.IsActive);

        if (template == null)
        {
            return (false, $"Template '{templateName}' not found or is inactive.");
        }

        // 2. Parse the content
        var messageContent = MessageTemplateParser.Parse(template.Content, placeholders);

        // 3. Select the correct provider (SMS or WhatsApp)
        var provider = _providers.FirstOrDefault(p => p.Channel.Equals(template.Channel, StringComparison.OrdinalIgnoreCase));

        if (provider == null)
        {
            return (false, $"No provider found for channel '{template.Channel}'.");
        }

        // 4. Send the message
        var (success, errorMessage) = await provider.SendAsync(recipientPhone, messageContent);

        // 5. Log the history
        var history = new NotificationHistory
        {
            RecipientPhone = recipientPhone,
            MessageContent = messageContent,
            Channel = template.Channel,
            TemplateId = template.Id,
            Status = success ? "Sent" : "Failed",
            ErrorMessage = errorMessage,
            RelatedEntityType = relatedEntityType,
            RelatedEntityId = relatedEntityId
        };

        _context.NotificationHistories.Add(history);
        await _context.SaveChangesAsync();

        return (success, errorMessage);
    }
}
