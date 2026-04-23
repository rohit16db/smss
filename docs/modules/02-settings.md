# Module: Settings & Academic Years

## Overview
Manages school-wide configuration (name, branding, preferences) and academic year lifecycle. Academic year is the top-level scope for most data.

---

## Domain Entities

### School (`SMS.Domain.Entities.School` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | Required |
| Code | string | Required |
| Address, City, State, PostalCode | string? | |
| PhoneNumber, EmailAddress, Website | string? | |
| LogoImage | byte[]? | Binary logo data |
| LogoFileName | string? | |
| EstablishedDate | DateTime | |
| IsActive | bool | Default true |
| PrimaryColor | string | Default `#1976D2` |
| SecondaryColor | string | Default `#DC004E` |
| AccentColor | string | Default `#FF6F00` |
| HeaderText, FooterText | string? | Used in report PDFs |
| DateFormat | string | Default `dd/MM/yyyy` |
| CurrencyCode | string | Default `INR` |
| CurrencySymbol | string | Default `₹` |

### AcademicYear (`SMS.Domain.Entities.AcademicYear` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | e.g., "2026-2027" |
| StartDate | DateTime | |
| EndDate | DateTime | |
| IsCurrent | bool | Only one should be current |
| IsActive | bool | Default true |
| *Nav* | Enrollments, Holidays, FeeStructures, Exams, StaffAssignments | |

---

## API Endpoints

**Controller**: `SettingsController` — Route: `api/v1/settings`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/settings/school` | Get school settings |
| PUT | `/api/v1/settings/school` | Update school settings |
| POST | `/api/v1/settings/school/logo` | Upload school logo |
| GET | `/api/v1/settings/academic-years` | List all academic years |
| GET | `/api/v1/settings/academic-years/active` | Get active academic year |
| POST | `/api/v1/settings/academic-years` | Create academic year |
| PATCH | `/api/v1/settings/academic-years/{id}/toggle-status` | Toggle active status |

---

## CQRS Commands & Queries

### Commands
- `UpdateSchoolSettingsCommand` → Updates school info (all fields from School entity)
- `CreateAcademicYearCommand` → Name, StartDate, EndDate
- `ToggleAcademicYearStatusCommand` → Id

### Queries
- `GetSchoolSettingsQuery` → `SchoolDto`
- `GetAcademicYearsQuery` → `List<AcademicYearDto>`
- `GetActiveAcademicYearQuery` → `AcademicYearDto`

### DTOs
- `SchoolDto` — All school properties + `LogoBase64` (base64 encoded logo for frontend)
- `AcademicYearDto` — Id, Name, StartDate, EndDate, IsCurrent, IsActive

---

## File Map

### Backend
| Layer | File |
|-------|------|
| Entity | `backend/src/SMS.Domain/Entities/School.cs` |
| Entity | `backend/src/SMS.Domain/Entities/AcademicYear.cs` |
| Commands | `backend/src/SMS.Application/Features/Settings/Commands/UpdateSchoolSettingsCommand.cs` |
| Commands | `backend/src/SMS.Application/Features/Settings/Commands/AcademicYearCommands.cs` |
| DTOs | `backend/src/SMS.Application/Features/Settings/DTOs/SchoolDto.cs` |
| DTOs | `backend/src/SMS.Application/Features/Settings/DTOs/AcademicYearDto.cs` |
| Handlers | `backend/src/SMS.Application/Features/Settings/Handlers/Commands/UpdateSchoolSettingsCommandHandler.cs` |
| Handlers | `backend/src/SMS.Application/Features/Settings/Handlers/Queries/GetSchoolSettingsQueryHandler.cs` |
| Handlers | `backend/src/SMS.Application/Features/Settings/Handlers/AcademicYearHandlers.cs` |
| Queries | `backend/src/SMS.Application/Features/Settings/Queries/GetSchoolSettingsQuery.cs` |
| Queries | `backend/src/SMS.Application/Features/Settings/Queries/AcademicYearQueries.cs` |
| Controller | `backend/src/SMS.API/Controllers/SettingsController.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/SchoolConfiguration.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/AcademicYearConfiguration.cs` |

### Frontend
| Layer | File |
|-------|------|
| Settings Page | `frontend/src/pages/SettingsPage.tsx` |
| Academic Years Page | `frontend/src/pages/AcademicYearManagementPage.tsx` |
| School Hook | `frontend/src/hooks/useSchool.ts` |
| Academic Year Hook | `frontend/src/hooks/useAcademicYear.ts` |
| API Service | `frontend/src/services/api.ts` (settingsApi section) |
| Session Selector | `frontend/src/components/layout/Header.tsx` (academic year dropdown) |

---

## Business Rules
- Only one academic year can be `IsCurrent = true` at a time
- Selected academic year stored in `localStorage('selectedAcademicYearId')` and used globally
- Switching academic year reloads the page to re-scope all data
- School logo uploaded as multipart form data, stored as byte[] in DB, served as Base64
- School branding (colors, header/footer text) used in PDF exports (fee receipts, report cards)
