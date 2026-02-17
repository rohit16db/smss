# Tasks: Teacher, Fee, and Attendance Management

**Feature**: 002-teacher-fee-attendance  
**Created**: January 12, 2026  
**Input**: [plan.md](./plan.md), [spec.md](./spec.md), [database-schema.md](./database-schema.md), [api-endpoints.md](./api-endpoints.md), [ui-components.md](./ui-components.md)  
**Prerequisites**: Phase 2 backend code (complete), Phase 2 database (Users, Students tables exist)

---

## Format: `- [ ] [ID] [P?] [Story] Description - file path`

- **Checkbox**: Always `- [ ]`
- **[ID]**: Task identifier (T001, T002, etc.)
- **[P]**: Mark if parallelizable (different files, no dependencies)
- **[Story]**: User story label (US1, US2, US3, etc.) for story phase tasks only
- **Description**: Clear action with exact file path

---

## Phase 1: Setup (Project Initialization)

**Purpose**: Initialize project structure and prepare for database migrations

- [ ] T001 Create feature branch structure with directories for backend/frontend
- [ ] T002 Create database migrations directory structure in `backend/src/SMS.Infrastructure/Migrations/`
- [ ] T003 [P] Verify .NET 10 and PostgreSQL connectivity from Phase 2 setup
- [ ] T004 [P] Verify React 18, Vite, MUI, and React Query dependencies in frontend

---

## Phase 2: Foundational (Infrastructure & Database)

**Purpose**: Core database and backend infrastructure - MUST complete before user story work

**⚠️ CRITICAL**: No user story implementation can begin until Phase 2 is complete.

### Database Infrastructure

- [ ] T005 Create migration `20260112_AddTeacherManagement.cs` in `backend/src/SMS.Infrastructure/Migrations/` with teachers and teacher_assignments tables per database-schema.md
- [ ] T006 Create migration `20260112_AddFeeManagement.cs` in `backend/src/SMS.Infrastructure/Migrations/` with fee_structures, fee_structure_categories, student_fees, and fee_payments tables
- [ ] T007 Create migration `20260112_AddAttendanceManagement.cs` in `backend/src/SMS.Infrastructure/Migrations/` with student_attendances and teacher_attendances tables
- [ ] T008 Apply all three migrations to PostgreSQL database: `dotnet ef database update --project backend/src/SMS.Infrastructure`
- [ ] T009 Verify migration rollback functionality by testing: `dotnet ef database update -1`

### Domain Layer (Entities & Enums)

- [ ] T010 [P] Create `Teacher.cs` entity in `backend/src/SMS.Domain/Entities/` with 11 fields and audit trail
- [ ] T011 [P] Create `TeacherAssignment.cs` entity in `backend/src/SMS.Domain/Entities/`
- [ ] T012 [P] Create `FeeStructure.cs` entity in `backend/src/SMS.Domain/Entities/`
- [ ] T013 [P] Create `FeeStructureCategory.cs` entity in `backend/src/SMS.Domain/Entities/`
- [ ] T014 [P] Create `StudentFee.cs` entity in `backend/src/SMS.Domain/Entities/`
- [ ] T015 [P] Create `FeePayment.cs` entity in `backend/src/SMS.Domain/Entities/`
- [ ] T016 [P] Create `StudentAttendance.cs` entity in `backend/src/SMS.Domain/Entities/`
- [ ] T017 [P] Create `TeacherAttendance.cs` entity in `backend/src/SMS.Domain/Entities/`
- [ ] T018 [P] Create enums in `backend/src/SMS.Domain/Enums/`: `AttendanceStatus.cs`, `PaymentMethod.cs`, `FeeFrequency.cs`

### Infrastructure Layer (EF Core Configurations)

- [ ] T019 [P] Create `TeacherConfiguration.cs` in `backend/src/SMS.Infrastructure/Configurations/` with snake_case mapping, indexes, and constraints
- [ ] T020 [P] Create `TeacherAssignmentConfiguration.cs` in `backend/src/SMS.Infrastructure/Configurations/`
- [ ] T021 [P] Create `FeeStructureConfiguration.cs` in `backend/src/SMS.Infrastructure/Configurations/`
- [ ] T022 [P] Create `FeeStructureCategoryConfiguration.cs` in `backend/src/SMS.Infrastructure/Configurations/`
- [ ] T023 [P] Create `StudentFeeConfiguration.cs` in `backend/src/SMS.Infrastructure/Configurations/`
- [ ] T024 [P] Create `FeePaymentConfiguration.cs` in `backend/src/SMS.Infrastructure/Configurations/`
- [ ] T025 [P] Create `StudentAttendanceConfiguration.cs` in `backend/src/SMS.Infrastructure/Configurations/` with unique constraint on (student_id, class_id, date)
- [ ] T026 [P] Create `TeacherAttendanceConfiguration.cs` in `backend/src/SMS.Infrastructure/Configurations/`
- [ ] T027 Update `ApplicationDbContext.cs` in `backend/src/SMS.Infrastructure/` to include DbSets for all new entities and call ApplyConfigurationsFromAssembly()

### Verify Foundation

- [ ] T028 Run `dotnet build` from `backend/` and verify no compilation errors
- [ ] T029 Run `dotnet ef database update` and verify all tables created successfully in PostgreSQL

**✅ Checkpoint**: Database and domain/infrastructure layers complete - ready for user story implementation

---

## Phase 3: User Story 1 - Teacher CRUD & Class Assignment (Priority: P1) 🎯 MVP

**Goal**: Enable administrators to create teacher records, manage their qualifications, and assign them to classes without conflicts.

**Independent Test**: Create a teacher, assign to a class, verify assignment appears in list, attempt duplicate assignment (should fail).

### Backend Implementation for US1

**Application Layer**

- [ ] T030 [P] [US1] Create `TeacherDTOs.cs` in `backend/src/SMS.Application/Teacher/DTOs/` with: CreateTeacherRequest, UpdateTeacherRequest, TeacherDto, TeacherAssignmentDto, PagedTeacherResult
- [ ] T031 [P] [US1] Create teacher commands in `backend/src/SMS.Application/Teacher/Commands/` with: CreateTeacherCommand, UpdateTeacherCommand, AssignTeacherToClassCommand, RemoveTeacherAssignmentCommand
- [ ] T032 [P] [US1] Create teacher queries in `backend/src/SMS.Application/Teacher/Queries/` with: GetTeacherByIdQuery, GetAllTeachersQuery (with pagination/filtering)
- [ ] T033 [US1] Create teacher validators in `backend/src/SMS.Application/Teacher/Validators/`: CreateTeacherValidator, UpdateTeacherValidator, AssignTeacherValidator
- [ ] T034 [US1] Create teacher command handlers in `backend/src/SMS.Application/Teacher/Handlers/` for CreateTeacher, UpdateTeacher, AssignTeacherToClass, RemoveTeacherAssignment
- [ ] T035 [US1] Create teacher query handlers in `backend/src/SMS.Application/Teacher/Handlers/` for GetTeacherById, GetAllTeachers with search/filter/sort/pagination

**API Layer**

- [ ] T036 [US1] Create `TeachersController.cs` in `backend/src/SMS.API/Controllers/` with 6 endpoints: POST /api/v1/teachers, GET /api/v1/teachers/{id}, GET /api/v1/teachers, PUT /api/v1/teachers/{id}, POST /api/v1/teachers/{id}/assignments, DELETE /api/v1/teachers/{id}/assignments/{assignmentId}
- [ ] T037 [US1] Add Swagger/OpenAPI documentation comments to TeachersController
- [ ] T038 [US1] Test all endpoints manually via Swagger UI - verify requests/responses match api-endpoints.md

### Frontend Implementation for US1

- [ ] T039 [P] [US1] Create teacher service in `frontend/src/services/teacherService.ts` with API calls for all teacher endpoints
- [ ] T040 [P] [US1] Create teacher types in `frontend/src/types/teacher.ts` with TypeScript interfaces for Teacher, TeacherAssignment, CreateTeacherRequest
- [ ] T041 [P] [US1] Create `TeacherListPage.tsx` in `frontend/src/pages/teachers/` with search, filter, pagination, and sorting
- [ ] T042 [P] [US1] Create `TeacherForm.tsx` in `frontend/src/components/forms/` with React Hook Form, Zod validation, create/edit modes
- [ ] T043 [P] [US1] Create `TeacherDetailCard.tsx` in `frontend/src/components/cards/` showing teacher info and assigned classes
- [ ] T044 [P] [US1] Create `TeacherAssignmentModal.tsx` in `frontend/src/components/modals/` for assigning teacher to class
- [ ] T045 [US1] Create `TeacherAssignmentList.tsx` in `frontend/src/components/tables/` showing current assignments
- [ ] T046 [US1] Integrate components into main app navigation and routing
- [ ] T047 [US1] Test teacher creation form with valid/invalid inputs
- [ ] T048 [US1] Test teacher assignment creation and duplicate prevention

**✅ Checkpoint: User Story 1 Complete** - Teachers can be created, edited, assigned to classes; all functionality independent and testable

---

## Phase 4: User Story 2 - Fee Structure & Collection Tracking (Priority: P1) 🎯 MVP

**Goal**: Enable administrators to define flexible fee structures (with multiple categories), assign to students, track payments, and identify outstanding dues.

**Independent Test**: Create fee structure with categories, assign to student, record payment, verify fee status shows correct amounts.

### Backend Implementation for US2

**Application Layer**

- [ ] T049 [P] [US2] Create `FeeDTOs.cs` in `backend/src/SMS.Application/Fee/DTOs/` with: CreateFeeStructureRequest, FeeStructureDto, StudentFeeDto, FeePaymentDto, StudentFeeStatusDto, OutstandingFeeDto, PagedFeeResult
- [ ] T050 [P] [US2] Create fee commands in `backend/src/SMS.Application/Fee/Commands/`: CreateFeeStructureCommand, AssignFeeStructureToStudentCommand, RecordFeePaymentCommand, ReverseFeePaymentCommand
- [ ] T051 [P] [US2] Create fee queries in `backend/src/SMS.Application/Fee/Queries/`: GetFeeStructureByIdQuery, GetAllFeeStructuresQuery, GetStudentFeeStatusQuery, GetOutstandingFeesReportQuery
- [ ] T052 [US2] Create fee validators in `backend/src/SMS.Application/Fee/Validators/`: CreateFeeStructureValidator, AssignFeeValidator, PaymentValidator with amount validation
- [ ] T053 [US2] Create fee command handlers in `backend/src/SMS.Application/Fee/Handlers/` implementing fee logic: total calculation, period generation, payment tracking
- [ ] T054 [US2] Create fee query handlers in `backend/src/SMS.Application/Fee/Handlers/` for retrieval and reporting

**API Layer**

- [ ] T055 [US2] Create `FeesController.cs` in `backend/src/SMS.API/Controllers/` with 8 endpoints per api-endpoints.md
- [ ] T056 [US2] Add Swagger documentation to FeesController

### Frontend Implementation for US2

- [ ] T057 [P] [US2] Create fee service in `frontend/src/services/feeService.ts`
- [ ] T058 [P] [US2] Create fee types in `frontend/src/types/fee.ts`
- [ ] T059 [P] [US2] Create `FeeStructureListPage.tsx` in `frontend/src/pages/fees/`
- [ ] T060 [P] [US2] Create `FeeStructureForm.tsx` in `frontend/src/components/forms/` with dynamic category addition
- [ ] T061 [P] [US2] Create `StudentFeeAssignmentModal.tsx` in `frontend/src/components/modals/` with date range and bulk assignment
- [ ] T062 [P] [US2] Create `StudentFeeStatusCard.tsx` in `frontend/src/components/cards/` showing due/paid/outstanding
- [ ] T063 [P] [US2] Create `PaymentRecordingModal.tsx` in `frontend/src/components/modals/` for recording payments
- [ ] T064 [P] [US2] Create `OutstandingFeesReport.tsx` in `frontend/src/components/reports/`
- [ ] T065 [US2] Test fee structure creation with multiple categories and total calculation
- [ ] T066 [US2] Test student fee assignment and payment recording
- [ ] T067 [US2] Test partial payment and credit balance handling

**✅ Checkpoint: User Story 2 Complete** - Fee structures can be created with categories, assigned to students, payments recorded and tracked

---

## Phase 5: User Story 3 - Daily Student Attendance (Priority: P1) 🎯 MVP

**Goal**: Enable teachers to mark daily attendance with status (present/absent/leave/unexcused), allow editing with audit trail, generate attendance reports.

**Independent Test**: Mark attendance for a class, generate monthly report, verify percentage calculation.

### Backend Implementation for US3

**Application Layer**

- [ ] T068 [P] [US3] Create `AttendanceDTOs.cs` in `backend/src/SMS.Application/Attendance/DTOs/`: MarkStudentAttendanceRequest, StudentAttendanceDto, AttendanceReportDto, ClassAttendanceSummaryDto
- [ ] T069 [P] [US3] Create attendance commands in `backend/src/SMS.Application/Attendance/Commands/`: MarkStudentAttendanceCommand, UpdateStudentAttendanceCommand, MarkTeacherAttendanceCommand
- [ ] T070 [P] [US3] Create attendance queries in `backend/src/SMS.Application/Attendance/Queries/`: GetStudentAttendanceRecordQuery, GetClassAttendanceSummaryQuery, GetTeacherAttendanceReportQuery
- [ ] T071 [US3] Create attendance validators in `backend/src/SMS.Application/Attendance/Validators/` with status and date validation
- [ ] T072 [US3] Create attendance command handlers implementing: batch marking, edit history tracking, percentage calculation
- [ ] T073 [US3] Create attendance query handlers for retrieval and reporting with filtering/sorting

**API Layer**

- [ ] T074 [US3] Create `AttendanceController.cs` in `backend/src/SMS.API/Controllers/` with 7 endpoints per api-endpoints.md
- [ ] T075 [US3] Add Swagger documentation to AttendanceController

### Frontend Implementation for US3

- [ ] T076 [P] [US3] Create attendance service in `frontend/src/services/attendanceService.ts`
- [ ] T077 [P] [US3] Create attendance types in `frontend/src/types/attendance.ts`
- [ ] T078 [P] [US3] Create `AttendanceMarkingPage.tsx` in `frontend/src/pages/attendance/` with class/date selection and batch marking
- [ ] T079 [P] [US3] Create `AttendanceEditModal.tsx` in `frontend/src/components/modals/` for correcting past attendance with audit trail
- [ ] T080 [P] [US3] Create `StudentAttendanceReport.tsx` in `frontend/src/components/reports/` with calendar and list views
- [ ] T081 [P] [US3] Create `ClassAttendanceSummary.tsx` in `frontend/src/components/reports/` showing class and per-student breakdown
- [ ] T082 [US3] Test attendance marking for entire class with different statuses
- [ ] T083 [US3] Test attendance editing and audit trail logging
- [ ] T084 [US3] Test attendance report generation and percentage calculation

**✅ Checkpoint: User Story 3 Complete** - MVP Phase complete! Teachers can mark daily attendance, edit with history, reports show percentages and trends

---

## Phase 6: User Story 4 - Teacher Attendance Tracking (Priority: P2)

**Goal**: Enable marking teacher attendance for payroll/compliance, track percentage, and generate payroll reports.

**Independent Test**: Mark teacher attendance, generate attendance report with percentage, verify payroll calculation.

### Backend Implementation for US4

- [ ] T085 [US4] Create teacher attendance endpoints in `TeachersController.cs`: POST /api/v1/attendance/teacher (mark attendance)
- [ ] T086 [US4] Create teacher attendance handlers in `backend/src/SMS.Application/Attendance/Handlers/`
- [ ] T087 [US4] Extend `TeacherAttendanceReportQuery` for payroll period calculations and bonus eligibility

### Frontend Implementation for US4

- [ ] T088 [P] [US4] Create `TeacherAttendancePage.tsx` in `frontend/src/pages/attendance/` with marking form and history
- [ ] T089 [P] [US4] Create `TeacherAttendanceReport.tsx` in `frontend/src/components/reports/` with payroll summary
- [ ] T090 [US4] Test teacher attendance marking and payroll report generation

**✅ Checkpoint: User Story 4 Complete** - Teacher attendance tracking ready for payroll integration

---

## Phase 7: User Story 5 - Financial Dashboard & Reports (Priority: P2)

**Goal**: Provide administrators with financial overview: collection status, outstanding fees, and detailed reports.

**Independent Test**: Create fees, record some payments, verify dashboard aggregates match calculations.

### Backend Implementation for US5

**Application Layer**

- [ ] T091 [P] [US5] Create `DashboardDTOs.cs` in `backend/src/SMS.Application/Dashboard/DTOs/` with summary card data structures
- [ ] T092 [P] [US5] Create `GetDashboardSummaryQuery.cs` in `backend/src/SMS.Application/Dashboard/Queries/`
- [ ] T093 [US5] Create dashboard query handler implementing aggregations: total teachers, students, fees collected, outstanding, attendance averages

**API Layer**

- [ ] T094 [US5] Create `DashboardController.cs` in `backend/src/SMS.API/Controllers/` with GET /api/v1/dashboard/summary endpoint

### Frontend Implementation for US5

- [ ] T095 [P] [US5] Create dashboard service in `frontend/src/services/dashboardService.ts`
- [ ] T096 [P] [US5] Create `DashboardSummaryCards.tsx` in `frontend/src/components/dashboard/` displaying KPIs
- [ ] T097 [P] [US5] Create `FeesCollectionChart.tsx` in `frontend/src/components/dashboard/` with collection vs. target visualization
- [ ] T098 [P] [US5] Create `AttendanceTrendChart.tsx` in `frontend/src/components/dashboard/` showing trends
- [ ] T099 [US5] Create dashboard page linking all components together
- [ ] T100 [US5] Test dashboard aggregation accuracy and chart display

**✅ Checkpoint: User Story 5 Complete** - Dashboard provides financial visibility

---

## Phase 8: User Story 6 - Attendance-to-Salary Integration (Priority: P3)

**Goal**: Calculate bonus eligibility based on attendance percentage (90% threshold).

**Independent Test**: Generate attendance report, verify bonus eligibility calculated correctly.

### Backend Implementation for US6

- [ ] T101 [US6] Extend `TeacherAttendanceReportQuery` with bonus calculation logic
- [ ] T102 [US6] Create `GetBonusEligibilityQuery.cs` in `backend/src/SMS.Application/Payroll/` (future module)
- [ ] T103 [US6] Add bonus eligibility to teacher attendance report DTO

### Frontend Implementation for US6

- [ ] T104 [P] [US6] Create `SalaryReportWithAttendance.tsx` in `frontend/src/components/reports/`
- [ ] T105 [US6] Test salary report shows bonus eligibility based on attendance

**✅ Checkpoint: User Story 6 Complete** - Attendance feeds into payroll calculations

---

## Phase 9: Polish & Integration (Cross-Cutting Concerns)

**Goal**: Ensure all features work together seamlessly with proper error handling, logging, and validation.

### Error Handling & Validation

- [ ] T106 [P] Add global error handler middleware to catch API exceptions in `backend/src/SMS.API/Middleware/`
- [ ] T107 [P] Add validation error response formatting in `backend/src/SMS.API/Controllers/BaseController.cs`
- [ ] T108 [P] Add client-side error toast notifications in `frontend/src/components/alerts/` for all API errors
- [ ] T109 [P] Add form validation error display for all forms in `frontend/src/components/forms/`

### Logging & Monitoring

- [ ] T110 [P] Add logging to all CQRS handlers for audit trail: `backend/src/SMS.Application/*/Handlers/`
- [ ] T111 [P] Add logging to all API endpoints in controllers
- [ ] T112 [P] Add API request/response logging middleware in `backend/src/SMS.API/Middleware/`

### Data Integrity & Constraints

- [ ] T113 Verify all database constraints enforced: unique indexes, foreign keys, CHECK constraints per database-schema.md
- [ ] T114 Test concurrent operations: simultaneous attendance marking, fee payments (verify no race conditions)
- [ ] T115 Test transaction rollback: simulate payment failure, verify no orphaned records

### Integration Tests

- [ ] T116 [P] Create end-to-end test: Create teacher → Assign to class → Mark attendance → Verify in report
- [ ] T117 [P] Create end-to-end test: Create fee structure → Assign to student → Record payment → Verify status
- [ ] T118 Create dashboard integration test: Verify summary cards aggregate all features correctly
- [ ] T119 Create authentication test: Verify all endpoints require JWT token and proper authorization

### Performance Testing

- [ ] T120 [P] Test attendance marking performance with large class (500+ students)
- [ ] T121 [P] Test fee report generation with 1000+ payment records
- [ ] T122 Test pagination performance with large result sets

### Documentation

- [ ] T123 Add API documentation to all endpoints (Swagger comments already in controllers)
- [ ] T124 Create README in `backend/` explaining database setup and migration steps
- [ ] T125 Create README in `frontend/` explaining component structure and integration
- [ ] T126 Document data flow diagrams in [plan.md](./plan.md)

### Frontend UI Polish

- [ ] T127 [P] Add loading skeletons to all list pages during data fetch
- [ ] T128 [P] Add empty state messages to all lists when no data
- [ ] T129 [P] Add success/error toast notifications for all operations
- [ ] T130 [P] Implement responsive design breakpoints for mobile/tablet/desktop
- [ ] T131 [P] Add accessibility labels (ARIA) to all form inputs and buttons
- [ ] T132 [P] Test keyboard navigation on all pages (Tab, Enter, Escape)
- [ ] T133 Test color contrast ratios meet WCAG 2.1 AA standards

### Production Checklist

- [ ] T134 [P] Remove all console.log statements and debug code
- [ ] T135 [P] Add environment variable validation in `backend/src/SMS.API/Program.cs`
- [ ] T136 [P] Add environment variable validation in `frontend/` for API base URL
- [ ] T137 [P] Test all API responses match api-endpoints.md error codes
- [ ] T138 [P] Setup CORS properly in `backend/src/SMS.API/Program.cs` for frontend domain
- [ ] T139 Verify JWT token expiry and refresh token flow works correctly
- [ ] T140 Test database connection pooling and performance
- [ ] T141 Create deployment checklist and runbook

**✅ Final Checkpoint: All Features Complete & Polished** - Ready for QA and deployment

---

## Dependencies & Parallel Execution

### Hard Dependency Chain
```
Phase 2 (Foundation) → Phase 3-5 (User Stories 1-3, can run in parallel)
                    ↓
                Phase 6-7 (User Stories 4-5, after 1-3 complete)
                    ↓
                Phase 8 (User Story 6, depends on 4)
                    ↓
                Phase 9 (Polish, all parallel)
```

### Parallel Work Opportunities (Same Phase)

**Phase 2 (Foundation)**:
- T010-T018 (All entities): Can create in parallel
- T019-T026 (All configurations): Can create in parallel
- T030-T035 (US1 backend): Can start after T010, T019
- T039-T048 (US1 frontend): Can start independently after T003

**Phase 3-5 (User Stories)**:
- **All three stories (US1, US2, US3) can be developed in parallel** by different team members:
  - Developer 1: US1 (Teacher) → T030-T048
  - Developer 2: US2 (Fee) → T049-T067
  - Developer 3: US3 (Attendance) → T068-T084

**Phase 9 (Polish)**:
- T106-T112 (Error handling, logging): Can run in parallel
- T120-T122 (Performance tests): Can run in parallel
- T127-T133 (UI polish): Can run in parallel

### Suggested Team Allocation (3 Developers)

| Developer | Phase 2 | Phase 3-5 | Phase 6-7 | Phase 8 | Phase 9 |
|-----------|---------|-----------|-----------|---------|---------|
| Backend Lead | T005-T028 | US1 Backend | US4 Backend | US6 Backend | T110-T112, T139 |
| Backend Dev 2 | Support T005-T028 | US2 Backend | US5 Backend | - | T106-T109, T113-T119 |
| Frontend Dev | T039-T048 | US2+US3 Frontend | US4+US5 Frontend | US6 Frontend | T127-T133 |

---

## Success Criteria

Each user story is complete when:

- ✅ All backend handlers implemented and unit tested
- ✅ All API endpoints functional and tested via Swagger
- ✅ All frontend components render and accept user input
- ✅ All form validation works (client and server)
- ✅ API-frontend integration verified (data flows correctly)
- ✅ Error handling works (graceful failure messages)
- ✅ Independent test scenario passes (see each story section)

---

## Definition of Done

Feature is ready for QA when:

- ✅ All 141 tasks completed
- ✅ All user stories (US1-US6) functional and integrated
- ✅ Database migrations applied successfully
- ✅ `dotnet build` and `dotnet run` work without errors
- ✅ `npm run dev` starts frontend without errors
- ✅ All API endpoints documented in Swagger
- ✅ All components render in Storybook or browser
- ✅ End-to-end tests passing (Phase 9)
- ✅ Performance tests acceptable
- ✅ Code reviewed and merged to main branch

---

## Reference Materials

- **Specification**: [spec.md](./spec.md) - User stories, acceptance criteria, functional requirements
- **Database Design**: [database-schema.md](./database-schema.md) - Entity definitions, migrations, constraints, indexes
- **API Design**: [api-endpoints.md](./api-endpoints.md) - 21 endpoints, request/response schemas, validation
- **UI Design**: [ui-components.md](./ui-components.md) - 26 components, TypeScript props, integration patterns
- **Implementation Plan**: [plan.md](./plan.md) - Tech stack, assumptions, dependencies, rollout plan

---

## Timeline Estimate

- **Phase 2 (Foundation)**: 3-4 days (all in parallel)
- **Phase 3-5 (User Stories 1-3)**: 7-10 days (3 features in parallel, MVP scope)
- **Phase 6-7 (User Stories 4-5)**: 3-5 days (depends on phase 3-5 complete)
- **Phase 8 (User Story 6)**: 1-2 days (depends on phase 6-7)
- **Phase 9 (Polish)**: 2-3 days (all in parallel)

**Total Estimated Duration**: 16-24 days for full feature implementation with 3 developers

**MVP Duration** (US1-US3 only): 10-14 days

