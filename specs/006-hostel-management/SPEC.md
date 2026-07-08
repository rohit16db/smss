# Feature Specification: Hostel Management

## 1. Overview
The system currently has no concept of student boarding/hostel accommodation. This feature introduces **Hostel Management**: tracking hostel buildings, their rooms and beds, allocating students to a bed for an academic year, and automatically charging the corresponding hostel fee through the existing Fees module.

This is primarily a net-new module, but it also requires one small, necessary change to the existing **Students** module: `Student` currently has no `Gender` field (only staff `UserProfile` does), and the gender-segregation rule in Section 4 depends on it. The closest existing structural precedent for the new module is the **Transport** module (a resource + student-assignment domain with an associated fee), and this spec follows its conventions (Enrollment-scoped assignment, auto-synced fee) wherever they apply.

### 1.1 In Scope (v1)
- Hostel / Room / Bed inventory management.
- Allocating, transferring, and vacating a student's bed for an academic year.
- Room-type-based hostel fee, auto-charged to the student's fee record on allocation.
- Occupancy reporting (vacant/occupied beds per hostel/room).

### 1.2 Out of Scope (v1)
- Attendance / leave / in-out (check-in/check-out) tracking for boarders.
- Complaints or maintenance ticketing against a room.
- Mess / meal plan management.
- A new "Warden" role (existing roles are reused — see Section 6).
- Waiting lists for full hostels.
- Bed-level "Maintenance/Out of Service" status (beds are binary Available/Occupied in v1).
- Showing hostel/room/bed info on the existing Student detail page.

## 2. Impacted Existing Module: Students
- Add `Gender` (enum: Male, Female) to `SMS.Domain.Entities.Student`, required going forward.
- New EF Core migration for the added column; existing rows will need a value — exact backfill approach (nullable-then-required vs. required-with-default) to be decided during implementation planning, consistent with how the project has handled prior non-nullable additions (see `specs/005-academic-years` for precedent: "existing data will be wiped out manually" was acceptable pre-launch — confirm whether that still applies here or if production data now exists).
- `CreateStudentRequest`/`UpdateStudentRequest` DTOs, their FluentValidation validators, and the student create/edit form in `StudentsPage.tsx` (or its associated form component) gain a required Gender field.
- No other Student behavior changes; this field is additive and only consumed by Hostel's allocation rule (Section 4.3) in v1.

## 3. Domain Entities

### Hostel (`SMS.Domain.Entities.Hostel` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| Name | string | e.g. "Boys Hostel A" |
| Gender | enum (Male, Female) | Used to enforce Section 4.3 |
| Address | string? | |
| Description | string? | |
| IsActive | bool | |
| *Nav* | Rooms (collection) |

### RoomType (`SMS.Domain.Entities.RoomType` : BaseEntity)
A shared catalog, not per-hostel — the same type (e.g. "AC") can be used across multiple hostels.
| Property | Type | Notes |
|----------|------|-------|
| Name | string | e.g. "Standard", "AC", "Single" |
| FeeAmount | decimal | Annual/per-year hostel fee for this type — see Section 5 |
| Description | string? | |
| IsActive | bool | |

### Room (`SMS.Domain.Entities.Room` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| HostelId | Guid | FK |
| RoomNumber | string | |
| Floor | string? | |
| RoomTypeId | Guid | FK |
| Capacity | int | Number of beds; Bed rows are auto-created to match this count when the Room is created |
| *Nav* | Hostel, RoomType, Beds (collection) |

### Bed (`SMS.Domain.Entities.Bed` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| RoomId | Guid | FK |
| BedNumber | string | e.g. "1", "A" |
| *Nav* | Room |

Occupancy is **derived**, not stored: a bed is occupied if it has a `StudentHostelAllocation` with `Status = Active`. This avoids keeping a denormalized flag in sync (consistent with Principle V, Simplicity, in the project constitution).

### StudentHostelAllocation (`SMS.Domain.Entities.StudentHostelAllocation` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| EnrollmentId | Guid | FK — scopes the allocation to a specific academic year, mirroring `StudentTransportAssignment` |
| BedId | Guid | FK |
| AllocatedDate | DateOnly | |
| VacatedDate | DateOnly? | Set when vacated/transferred out |
| Status | enum (Active, Vacated) | |
| *Nav* | Enrollment, Bed |

## 4. Business Rules
1. A bed can have at most one allocation with `Status = Active` at a time.
2. An enrollment can have at most one allocation with `Status = Active` at a time (a student can't hold two beds in the same academic year).
3. **Gender rule**: allocation is rejected if the student's gender does not match the target hostel's `Gender`.
4. **Auto fee on allocation**: allocating a bed adds a "Hostel" fee charge to the student's fee record for that academic year (via the existing Fees module — see Section 5), amount equal to the bed's Room's RoomType.FeeAmount.
5. **Vacate**: ends the allocation (`Status = Vacated`, `VacatedDate` set) and frees the bed. Any hostel fee charge already added is **left unchanged** — no automatic refund or proration. Staff adjust it manually via existing Fees screens if needed.
6. **Transfer**: moving a student from one bed to another is a single atomic action equivalent to Vacate (old bed) + Allocate (new bed). Per rules 4 and 5, this means: the original fee charge is left as-is, and a **new, separate** hostel fee charge is added for the new bed's room type. A same-year transfer between different room types can therefore result in two hostel charges on the student's fee record — this is accepted behavior for v1, not a defect.
7. A Room's capacity cannot be reduced below its number of currently-active allocations.
8. Deleting a Hostel, Room, or Bed is blocked (via a `BusinessRuleValidationException`, consistent with other modules) while it has any active allocation underneath it. Deletes are soft (`IsActive = false` / existing soft-delete pattern), consistent with Students.

## 5. Fees Integration
- Hostel fee amount is configured per **RoomType**, not per hostel or per individual room.
- On successful allocation, the handler adds a "Hostel" fee line to the student's existing `StudentFee` record for the current academic year (exact plumbing — new `FeeStructureCategory` vs. a dedicated field — to be finalized against the current `StudentFee`/`FeeStructureCategory` schema during implementation planning).
- No new fee-specific UI is introduced; the Accountant role continues to view/collect this charge through the existing Fees screens. This satisfies the "reuse existing roles" decision in Section 6 — hostel fee handling doesn't require a hostel-specific fee screen.
- Vacating/transferring never removes or edits a previously-added charge (Rules 5–6).

## 6. Access Control
No new role is introduced. Hostel setup and allocation reuse the same policies as the Students/Academic modules:
- **Read** endpoints (list hostels/rooms/beds/allocations, occupancy report) → `AcademicViewAccess` policy (Admin, Clerk, Teacher).
- **Write** endpoints (create/update/delete Hostel/RoomType/Room/Bed, allocate/transfer/vacate) → `AcademicAccess` policy (Admin, Clerk).
- Accountant has no dedicated hostel policy; they see the resulting fee charge through existing Fees-scoped endpoints/screens.

## 7. API Endpoints (indicative — finalized during implementation planning)

**Controller(s)**: `HostelsController` — route base `api/hostels`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/hostels` | List hostels |
| POST | `/api/hostels` | Create hostel |
| PUT | `/api/hostels/{id}` | Update hostel |
| DELETE | `/api/hostels/{id}` | Soft-delete hostel (blocked if active allocations exist beneath it) |
| GET | `/api/hostels/room-types` | List room types |
| POST | `/api/hostels/room-types` | Create room type |
| PUT | `/api/hostels/room-types/{id}` | Update room type |
| GET | `/api/hostels/{hostelId}/rooms` | List rooms in a hostel (with bed occupancy) |
| POST | `/api/hostels/{hostelId}/rooms` | Create room (auto-creates its beds per Capacity) |
| PUT | `/api/hostels/rooms/{id}` | Update room |
| DELETE | `/api/hostels/rooms/{id}` | Soft-delete room (blocked if active allocations exist) |
| GET | `/api/hostels/allocations` | List/search allocations (filterable by hostel/room/status) |
| POST | `/api/hostels/allocations` | Allocate a student (EnrollmentId) to a bed |
| POST | `/api/hostels/allocations/{id}/transfer` | Transfer allocation to a different bed |
| POST | `/api/hostels/allocations/{id}/vacate` | Vacate an allocation |
| GET | `/api/hostels/occupancy` | Occupancy report (per hostel/room: total/occupied/vacant beds) |
| GET | `/api/hostels/student/{enrollmentId}` | Current hostel allocation status for a student |

All list endpoints are paginated, per the project constitution's Scalability principle.

## 8. Frontend Requirements
- **Hostel Setup page** (`HostelSetupPage.tsx`) — manage Hostels, Room Types, Rooms, and their Beds (CRUD), with a per-room bed grid showing occupied/vacant state.
- **Allocation page** (`HostelAllocationPage.tsx`) — search/list current allocations; allocate a student to a vacant bed; transfer; vacate.
- **Occupancy dashboard** — occupancy % and vacant-bed counts per hostel/room (may be a section within the Setup page or its own view; decided during implementation planning).
- New `hostelService.ts` in `frontend/src/services/`, following the per-module service-file pattern used by Transport/Payroll, built on the shared `api` Axios instance.
- New nav entry and routes in `App.tsx`, guarded consistent with Section 6 (role names in the frontend's local `roleMap`, not the raw backend enum — see the existing Teacher/"Staff" naming quirk documented for Transport-style routes).
- Not in scope: any change to the existing `StudentsPage.tsx` detail view.

## 9. Non-Functional / Constitution Compliance
- **Clean Architecture / CQRS**: new module under `SMS.Application/Features/Hostel/{Commands,Queries,DTOs,Handlers,Validators}`, entities in `SMS.Domain/Entities`, EF configuration under `SMS.Infrastructure/Data/Configurations`, following the Transport module's file layout.
- **Scalability**: all list endpoints paginated; Bed occupancy computed via indexed FK lookups (`StudentHostelAllocation.BedId`, `.EnrollmentId`).
- **Security**: standard JWT + policy-based authorization (Section 6); no new PII beyond what already exists on Student/Enrollment.
- **Simplicity/YAGNI**: no bed-level status field (derived instead), no waiting-list logic, no proration engine — all deferred per Section 1.2.

## 10. Open Items for Implementation Planning
These are intentionally left for the implementation-planning phase rather than assumed here:
- Exact schema change (if any) to `StudentFee`/`FeeStructureCategory` needed to represent a "Hostel" charge line (Section 5).
- Whether Hostel endpoints live in one consolidated `HostelsController` or are split (e.g. a separate `HostelAllocationsController`), matching whichever convention is cleaner given the final route/DTO design.
- Database table naming convention (snake_case vs PascalCase) — existing modules are inconsistent (Students uses snake_case, Transport/Inventory use EF defaults); pick one for the new tables and document the choice.
