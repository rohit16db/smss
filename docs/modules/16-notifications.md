# Module: Notifications

## Overview
Manage notification templates (SMS/WhatsApp) with placeholder support, and send notifications. Templates are categorized and can be activated/deactivated. Notification history tracks sent messages.

---

## Domain Entities

### NotificationTemplate (`SMS.Domain.Entities.NotificationTemplate` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | Unique code e.g., "FEE_RECEIPT" |
| Description | string | Human-readable purpose |
| Content | string | Message with placeholders: `{{StudentName}}`, `{{Amount}}` |
| Channel | string | "SMS" or "WhatsApp" |
| Category | string | "Fees", "Transport", "Attendance", "General" |
| IsActive | bool | |

### NotificationHistory (`SMS.Domain.Entities.NotificationHistory` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| TemplateId | Guid? | FK |
| Recipient | string | Phone number |
| Message | string | Rendered message |
| Channel | string | |
| Status | string | Sent/Failed/Pending |
| SentAt | DateTime? | |
| ErrorMessage | string? | |

---

## API Endpoints

### NotificationSettingsController — Route: `api/settings/notifications`
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/settings/notifications/templates` | List templates |
| GET | `/api/settings/notifications/templates/{id}` | Get template |
| POST | `/api/settings/notifications/templates` | Create template |
| PUT | `/api/settings/notifications/templates/{id}` | Update template |
| DELETE | `/api/settings/notifications/templates/{id}` | Delete template |

### NotificationsController — Route: `api/notifications`
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/notifications/send` | Send notification |

---

## CQRS (in `Features/Notifications`)
- **Commands**: `SendNotificationCommand` — TemplateId, Recipients, Variables (placeholder values)
- **Handlers**: `NotificationTemplateHandlers.cs` — CRUD for templates + send logic

---

## File Map

| Layer | File |
|-------|------|
| Entity | `backend/src/SMS.Domain/Entities/NotificationTemplate.cs` |
| Entity | `backend/src/SMS.Domain/Entities/NotificationHistory.cs` |
| Commands | `backend/src/SMS.Application/Features/Notifications/Commands/SendNotificationCommand.cs` |
| DTOs | `backend/src/SMS.Application/Features/Notifications/DTOs/NotificationTemplateDto.cs` |
| Handlers | `backend/src/SMS.Application/Features/Notifications/Handlers/NotificationTemplateHandlers.cs` |
| Controller | `backend/src/SMS.API/Controllers/NotificationsController.cs` |
| Controller | `backend/src/SMS.API/Controllers/NotificationSettingsController.cs` |
| Frontend Component | `frontend/src/components/NotificationSettings.tsx` |
| Settings Page | `frontend/src/pages/SettingsPage.tsx` (Notifications tab) |
| Frontend API | `frontend/src/services/api.ts` (notificationApi section) |

---

## Business Rules
- Placeholders in content (e.g., `{{StudentName}}`) are replaced at send time
- Templates can be filtered by Category and Channel
- Notification history keeps audit trail of all sent messages
