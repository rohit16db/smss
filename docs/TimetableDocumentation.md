# Timetable & Scheduler Module Documentation

## Overview
The Timetable Management module is a core component of the School Management System (SMS) designed to seamlessly orchestrate daily class schedules. It provides a highly interactive and session-aware interface for administrators to define periods (Time Slots) and assign subjects/teachers to specific sections (Timetable Entries) while strictly preventing overlaps and scheduling conflicts.

---

## 1. Domain Architecture

The module introduces two primary entities into the `SMS.Domain` namespace, both featuring strict `AcademicYearId` scoping to support the multi-year architecture.

### A. `TimeSlot` Entity
Represents a predefined period of time during a school day.
*   **Properties:**
    *   `Name` (e.g., "Period 1", "Lunch", "Period 2")
    *   `DayOfWeek` (Integer: 1-Monday to 6-Saturday)
    *   `StartTime` & `EndTime` (TimeSpan)
    *   `IsBreak` (Boolean: Determines if the slot is an instructional period or an interval)
    *   `AcademicYearId` (Guid: Ties the slot to a specific academic session)

### B. `TimetableEntry` Entity
Represents the actual assignment of a subject and teacher to a class section during a specific `TimeSlot`.
*   **Properties:**
    *   `TimeSlotId` (Guid) -> FK to `TimeSlot`
    *   `SectionId` (Guid) -> FK to `Section`
    *   `SubjectId` (Guid) -> FK to `Subject`
    *   `TeacherId` (Guid) -> FK to `Teacher`
    *   `RoomNumber` (String, Optional)
    *   `DayOfWeek` (Integer: Denormalized from TimeSlot for faster querying)
    *   `AcademicYearId` (Guid: Strict session isolation)

---

## 2. Business Logic & Conflict Detection

To ensure data integrity, the system implements robust, application-layer conflict detection within the MediatR Command Handlers before persisting any `TimetableEntry`.

**Validation Rules Enforced (`CreateTimetableEntryCommandHandler`):**
1.  **Teacher Overlap Prevention:** The system verifies that the selected `Teacher` is not already scheduled to teach *any* other section during the given `TimeSlot` within the same `DayOfWeek` and `AcademicYearId`.
2.  **Section Overlap Prevention:** The system verifies that the target `Section` does not already have a subject scheduled for the given `TimeSlot`.
3.  **Break Slot Protection:** The system inherently treats "Break" slots as read-only intervals where instructional entries cannot be assigned.

*If any overlaps are detected, the backend returns a clear HTTP 400 Bad Request with a specific error message (e.g., "The teacher is already teaching another section at this time."), which is subsequently displayed on the frontend.*

---

## 3. Backend API (`TimetableController`)

The RESTful API exposes endpoints to manage both slots and entries.

### Time Slots
*   `GET /api/timetable/slots?academicYearId={id}`: Retrieves all slots for the active session, sorted chronologically.
*   `POST /api/timetable/slots`: Creates a new period or break.
*   `DELETE /api/timetable/slots/{id}`: Removes a slot (and cascadingly, any associated entries).

### Timetable Entries
*   `GET /api/timetable/section/{sectionId}?academicYearId={id}`: Returns the complete weekly schedule for a specific class section. Includes materialized names (`SubjectName`, `TeacherName`) via DTO projections.
*   `GET /api/timetable/teacher/{teacherId}?academicYearId={id}`: *(Available for future Teacher Portal)* Returns the weekly schedule for a specific teacher.
*   `POST /api/timetable/entries`: Assigns a subject/teacher to a slot, triggering conflict validation.
*   `DELETE /api/timetable/entries/{id}`: Unassigns a slot.

---

## 4. Frontend Implementation

The frontend is housed entirely within `TimetablePage.tsx` and utilizes `React Query` for aggressive caching and optimistic UI updates.

### UI Components & Interactivity
*   **Global Session Awareness**: Listens to the `useAcademicYear()` hook. Modifying the session in the global header automatically invalidates queries and re-fetches the timetable for the newly selected year.
*   **Premium Grid Layout**:
    *   Constructs a dynamic tabular view crossing predefined `DAYS_OF_WEEK` (columns) against ascending `TimeSlots` (rows).
    *   Features CSS-driven micro-animations (`hover:scale-[1.03]`, shadow elevation) when hovering over assigned subject blocks.
    *   Highlights the column corresponding to the "Current Day" with an animated pulsing indicator.
*   **Data Entry Dialogs**:
    *   **Manage Time Slots Dialog**: Allows the creation/deletion of periods and breaks.
    *   **Assign Subject Dialog**: Context-aware modal that appears when an empty slot is clicked. Offers dropdowns to select Teachers and Subjects, and handles the display of backend conflict errors.
*   **Smooth Loading States**: Implements unified table `LoadingSkeleton` overlays to mask network latencies when switching between different class sections.

---

## 5. Security & Roles
The module is protected via route guards (`ProtectedRoute` in `App.tsx`). Access is granted as follows:
*   **Admin / Clerk:** Full read/write access to configure slots and schedules.
*   **Teacher:** (Currently scoped to the same UI, but ideally read-only in a future iteration to view their personal schedule).
