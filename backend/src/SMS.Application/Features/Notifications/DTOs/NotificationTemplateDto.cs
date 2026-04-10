namespace SMS.Application.Features.Notifications.DTOs;

public class NotificationTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Channel { get; set; } = "SMS";
    public string Category { get; set; } = "General";
    public bool IsActive { get; set; }
}

public class CreateNotificationTemplateDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Channel { get; set; } = "SMS";
    public string Category { get; set; } = "General";
}

public class UpdateNotificationTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Channel { get; set; } = "SMS";
    public string Category { get; set; } = "General";
    public bool IsActive { get; set; }
}
