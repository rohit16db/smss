using SMS.Application.Common.Interfaces;
using SMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace SMS.Infrastructure.Seeders;

public static class NotificationTemplateSeeder
{
    public static async Task SeedAsync(IApplicationDbContext context)
    {
        var templates = new List<NotificationTemplate>
        {
            // Fees
            new NotificationTemplate
            {
                Name = "FEE_DUE_SMS",
                Description = "SMS reminder for pending fees",
                Channel = "SMS",
                Category = "Fees",
                Content = "Reminder: An amount of {{Amount}} is pending for {{StudentName}}'s school fees. Please clear the dues at the earliest. Regards, {{SchoolName}}."
            },
            new NotificationTemplate
            {
                Name = "FEE_DUE_WA",
                Description = "WhatsApp reminder for pending fees",
                Channel = "WhatsApp",
                Category = "Fees",
                Content = "Hello! This is a reminder that a balance of {{Amount}} is pending for {{StudentName}}'s fees. Please clear it by {{DueDate}} to avoid late fees. Regards, {{SchoolName}}."
            },
            new NotificationTemplate
            {
                Name = "FEE_PAYMENT_SMS",
                Description = "Sent after a fee payment is recorded",
                Channel = "SMS",
                Category = "Fees",
                Content = "Dear Guardian, we have received a payment of {{Amount}} for {{StudentName}} on {{Date}}. Receipt No: {{ReceiptNo}}. Thank you!"
            },
            
            // Attendance
            new NotificationTemplate
            {
                Name = "ATTENDANCE_ABSENT_SMS",
                Description = "Sent when a student is marked absent (SMS)",
                Channel = "SMS",
                Category = "Attendance",
                Content = "Alert: {{StudentName}} was marked ABSENT today ({{Date}}). If this was not planned, please contact the school office."
            },
            new NotificationTemplate
            {
                Name = "ATTENDANCE_ABSENT_WA",
                Description = "Sent when a student is marked absent (WhatsApp)",
                Channel = "WhatsApp",
                Category = "Attendance",
                Content = "Hello! This is to inform you that {{StudentName}} has been marked ABSENT for today's classes ({{Date}}). Please acknowledge."
            },

            // Transport
            new NotificationTemplate
            {
                Name = "TRANSPORT_UPDATE_SMS",
                Description = "Sent when transport details change (SMS)",
                Channel = "SMS",
                Category = "Transport",
                Content = "Transport Update: {{StudentName}}'s transport route/schedule has been updated. Route: {{RouteName}}. Please check the admin for details."
            },
            new NotificationTemplate
            {
                Name = "TRANSPORT_UPDATE_WA",
                Description = "Sent when transport details change (WhatsApp)",
                Channel = "WhatsApp",
                Category = "Transport",
                Content = "Hi! {{StudentName}}'s transport details have been updated. Route: {{RouteName}}. Vehicle No: {{VehicleNo}}. Driver: {{DriverName}} ({{DriverPhone}})."
            }
        };

        foreach (var template in templates)
        {
            var existing = await context.NotificationTemplates
                .FirstOrDefaultAsync(t => t.Name == template.Name);

            if (existing == null)
            {
                context.NotificationTemplates.Add(template);
            }
            else
            {
                // Update existing to ensure consistency
                existing.Description = template.Description;
                existing.Channel = template.Channel;
                existing.Category = template.Category;
                existing.Content = template.Content;
                existing.IsActive = true;
            }
        }

        await context.SaveChangesAsync();
    }
}
