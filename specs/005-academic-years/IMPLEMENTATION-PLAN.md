# Implementation Plan: Academic Year & Enrollment Architecture

## Phase 1: Domain & Data Access Layer (Schema)
1. **Delete Obsolete Entity**: Remove `SMS.Domain.Entities.StudentSection.cs`.
2. **Create New Entities**: 
   - Add `AcademicYear.cs` inside `SMS.Domain.Entities`.
   - Add `Enrollment.cs` inside `SMS.Domain.Entities`.
3. **Refactor Existing Entities**:
   - `Class.cs`: Remove `AcademicYear` string field.
   - `Student.cs`: Remove `StudentSections` collection, add `Enrollments` collection.
   - `Exam.cs`: Add `AcademicYearId` navigation property.
   - `StudentAttendance.cs`: Change `StudentId` and `SectionId` to `EnrollmentId`.
   - `StudentFee.cs`: Change `StudentId` to `EnrollmentId` or add `AcademicYearId`.
   - `StudentMarks.cs`: Validate if `EnrollmentId` is better suited than `StudentId`.
4. **Update DB Context Configuration**:
   - Update mapping files in `SMS.Infrastructure.Configurations` (Fluent API setup).
   - Remove `StudentSectionConfiguration.cs`.
   - Add Configurations for `AcademicYear` and `Enrollment`.
5. **Database Reset**:
   - Drop the underlying database.
   - Clear the `Migrations` folder in the Infrastructure/API project.
   - Run `dotnet ef migrations add IntroduceAcademicYears`.
   - Run `dotnet ef database update`.

## Phase 2: Application Layer (CQRS & Services)
1. **Fix Broken References**:
   - Go through the compiler errors in `SMS.Application`.
   - Replace dependencies on `StudentSection` with `Enrollment`.
2. **Commands & Queries**:
   - Update DTOs to reflect the new relationships.
   - E.g., `GetStudentsBySectionQuery` becomes `GetEnrollmentsBySectionQuery` (filtered by active academic year).
3. **Add Academic Year Services**:
   - Create operations for CRUD Academic Years.

## Phase 3: Web API Layer (Controllers & Middleware)
1. **Context Middleware**:
   - Implement `AcademicYearMiddleware` (or Action Filter) to look for an `X-Academic-Year-Id` header.
   - Map this to an `ITenantContext` / `IAcademicYearContext` injected via DI so handlers know the current context without explicit parameters.
2. **Controllers**:
   - Expose `AcademicYearsController`.
   - Update `StudentsController`, `AttendanceController`, `FeesController`, `ExamsController` if needed based on the DTO adjustments from Phase 2.

## Phase 4: Frontend Integrations
1. **Services / API Clients**:
   - Create `academicYearApi.ts` to manage fetching academic years.
   - Add interceptor in Axios/Fetch wrappers to automatically append `X-Academic-Year-Id` header.
2. **Global Context/State**:
   - Create React Context (e.g., `AcademicYearContext.tsx`) or update global store to hold the currently selected Academic Year ID.
3. **UI Updates (Layout)**:
   - Add dropdown in Header/Navbar rendering available Academic Years.
   - Upon changing the dropdown, update global state and trigger cache invalidation for React Query.
4. **View Refactoring**:
   - Ensure the views for Fees, Attendance, and Exams rely correctly on the new contextual endpoints. Clean up any UI logic that explicitly referenced old structural paradigms.

## Phase 5: Testing & QA
1. Create a dummy Academic Year 2024-2025.
2. Create dummy Classes & Sections.
3. Enroll a student.
4. Add attendance/fees for that student.
5. Create a new dummy Academic Year 2025-2026.
6. Verify swapping the global dropdown hides the 2024-2025 data and shows blank data for 2025-2026 without breaking.
