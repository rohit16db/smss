# Implementation Plan: Teacher, Fee, and Attendance Management

**Feature**: 002-teacher-fee-attendance  
**Created**: January 12, 2026  
**Phase**: 3  
**Status**: Ready for Implementation  
**Based on**: [spec.md](./spec.md), [database-schema.md](./database-schema.md), [api-endpoints.md](./api-endpoints.md), [ui-components.md](./ui-components.md)

---

## Tech Stack

### Backend
- **Framework**: ASP.NET Core 10
- **Architecture**: Clean Architecture + CQRS (MediatR 14.0.0)
- **ORM**: Entity Framework Core 10.0.1
- **Database**: PostgreSQL 15
- **Authentication**: JWT (existing from Phase 2)
- **Validation**: FluentValidation 12.1.1
- **API**: REST with Swagger/OpenAPI

### Frontend
- **Framework**: React 18 + Vite
- **UI Library**: Material-UI (MUI) v5
- **State Management**: React Query (TanStack Query)
- **Form Management**: React Hook Form + Zod
- **HTTP Client**: Axios

### Project Structure
```
backend/src/
├── SMS.Domain/          # Entities, Enums, Interfaces
├── SMS.Application/     # CQRS Commands/Queries, Handlers, DTOs, Validators
├── SMS.Infrastructure/  # EF Core, JWT Service, Database Context
└── SMS.API/             # Controllers, Middleware, Startup Config

frontend/src/
├── pages/               # Page components
├── components/          # Reusable UI components
├── forms/               # Form components
├── modals/              # Modal dialogs
├── reports/             # Report components
├── services/            # API service layer
├── hooks/               # Custom React hooks
├── contexts/            # React contexts
└── types/               # TypeScript types
```

---

## Feature Overview

This phase implements three interconnected features:

### 1. Teacher Management (Foundational)
- CRUD operations on teacher records
- Teacher-to-class assignments
- Qualification and experience tracking
- Active/Inactive status management

### 2. Fee Management (Core Business)
- Fee structure definition (flexible categorization)
- Student-wise fee assignment
- Payment tracking (partial, full, overdue)
- Financial reporting and outstanding fees tracking

### 3. Attendance Management (Operational)
- Daily student attendance marking
- Student attendance reporting and analysis
- Teacher attendance tracking
- Bonus eligibility calculation based on attendance

---

## Implementation Strategy

### MVP Scope (Minimum Viable Product)
Focus on Phase 1 features (P1 stories) that unlock other functionality:
1. **Teacher CRUD & Assignment** (User Story 1)
2. **Fee Structure & Collection** (User Story 2)
3. **Student Attendance** (User Story 3)

These three features are independent and cover core school operations.

### Phase 2 Implementation (P2 Stories)
After MVP validation:
4. **Teacher Attendance** (User Story 4)
5. **Financial Dashboard** (User Story 5)

### Phase 3+ Enhancement (P3)
6. **Attendance-to-Salary Integration** (User Story 6)

---

## Layer-by-Layer Implementation

### Domain Layer (Zero Dependencies)
Define all entities, enums, and value objects:
- Teacher entity (11 fields)
- TeacherAssignment entity (8 fields)
- FeeStructure entity (7 fields)
- FeeStructureCategory entity (6 fields)
- StudentFee entity (8 fields)
- FeePayment entity (8 fields)
- StudentAttendance entity (10 fields)
- TeacherAttendance entity (8 fields)
- Enums: AttendanceStatus, PaymentMethod, FeeFrequency

### Application Layer (Depends on Domain)
Implement CQRS pattern with MediatR:
- Commands (Create, Update, Delete operations)
- Queries (Read operations with filtering/pagination)
- Handlers (Business logic implementation)
- Validators (FluentValidation rules)
- DTOs (Request/response models)

### Infrastructure Layer (Database & Services)
- EF Core entity configurations (fluent API)
- Database migrations
- Repository implementations (if needed)
- External service integrations

### API Layer (REST Endpoints)
- Controllers for each feature
- JWT authorization
- Swagger documentation
- Error handling middleware

### Frontend Layer (React Components)
- Page components (Lists, Forms)
- Reusable UI components
- Forms with validation
- Data fetching with React Query
- State management with hooks

---

## Database Migrations

Three separate migrations (one per feature group):

1. **AddTeacherManagement**: Create `teachers`, `teacher_assignments` tables
2. **AddFeeManagement**: Create `fee_structures`, `fee_structure_categories`, `student_fees`, `fee_payments` tables
3. **AddAttendanceManagement**: Create `student_attendances`, `teacher_attendances` tables

Each migration includes:
- Table definitions with constraints
- Indexes for performance
- Audit trail fields (created_at, updated_at, created_by, updated_by)
- Foreign key relationships with cascade policies

---

## Data Flow & Integration Points

### Teacher Assignment Flow
1. Admin creates teacher (API: POST /api/v1/teachers)
2. Admin assigns teacher to class (API: POST /api/v1/teachers/{id}/assignments)
3. Teachers appear in class roster
4. Attendance marked by assigned teacher
5. Attendance report links to teacher assignment

### Fee Collection Flow
1. Admin creates fee structure (API: POST /api/v1/fee-structures)
2. Admin assigns structure to student (API: POST /api/v1/students/{studentId}/fees)
3. System generates payment schedule
4. Admin records payments (API: POST /api/v1/students/{studentId}/fee-payments)
5. Dashboard shows collection status
6. Outstanding fees report generated

### Attendance Flow
1. Teacher marks daily attendance (API: POST /api/v1/attendance/student)
2. System stores with timestamp and teacher reference
3. Attendance can be edited with full audit trail
4. Reports generated per student/class/month
5. Low attendance flagged in student detail

---

## Success Criteria & Testability

Each user story is independently testable:

| Story | MVP Complete When | Test Criteria |
|-------|------------------|---------------|
| 1. Teacher CRUD | CRUD endpoints work, assignments created, no duplicates | Create teacher, assign to class, verify in list |
| 2. Fee Structure | Structures created, students assigned, payments tracked | Define fee, assign to student, record payment, verify status |
| 3. Student Attendance | Daily attendance marked, reports generated, low attendance flagged | Mark attendance, generate report, verify percentages |
| 4. Teacher Attendance | Daily tracking works, reports generated for payroll | Mark teacher attendance, generate payroll report |
| 5. Financial Dashboard | Summary cards and reports show correct aggregates | Create fees, record payments, verify dashboard totals |
| 6. Attendance-to-Salary | Bonus eligibility calculated correctly | Generate salary report with attendance calculation |

---

## Dependencies & Sequencing

### Hard Dependencies (Must Complete Before)
1. **Phase 2 Database**: Users and Students tables (existing)
2. **Phase 2 Authentication**: JWT auth and User endpoints (existing)

### Feature Dependencies (Ordering)
1. **Teacher Management** → No dependencies on other Phase 3 features
2. **Fee Management** → Depends on Student entity (Phase 2, exists)
3. **Student Attendance** → Depends on Teacher & Class entities
4. **Teacher Attendance** → Depends on Teacher entity (Story 1)
5. **Financial Dashboard** → Depends on Fee Management (Story 2)
6. **Attendance-to-Salary** → Depends on Teacher Attendance (Story 4)

### Parallel Implementation Opportunities
- Stories 1, 2, 3 can be implemented in parallel (different features)
- Within each story, backend and frontend can be developed in parallel
- Different team members can work on Teacher, Fee, and Attendance features simultaneously

---

## Key Design Decisions

### Naming Convention
- Database: `snake_case` (e.g., `teacher_assignments`)
- Code: `PascalCase` classes, `camelCase` properties
- API URLs: `/api/v1/resource` (RESTful)

### Soft Deletes
- Teachers marked `is_active = false` rather than deleted
- Assignments use `removal_date = NULL` for active assignments
- Preserves audit trail and referential integrity

### Fee Calculation
- Categories stored separately in `fee_structure_categories`
- `total_amount` cached in `student_fees` for query performance
- Partial payments allowed, credit applied to next period

### Attendance Status
- Four statuses: present, absent, leave, unexcused
- Full edit history maintained
- Timestamp stored for each mark

### Audit Trail
- All tables include: `created_at`, `updated_at`, `created_by`, `updated_by`
- Immutable append-only for financial transactions (fee payments)
- Edit history in separate records (e.g., attendance edits)

### Indexing Strategy
- Indexes on foreign keys and frequently queried columns
- Composite indexes for common filter combinations
- Covering indexes for report queries
- See [database-schema.md](./database-schema.md) for detailed index list

---

## Testing Strategy

### Unit Testing
- CQRS handlers tested in isolation
- Validators tested with valid/invalid inputs
- Business logic (fee calculation, attendance percentage) tested

### Integration Testing
- API endpoints tested end-to-end
- Database migrations tested for rollback
- Authorization verified (admin-only operations)

### Manual Testing
- Form validation (client and server)
- Pagination and sorting
- Filter combinations
- Error handling and recovery
- Edge cases (mid-month assignments, reversals, etc.)

---

## Rollout Plan

### Phase 3 Iteration 1: MVP (Teacher + Fee + Student Attendance)
- Backend: Entities, CQRS handlers, API endpoints
- Database: First migration (teachers, assignments)
- Frontend: Teacher list, form, detail; Fee structure and assignment; Attendance marking
- Duration: 2 weeks

### Phase 3 Iteration 2: Phase 2 Features (Teacher Attendance + Dashboard)
- Backend: Teacher attendance endpoints, dashboard aggregation
- Database: Second migration (fees); Third migration (attendance)
- Frontend: Teacher attendance page, dashboard cards, financial reports
- Duration: 1 week

### Phase 3 Iteration 3: Enhancements (Salary Integration)
- Backend: Bonus eligibility calculations
- Frontend: Salary report with attendance adjustments
- Duration: 3-5 days

---

## Assumptions

1. **Academic Calendar**: Schools operate Jan-Dec per spec.md (assumption S5)
2. **Working Days**: 240 working days/year, 22 days/month (spec.md S6)
3. **Fee Frequency**: Monthly, quarterly, yearly options; no complex multi-frequency structures
4. **Attendance Precision**: Daily granularity (no period-wise or session-wise)
5. **Time Zones**: All operations in local school timezone (spec.md S8)
6. **Payment Method**: Cash, check, bank transfer (no external payment gateway in Phase 2)
7. **Teacher Qualifications**: Text field only (no validation against official credentials)
8. **Bonus Policy**: 90% attendance threshold for full bonus (configurable later)

---

## Open Questions Resolved in Design

1. **Fee Pro-rationing**: Handled via custom amount in fee assignment
2. **Grace Period**: 7-day grace for late attendance entry
3. **Late Penalties**: Not in Phase 3 scope (future enhancement)
4. **Batch Operations**: UI supports bulk fee assignment and batch attendance
5. **Holiday Handling**: Not tracked separately (manually excluded from working days)
6. **Subject Expertise**: Advisory validation only (not blocking)

---

## Risk Mitigation

| Risk | Impact | Mitigation |
|------|--------|-----------|
| Complex fee calculations | Implementation delay | Use stored calculations in `student_fees.total_amount` |
| Duplicate attendance entries | Data integrity | UNIQUE constraint on (student, class, date) |
| Attendance audit trail bloat | Performance | Separate table for edits, keep main table immutable |
| Overlapping teacher assignments | Scheduling conflicts | Pre-check in API before allowing assignment |
| Mid-month fee assignments | Incorrect billing | Allow custom amount in fee assignment modal |
| Database migration failures | Downtime | Test migrations on staging first, have rollback plan |

---

## File Structure After Implementation

```
backend/src/SMS.Domain/
├── Entities/
│   ├── Teacher.cs
│   ├── TeacherAssignment.cs
│   ├── FeeStructure.cs
│   ├── FeeStructureCategory.cs
│   ├── StudentFee.cs
│   ├── FeePayment.cs
│   ├── StudentAttendance.cs
│   └── TeacherAttendance.cs
├── Enums/
│   ├── AttendanceStatus.cs
│   ├── PaymentMethod.cs
│   ├── FeeFrequency.cs
│   └── /* ...existing enums... */
└── /* ...existing domain files... */

backend/src/SMS.Application/
├── Teacher/
│   ├── Commands/
│   ├── Queries/
│   ├── Handlers/
│   ├── DTOs/
│   └── Validators/
├── Fee/
│   ├── Commands/
│   ├── Queries/
│   ├── Handlers/
│   ├── DTOs/
│   └── Validators/
├── Attendance/
│   ├── Commands/
│   ├── Queries/
│   ├── Handlers/
│   ├── DTOs/
│   └── Validators/
└── /* ...existing application files... */

backend/src/SMS.API/
├── Controllers/
│   ├── TeachersController.cs
│   ├── FeesController.cs
│   ├── AttendanceController.cs
│   └── DashboardController.cs
└── /* ...existing API files... */

frontend/src/
├── pages/
│   ├── teachers/
│   ├── fees/
│   └── attendance/
├── components/
│   ├── teachers/
│   ├── fees/
│   ├── attendance/
│   ├── dashboard/
│   └── shared/
├── forms/
├── modals/
├── reports/
├── services/
│   ├── teacherService.ts
│   ├── feeService.ts
│   ├── attendanceService.ts
│   └── dashboardService.ts
├── types/
│   ├── teacher.ts
│   ├── fee.ts
│   ├── attendance.ts
│   └── dashboard.ts
└── /* ...existing frontend files... */
```

---

## Next Steps

1. **Run check-prerequisites.ps1** to validate specification quality
2. **Generate tasks.md** with actionable task breakdown
3. **Assign tasks** to development team members
4. **Begin Phase 3 Implementation** starting with Teacher Management (Story 1)
5. **Sprint Planning**: Allocate 2 weeks for MVP (Stories 1-3)

---

## References

- **Specification**: [spec.md](./spec.md)
- **Database Design**: [database-schema.md](./database-schema.md)
- **API Design**: [api-endpoints.md](./api-endpoints.md)
- **UI Design**: [ui-components.md](./ui-components.md)
- **PRD**: [../../PRD/prd_school_management_software.md](../../PRD/prd_school_management_software.md)
- **Phase 2 Code**: [../../backend/src](../../backend/src)
