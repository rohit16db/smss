using Microsoft.Extensions.Logging;
using SMS.Application.Common.Interfaces;

namespace SMS.Infrastructure.Services.Messaging;

/// <summary>
/// A mock provider that logs messages to the console for development and testing.
/// Can be used as both SMS and WhatsApp provider for initial Phase 1.
/// </summary>
public class MockNotificationProvider : INotificationProvider
{
    private readonly ILogger<MockNotificationProvider> _logger;
    public string Channel { get; }
    public string ProviderName => "MockProvider";

    public MockNotificationProvider(ILogger<MockNotificationProvider> logger, string channel = "SMS")
    {
        _logger = logger;
        Channel = channel;
    }

    public Task<(bool Success, string? ErrorMessage)> SendAsync(string recipientPhone, string message)
    {
        _logger.LogInformation("[MOCK {Channel}] Sending message to {Phone}: {Message}", 
            Channel, recipientPhone, message);
        
        // Always succeed in mock
        return Task.FromResult((true, (string?)null));
    }
}
