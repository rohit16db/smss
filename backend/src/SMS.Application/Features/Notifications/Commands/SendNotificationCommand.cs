using MediatR;
using SMS.Application.Common.Interfaces;

namespace SMS.Application.Features.Notifications.Commands;

/// <summary>
/// Generic command to send a notification using a template name and a dictionary of data
/// </summary>
public class SendNotificationCommand : IRequest<(bool Success, string? ErrorMessage)>
{
    public string TemplateName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public Dictionary<string, string> Placeholders { get; set; } = new();
    
    // Optional logging metadata
    public string? RelatedEntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
}

public class SendNotificationCommandHandler : IRequestHandler<SendNotificationCommand, (bool Success, string? ErrorMessage)>
{
    private readonly INotificationService _notificationService;

    public SendNotificationCommandHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public async Task<(bool Success, string? ErrorMessage)> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        return await _notificationService.SendTemplateNotificationAsync(
            request.TemplateName,
            request.RecipientPhone,
            request.Placeholders,
            request.RelatedEntityType,
            request.RelatedEntityId);
    }
}
