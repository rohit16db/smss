# Module: Subjects

## Overview
CRUD for academic subjects (Mathematics, English, Science, etc.). Subjects are used in exams (ExamSubject), timetable (TimetableEntry), and staff assignments.

---

## Domain Entity

### Subject (`SMS.Domain.Entities.Subject` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | Required |
| Code | string? | Short code e.g. "MATH" |
| Description | string? | |
| IsActive | bool | Default true |

---

## API Endpoints

**Controller**: `SubjectsController` — Route: `api/subjects`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/subjects` | List all subjects |
| GET | `/api/subjects/{id}` | Get by ID |
| GET | `/api/subjects/active` | List active subjects only |
| POST | `/api/subjects` | Create subject |
| PUT | `/api/subjects/{id}` | Update subject |
| DELETE | `/api/subjects/{id}` | Delete subject |

---

## CQRS (in `Features/Subjects`)
- **Commands**: `CreateSubjectCommand`, `UpdateSubjectCommand`, `DeleteSubjectCommand`
- **Queries**: `GetAllSubjectsQuery`, `GetSubjectByIdQuery`, `GetActiveSubjectsQuery`
- **DTOs**: `SubjectDto`

---

## File Map

| Layer | File |
|-------|------|
| Entity | `backend/src/SMS.Domain/Entities/Subject.cs` |
| Commands | `backend/src/SMS.Application/Features/Subjects/Commands/SubjectCommands.cs` |
| DTOs | `backend/src/SMS.Application/Features/Subjects/DTOs/SubjectDto.cs` |
| Cmd Handlers | `backend/src/SMS.Application/Features/Subjects/Handlers/CommandHandlers/SubjectCommandHandlers.cs` |
| Query Handlers | `backend/src/SMS.Application/Features/Subjects/Handlers/QueryHandlers/SubjectQueryHandlers.cs` |
| Queries | `backend/src/SMS.Application/Features/Subjects/Queries/SubjectQueries.cs` |
| Controller | `backend/src/SMS.API/Controllers/SubjectsController.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/SubjectConfiguration.cs` |
| Frontend Page | `frontend/src/pages/SubjectManagementPage.tsx` |
| Frontend API | `frontend/src/services/api.ts` (subjectApi section) |

---

## Relationships
- `ExamSubject.SubjectId` → FK to Subject
- `TimetableEntry.SubjectId` → FK to Subject
- `StaffAssignment.SubjectId` → FK to Subject
