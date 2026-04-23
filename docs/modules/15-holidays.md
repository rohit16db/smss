# Module: Holidays

## Overview
Manage the school holiday calendar per academic year. Holidays are categorized by type and are used by the attendance module to exclude non-working days.

---

## Domain Entity

### Holiday (`SMS.Domain.Entities.Holiday` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | e.g., "Independence Day" |
| HolidayDate | DateOnly | |
| Description | string? | |
| Type | string? | "National", "Religious", "School Event" |
| AcademicYearId | Guid | FK |
| *Nav* | AcademicYear |

---

## API Endpoints

**Controller**: `HolidaysController` — Route: `api/holidays`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/holidays` | List holidays (filtered by academic year) |
| GET | `/api/holidays/{id}` | Get by ID |
| GET | `/api/holidays/month/{year}/{month}` | Holidays in a specific month |
| POST | `/api/holidays` | Create holiday |
| PUT | `/api/holidays/{id}` | Update holiday |
| DELETE | `/api/holidays/{id}` | Delete holiday |

---

## CQRS (in `Features/Holidays`)
- **Commands**: CreateHoliday, UpdateHoliday, DeleteHoliday
- **Queries**: GetHolidays (by academic year), GetHolidayById, GetHolidaysByMonth
- **Validators**: Date validation, required fields

---

## File Map

| Layer | File |
|-------|------|
| Entity | `backend/src/SMS.Domain/Entities/Holiday.cs` |
| Commands | `backend/src/SMS.Application/Features/Holidays/Commands/` |
| DTOs | `backend/src/SMS.Application/Features/Holidays/DTOs/` |
| Handlers | `backend/src/SMS.Application/Features/Holidays/Handlers/` |
| Queries | `backend/src/SMS.Application/Features/Holidays/Queries/` |
| Validators | `backend/src/SMS.Application/Features/Holidays/Validators/` |
| Controller | `backend/src/SMS.API/Controllers/HolidaysController.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/HolidayConfiguration.cs` |
| Frontend Page | `frontend/src/pages/HolidaysPage.tsx` |
| Frontend API | `frontend/src/services/api.ts` (holidayApi section) |

---

## Business Rules
- Holidays are scoped to academic year
- Attendance module should skip holiday dates
- Types can be used for filtering/display (color-coded calendar)
