# Module: Students

## Overview
Full student lifecycle: creation, enrollment into Class/Section/AcademicYear, profile images, activation/deactivation, and promotion to next class.

---

## Domain Entities

### Student (`SMS.Domain.Entities.Student` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| FirstName | string | Required |
| LastName | string | Required |
| Email | string | Required |
| PhoneNumber | string? | |
| DateOfBirth | DateTime | |
| Address, City, State, PostalCode | string? | |
| EnrollmentNumber | string | Auto-generated unique ID |
| EnrollmentDate | DateTime | |
| IsActive | bool | Default true |
| GuardianName | string? | |
| GuardianPhone | string? | |
| GuardianEmail | string? | |
| ImagePath | string? | Path in `/uploads/students/` |
| *Nav* | Enrollments (collection) | |

### Enrollment (see [04-classes.md](./04-classes.md))
Links Student → AcademicYear → Class → Section with RollNumber and Status.

---

## API Endpoints

**Controller**: `StudentsController` — Route: `api/students`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/students` | List all students (paginated, filterable) |
| GET | `/api/students/{id}` | Get student by ID |
| POST | `/api/students` | Create student + enrollment |
| PUT | `/api/students/{id}` | Update student |
| DELETE | `/api/students/{id}` | Delete student |
| PATCH | `/api/students/{id}/activate` | Activate student |
| PATCH | `/api/students/{id}/deactivate` | Deactivate student |
| POST | `/api/students/{id}/upload-image` | Upload profile image |

### Promotion Endpoint
**Controller**: `PromotionsController` — Route: `api/v1/promotions`

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/promotions/bulk` | Bulk promote students to next class/year |

---

## CQRS

### Features/Classes (student-related queries)
- `GetStudentsInSectionQuery` — Students filtered by SectionId + AcademicYearId

### Features/Promotions
- **Command**: `PromoteStudentsCommand` — SourceClassId, SourceSectionId, TargetClassId, TargetSectionId, TargetAcademicYearId, StudentIds
- **DTO**: `PromotionResultDto`
- **Handler**: `PromoteStudentsCommandHandler`

---

## File Map

| Layer | File |
|-------|------|
| Entity | `backend/src/SMS.Domain/Entities/Student.cs` |
| Entity | `backend/src/SMS.Domain/Entities/Enrollment.cs` |
| Controller | `backend/src/SMS.API/Controllers/StudentsController.cs` |
| Controller | `backend/src/SMS.API/Controllers/PromotionsController.cs` |
| Promotion Cmd | `backend/src/SMS.Application/Features/Promotions/Commands/PromoteStudentsCommand.cs` |
| Promotion DTO | `backend/src/SMS.Application/Features/Promotions/DTOs/PromotionResultDto.cs` |
| Promotion Handler | `backend/src/SMS.Application/Features/Promotions/Handlers/PromoteStudentsCommandHandler.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/StudentConfiguration.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/EnrollmentConfiguration.cs` |
| Frontend Page | `frontend/src/pages/StudentsPage.tsx` |
| Promotion Page | `frontend/src/pages/StudentPromotionPage.tsx` |
| Promotion Hook | `frontend/src/hooks/usePromotion.ts` |
| Frontend API | `frontend/src/services/api.ts` (studentApi section) |

---

## Business Rules
- Student creation also creates an Enrollment record (Class + Section + AcademicYear)
- Students are listed with server-side pagination and search
- Profile images stored on disk in `/uploads/students/` directory (wwwroot)
- Promotion creates new Enrollment records in target AcademicYear, preserving history
- Students cannot be hard-deleted if they have enrollment/fee/attendance history
