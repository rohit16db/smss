# Feature Specification: Academic Year & Enrollment Architecture

## 1. Overview
Currently, the School Management System links `Student` entities directly to `Section` (via `StudentSection`), and transactional records like `StudentAttendance`, `Exam`, and `StudentFee` are tied closely to the student or date ranges. 
This architecture makes it difficult to cleanly separate data by "Academic Year" and complicates student promotion.
The goal of this feature is to introduce an **Enrollment-Based Architecture** where every transactional record is explicitly or implicitly scoped to an `AcademicYear`.

## 2. Core Domain Changes

### 2.1 New Entities
*   **`AcademicYear`**: 
    *   `Id` (Guid)
    *   `Name` (string) - e.g., "2024-2025"
    *   `StartDate` (DateTime/DateOnly)
    *   `EndDate` (DateTime/DateOnly)
    *   `IsCurrent` (bool) - Indicates the default active year for the system.
    *   `IsActive` (bool)

*   **`Enrollment`**: 
    *   `Id` (Guid)
    *   `StudentId` (Guid) - Foreign Key
    *   `AcademicYearId` (Guid) - Foreign Key
    *   `ClassId` (Guid) - Foreign Key
    *   `SectionId` (Guid) - Foreign Key (Optional/Required depending on school rules)
    *   `RollNumber` (int?)
    *   `Status` (string) - "Enrolled", "Promoted", "Transferred", "Dropped"
    *   `EnrollmentDate` (DateTime/DateOnly)
    *   *Note: Replaces the existing `StudentSection` entity.*

### 2.2 Modified Entities
*   **`Class`**:
    *   Remove `AcademicYear` property.
*   **`StudentAttendance`**:
    *   Replace `StudentId` and `SectionId` with `EnrollmentId`. (Ties attendance strictly to that enrollment year).
*   **`Exam`**:
    *   Add `AcademicYearId`.
*   **`StudentMarks`**:
    *   Ensure it correctly references `EnrollmentId` or `StudentId` scoped by `Exam.AcademicYearId`.
*   **`StudentFee`**:
    *   Replace `StudentId` (or augment it) with `EnrollmentId` or `AcademicYearId`.
*   **`StudentSection`**:
    *   Delete entity completely.

## 3. Backend Requirements
*   **Database Cleanup**: 
    *   Since the app is under development, existing data will be wiped out manually. No data migration is required.
    *   A single Entity Framework migration will be added for the final corrected schema.
*   **EF Core Configuration**: 
    *   Update FluentAPI setups to reflect the relationships of `Enrollment` and `AcademicYear`.
*   **Middleware/Context**: 
    *   Implement an API Middleware to read `X-Academic-Year-Id` header to scope operations.
*   **Application Logic**: 
    *   Update all Queries/Commands (CQRS) that queried `StudentSection` to now use `Enrollment`.
    *   Ensure validation: transactions must align with the `AcademicYear`'s active dates.

## 4. Frontend Requirements
*   **Global State**: 
    *   Store the currently selected `AcademicYear` in the application state.
*   **Global Selector**: 
    *   Add an Academic Year dropdown to the main application header (Top Nav or Sidebar).
*   **API Interceptor**: 
    *   Configure Axios (or React Query fetchers) to automatically append the `X-Academic-Year-Id` header to all API requests.
*   **Page Updates**: 
    *   Attendance, Exams, and Fee pages should automatically refresh and query based on the selected Academic Year context.

## 5. Transition & Promotion Workflow
*   In the future, an interface will be needed for bulk promoting students from one `AcademicYear` to the next, which simply generates new `Enrollment` records. (Out of scope for initial core refactor).
