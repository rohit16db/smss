# Module: Timetable

## Overview
Manages time slots and timetable entries (class schedule). Supports weekly grid view, bulk copy between sections, and PDF export of timetables.

---

## Domain Entities

### TimeSlot (`SMS.Domain.Entities.TimeSlot` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | e.g., "Period 1" |
| StartTime | TimeOnly | |
| EndTime | TimeOnly | |
| AcademicYearId | Guid | FK |
| IsActive | bool | |

### TimetableEntry (`SMS.Domain.Entities.TimetableEntry` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| SectionId | Guid | FK |
| TimeSlotId | Guid | FK |
| SubjectId | Guid | FK |
| StaffId | Guid? | FK (teacher) |
| DayOfWeek | int | 0=Sunday..6=Saturday |
| AcademicYearId | Guid | FK |

---

## API Endpoints

**Controller**: `TimetableController` — Route: `api/timetable`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/timetable/timeslots/{academicYearId}` | List time slots |
| POST | `/api/timetable/timeslots` | Create time slot |
| PUT | `/api/timetable/timeslots/{id}` | Update time slot |
| DELETE | `/api/timetable/timeslots/{id}` | Delete time slot |
| POST | `/api/timetable/timeslots/bulk` | Bulk create time slots |
| GET | `/api/timetable/entries/section/{sectionId}/{academicYearId}` | Timetable for section |
| GET | `/api/timetable/entries/staff/{staffId}/{academicYearId}` | Timetable for staff |
| POST | `/api/timetable/entries` | Create entry |
| PUT | `/api/timetable/entries/{id}` | Update entry |
| DELETE | `/api/timetable/entries/{id}` | Delete entry |
| POST | `/api/timetable/entries/bulk-copy` | Copy timetable between sections |
| GET | `/api/timetable/entries/section/{sectionId}/{academicYearId}/export` | Export section PDF |
| GET | `/api/timetable/entries/staff/{staffId}/{academicYearId}/export` | Export staff PDF |

---

## CQRS (in `Features/Timetable`)
- **Commands**: CreateTimeSlot, UpdateTimeSlot, DeleteTimeSlot, BulkCreateTimeSlots, CreateTimetableEntry, UpdateTimetableEntry, DeleteTimetableEntry, BulkCopyTimetable
- **Queries**: GetTimeSlots, GetSectionTimetable, GetStaffTimetable, GetTimetablePdf
- **DTOs**: TimeSlotDto, TimetableEntryDto

---

## File Map

| Layer | File |
|-------|------|
| Entity | `backend/src/SMS.Domain/Entities/TimeSlot.cs` |
| Entity | `backend/src/SMS.Domain/Entities/TimetableEntry.cs` |
| Commands | `backend/src/SMS.Application/Features/Timetable/Commands/TimetableCommands.cs` |
| DTOs | `backend/src/SMS.Application/Features/Timetable/DTOs/TimeSlotDto.cs` |
| DTOs | `backend/src/SMS.Application/Features/Timetable/DTOs/TimetableEntryDto.cs` |
| Cmd Handlers | `backend/src/SMS.Application/Features/Timetable/Handlers/CommandHandlers/TimeSlotCommandHandlers.cs` |
| Cmd Handlers | `backend/src/SMS.Application/Features/Timetable/Handlers/CommandHandlers/TimetableEntryCommandHandlers.cs` |
| Query Handlers | `backend/src/SMS.Application/Features/Timetable/Handlers/QueryHandlers/TimetableQueryHandlers.cs` |
| PDF Handler | `backend/src/SMS.Application/Features/Timetable/Handlers/QueryHandlers/GetTimetablePdfQueryHandlers.cs` |
| Queries | `backend/src/SMS.Application/Features/Timetable/Queries/TimetableQueries.cs` |
| Controller | `backend/src/SMS.API/Controllers/TimetableController.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/TimeSlotConfiguration.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/TimetableEntryConfiguration.cs` |
| Frontend Page | `frontend/src/pages/TimetablePage.tsx` |
| Frontend API | `frontend/src/services/api.ts` (timetableApi section) |

---

## Business Rules
- Time slots are scoped per academic year
- Entries use DayOfWeek (0-6) for weekly scheduling
- Conflict detection: same section+timeslot+day cannot have two entries
- Bulk copy duplicates all entries from one section to another
- PDF export uses school branding
