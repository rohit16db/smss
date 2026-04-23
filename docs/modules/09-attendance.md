# Module: Attendance

## Overview
Track daily attendance for both students and staff. Supports statuses: Present, Absent, Late, HalfDay, Excused. Attendance data feeds into dashboards, reports, and payroll (salary deductions based on attendance).

---

## Domain Entities

### StudentAttendance (`SMS.Domain.Entities.StudentAttendance` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| EnrollmentId | Guid | FK to Enrollment |
| Date | DateOnly | |
| Status | AttendanceStatus (enum) | Present/Absent/Late/HalfDay/Excused |
| Remarks | string? | |
| MarkedById | Guid? | FK to User (who marked) |
| SectionId | Guid? | Denormalized for queries |

### StaffAttendance (`SMS.Domain.Entities.StaffAttendance` : BaseEntity)
| Property | Type | Notes |
|----------|------|-------|
| StaffId | Guid | FK to Staff |
| Date | DateOnly | |
| Status | AttendanceStatus (enum) | |
| Remarks | string? | |
| CheckInTime, CheckOutTime | TimeOnly? | |

### AttendanceStatus (Enum)
```csharp
Present = 0, Absent = 1, Late = 2, HalfDay = 3, Excused = 4
```

---

## API Endpoints

**Controller**: `AttendanceController` — Route: `api/attendance`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/attendance/students` | Student attendance list (filtered) |
| POST | `/api/attendance/students` | Save/update student attendance (bulk) |
| GET | `/api/attendance/students/section/{sectionId}` | Attendance for section on date |
| GET | `/api/attendance/students/report` | Student attendance report |
| GET | `/api/attendance/students/summary` | Attendance summary stats |
| GET | `/api/attendance/staff` | Staff attendance list |
| POST | `/api/attendance/staff` | Save/update staff attendance (bulk) |
| GET | `/api/attendance/staff/report` | Staff attendance report |
| GET | `/api/attendance/staff/summary` | Staff attendance summary |

---

## CQRS (in `Features/Attendance`)

### Commands
- `SaveStudentAttendanceCommand` — SectionId, Date, List of (EnrollmentId, Status, Remarks)
- `SaveStaffAttendanceCommand` — Date, List of (StaffId, Status, Remarks)

### Queries
- `GetStudentAttendanceQuery` — SectionId, Date
- `GetStudentAttendanceReportQuery` — SectionId, StartDate, EndDate
- `GetStudentAttendanceSummaryQuery`
- `GetStaffAttendanceQuery` — Date
- `GetStaffAttendanceReportQuery` — StartDate, EndDate
- `GetStaffAttendanceSummaryQuery`

### Validators
- `Features/Attendance/Validators/` — Date range, valid status values

---

## File Map

| Layer | File |
|-------|------|
| Entity | `backend/src/SMS.Domain/Entities/StudentAttendance.cs` |
| Entity | `backend/src/SMS.Domain/Entities/StaffAttendance.cs` |
| Enum | `backend/src/SMS.Domain/Enums/AttendanceStatus.cs` |
| Commands | `backend/src/SMS.Application/Features/Attendance/Commands/` |
| DTOs | `backend/src/SMS.Application/Features/Attendance/DTOs/` |
| Handlers | `backend/src/SMS.Application/Features/Attendance/Handlers/` |
| Queries | `backend/src/SMS.Application/Features/Attendance/Queries/` |
| Validators | `backend/src/SMS.Application/Features/Attendance/Validators/` |
| Controller | `backend/src/SMS.API/Controllers/AttendanceController.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/StudentAttendanceConfiguration.cs` |
| DB Config | `backend/src/SMS.Infrastructure/Data/Configurations/StaffAttendanceConfiguration.cs` |
| Frontend Page | `frontend/src/pages/AttendancePage.tsx` |
| Report Page | `frontend/src/pages/AttendanceReportPage.tsx` |
| Frontend API | `frontend/src/services/api.ts` (attendanceApi section) |

---

## Business Rules
- Attendance is saved as a bulk operation per section per date (upsert pattern)
- Holidays (from Holidays module) should be considered when calculating attendance %
- Staff attendance can include CheckIn/CheckOut times
- Attendance summary feeds dashboard charts (AttendanceTrendChart)
- Staff attendance correlates with salary deductions in Payroll module
