# Module: Staff Management

## Overview
Manages staff/employee profiles (personal info via UserProfile), department assignments, class/subject assignments, qualifications, profile images, and links to salary structures.

---

## Domain Entities

### Staff (`SMS.Domain.Entities.Staff` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| UserProfileId | Guid | FK to UserProfile (PII) |
| DepartmentId | Guid? | FK to Department |
| Designation | string | e.g., "Senior Math Teacher" |
| RoleType | UserRole (enum) | Admin/Accountant/Clerk/Teacher |
| ExperienceYears | int | |
| JoiningDate | DateOnly | |
| IsActive | bool | Default true |
| BasicSalary | decimal | |
| SalaryStructureId | Guid? | FK to SalaryStructure |
| SalaryStructureEffectiveDate | DateOnly? | |
| *Nav* | UserProfile, Department, SalaryStructure, Qualifications, Assignments, AttendanceRecords |
| *Computed* | `FullName` → `UserProfile.FirstName + LastName` |

### UserProfile (`SMS.Domain.Entities.UserProfile` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| FirstName, LastName | string | |
| Email | string | |
| PhoneNumber, Gender, DateOfBirth | various | |
| Address, City, State, PostalCode | string? | |
| ImagePath | string? | |

### EducationalQualification (`SMS.Domain.Entities.EducationalQualification`)
Degree, Institution, Year, linked to Staff.

### StaffAssignment (`SMS.Domain.Entities.StaffAssignment`)
Links Staff → Class → Section → Subject → AcademicYear. Tracks who teaches what where.

---

## API Endpoints

**Controller**: `StaffController` — Route: `api/staff`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/staff` | List all staff (paginated) |
| GET | `/api/staff/{id}` | Get by ID |
| GET | `/api/staff/by-email/{email}` | Find by email |
| GET | `/api/staff/active` | Active staff only |
| GET | `/api/staff/check-email/{email}` | Check email availability |
| POST | `/api/staff` | Create staff |
| PUT | `/api/staff/{id}` | Update staff |
| PATCH | `/api/staff/{id}/activate` | Activate |
| PATCH | `/api/staff/{id}/deactivate` | Deactivate |
| DELETE | `/api/staff/{id}` | Delete |
| POST | `/api/staff/{id}/upload-image` | Upload profile image |
| GET | `/api/staff/{id}/assignments` | Get assignments |
| GET | `/api/staff/section/{sectionId}/{academicYearId}` | Staff assigned to section |
| POST | `/api/staff/{id}/assignments` | Create assignment |
| DELETE | `/api/staff/{id}/assignments/{assignmentId}` | Remove assignment |

---

## CQRS (in `Features/StaffManagement`)

### Commands
- `CreateStaffCommand` — Full profile + department + designation + role + qualifications
- `UpdateStaffCommand`, `DeleteStaffCommand`
- `ActivateStaffCommand`, `DeactivateStaffCommand`
- `CreateStaffAssignmentCommand` — StaffId, ClassId, SectionId, SubjectId, AcademicYearId
- `DeleteStaffAssignmentCommand`

### Queries
- `GetAllStaffQuery` — Paginated with search, filter by department/role/active
- `GetStaffByIdQuery`, `GetStaffByEmailQuery`, `GetActiveStaffQuery`
- `GetStaffAssignmentsQuery`, `GetStaffBySectionQuery`

### Validators (`StaffValidators.cs`)
- FluentValidation rules for create/update commands

---

## File Map

| Layer | File |
|-------|------|
| Entity | `backend/src/SMS.Domain/Entities/Staff.cs` |
| Entity | `backend/src/SMS.Domain/Entities/UserProfile.cs` |
| Entity | `backend/src/SMS.Domain/Entities/EducationalQualification.cs` |
| Entity | `backend/src/SMS.Domain/Entities/StaffAssignment.cs` |
| Commands | `backend/src/SMS.Application/Features/StaffManagement/Commands/StaffCommands.cs` |
| Commands | `backend/src/SMS.Application/Features/StaffManagement/Commands/StaffAssignmentCommands.cs` |
| DTOs | `backend/src/SMS.Application/Features/StaffManagement/DTOs/StaffDto.cs` |
| DTOs | `backend/src/SMS.Application/Features/StaffManagement/DTOs/StaffAssignmentDto.cs` |
| Cmd Handlers | `backend/src/SMS.Application/Features/StaffManagement/Handlers/CommandHandlers/StaffCommandHandlers.cs` |
| Query Handlers | `backend/src/SMS.Application/Features/StaffManagement/Handlers/QueryHandlers/StaffQueryHandlers.cs` |
| Assignment Handlers | `backend/src/SMS.Application/Features/StaffManagement/Handlers/StaffAssignmentHandlers.cs` |
| Queries | `backend/src/SMS.Application/Features/StaffManagement/Queries/StaffQueries.cs` |
| Queries | `backend/src/SMS.Application/Features/StaffManagement/Queries/StaffAssignmentQueries.cs` |
| Validators | `backend/src/SMS.Application/Features/StaffManagement/Validators/StaffValidators.cs` |
| Controller | `backend/src/SMS.API/Controllers/StaffController.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/StaffConfiguration.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/StaffAssignmentConfiguration.cs` |
| Frontend Page | `frontend/src/pages/StaffDirectoryPage.tsx` |
| Frontend Components | `frontend/src/components/Staffs/` |
| Frontend API | `frontend/src/services/api.ts` (StaffApi section) |

---

## Key Relationships
- Staff → UserProfile (1:1, PII separated)
- Staff → Department (many:1)
- Staff → SalaryStructure (many:1, optional)
- StaffAssignment links Staff → Class → Section → Subject → AcademicYear
- Staff → StaffAttendance (1:many)
- Staff → SalaryPayment (1:many)
