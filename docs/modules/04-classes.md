# Module: Classes & Sections

## Overview
Manages school classes (grades), their sections, student-section mappings, roll number assignment, and section history tracking. This is foundational — students are enrolled into Class + Section + AcademicYear.

---

## Domain Entities

### Class (`SMS.Domain.Entities.Class` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | e.g., "Grade 10" |
| IsActive | bool | Default true |
| *Nav* | Sections, Enrollments, StaffAssignments | |

### Section (`SMS.Domain.Entities.Section` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | e.g., "A", "B" |
| ClassId | Guid | FK to Class |
| Capacity | int? | Max students |
| IsActive | bool | |
| *Nav* | Class | |

### Enrollment (`SMS.Domain.Entities.Enrollment` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| StudentId | Guid | FK to Student |
| AcademicYearId | Guid | FK to AcademicYear |
| ClassId | Guid | FK to Class |
| SectionId | Guid? | FK to Section |
| RollNumber | int? | |
| Status | string | Default "Enrolled" |
| EnrollmentDate | DateTime | |
| *Nav* | Student, AcademicYear, Class, Section | |

---

## API Endpoints

**Controller**: `ClassesController` — Route: `api/v1/classes`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/classes` | List all classes |
| GET | `/api/v1/classes/{id}` | Get class by ID |
| POST | `/api/v1/classes` | Create class |
| PUT | `/api/v1/classes/{id}` | Update class |
| DELETE | `/api/v1/classes/{id}` | Delete class |
| GET | `/api/v1/classes/{classId}/sections` | Get sections of a class |
| POST | `/api/v1/classes/{classId}/sections` | Add section to class |
| PUT | `/api/v1/classes/sections/{sectionId}` | Update section |
| DELETE | `/api/v1/classes/sections/{sectionId}` | Delete section |
| GET | `/api/v1/classes/sections/{sectionId}/students` | Students in section |
| GET | `/api/v1/classes/students/{studentId}/section-history` | Student section history |
| GET | `/api/v1/classes/students/{studentId}/current-section` | Current section |
| POST | `/api/v1/classes/students/{studentId}/move-section` | Move student to different section |
| GET | `/api/v1/classes/sections/{sectionId}/roll-numbers` | Get roll numbers |
| POST | `/api/v1/classes/sections/{sectionId}/auto-assign-roll-numbers` | Auto-assign roll numbers |
| PUT | `/api/v1/classes/student-sections/{id}/roll-number` | Update single roll number |
| PUT | `/api/v1/classes/sections/{sectionId}/bulk-update-roll-numbers` | Bulk update roll numbers |

---

## CQRS (in `Features/Classes`)
- **Commands**: Create/Update/Delete Class, Create/Update/Delete Section, MoveStudentSection, AutoAssignRollNumbers, UpdateRollNumber, BulkUpdateRollNumbers
- **Queries**: GetAllClasses, GetClassById, GetSections, GetStudentsInSection, GetSectionHistory, GetCurrentSection, GetRollNumbers
- **DTOs**: ClassDto, SectionDto, StudentSectionDto, RollNumberDto

---

## File Map

| Layer | File |
|-------|------|
| Entity | `backend/src/SMS.Domain/Entities/Class.cs` |
| Entity | `backend/src/SMS.Domain/Entities/Section.cs` |
| Entity | `backend/src/SMS.Domain/Entities/Enrollment.cs` |
| Commands | `backend/src/SMS.Application/Features/Classes/Commands/` |
| DTOs | `backend/src/SMS.Application/Features/Classes/DTOs/` |
| Handlers | `backend/src/SMS.Application/Features/Classes/Handlers/` |
| Queries | `backend/src/SMS.Application/Features/Classes/Queries/` |
| Controller | `backend/src/SMS.API/Controllers/ClassesController.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/ClassConfiguration.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/SectionConfiguration.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/EnrollmentConfiguration.cs` |
| Frontend Page | `frontend/src/pages/ClassManagementPage.tsx` |
| Roll Numbers Page | `frontend/src/pages/RollNumberManagementPage.tsx` |
| Frontend API | `frontend/src/services/api.ts` (classApi section) |

---

## Key Relationships
- Class → has many Sections
- Enrollment links Student ↔ AcademicYear ↔ Class ↔ Section
- Enrollment is the pivot table for most queries (attendance, fees, exams scoped by enrollment)
- Roll numbers are per-enrollment (class + section + academic year)
