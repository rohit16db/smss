# Module: Departments

## Overview
Simple CRUD for organizational departments (e.g., Mathematics, Science, Administration). Departments are assigned to Staff members.

---

## Domain Entity

### Department (`SMS.Domain.Entities.Department` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | Required, unique |
| IsActive | bool | Default true |

---

## API Endpoints

**Controller**: `DepartmentsController` — Route: `api/departments`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/departments` | List all departments |
| GET | `/api/departments/{id}` | Get by ID |
| POST | `/api/departments` | Create department |
| PUT | `/api/departments/{id}` | Update department |
| DELETE | `/api/departments/{id}` | Delete department |

---

## CQRS (in `Features/Departments`)
- **Commands**: `CreateDepartmentCommand`, `UpdateDepartmentCommand`, `DeleteDepartmentCommand`
- **Queries**: `GetAllDepartmentsQuery`, `GetDepartmentByIdQuery`
- **DTOs**: `DepartmentDto` — Id, Name, IsActive

---

## File Map

| Layer | File |
|-------|------|
| Entity | `backend/src/SMS.Domain/Entities/Department.cs` |
| Commands | `backend/src/SMS.Application/Features/Departments/Commands/DepartmentCommands.cs` |
| DTOs | `backend/src/SMS.Application/Features/Departments/DTOs/DepartmentDto.cs` |
| Handlers | `backend/src/SMS.Application/Features/Departments/Handlers/` |
| Queries | `backend/src/SMS.Application/Features/Departments/Queries/` |
| Controller | `backend/src/SMS.API/Controllers/DepartmentsController.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/DepartmentConfiguration.cs` |
| Frontend Page | `frontend/src/pages/DepartmentPage.tsx` |
| Frontend API | `frontend/src/services/api.ts` (departmentApi section) |

---

## Relationships
- `Staff.DepartmentId` → FK to `Department.Id`
- Deleting a department with assigned staff should be prevented
