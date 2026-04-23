# Module Implementation Context Documents

These documents provide AI-ready implementation context for each module in the School Management System.
Each file contains the complete file map, entity definitions, API endpoints, CQRS commands/queries, frontend pages/services, and business rules so that any AI assistant can immediately understand the module and start making changes.

## Module Index

| Module | Document | Description |
|--------|----------|-------------|
| Auth & Users | [01-auth.md](./01-auth.md) | Authentication, JWT, password management, user roles |
| Settings & Academic Years | [02-settings.md](./02-settings.md) | School configuration, branding, academic year management |
| Departments | [03-departments.md](./03-departments.md) | Department CRUD |
| Classes & Sections | [04-classes.md](./04-classes.md) | Classes, sections, roll numbers, student-section mapping |
| Subjects | [05-subjects.md](./05-subjects.md) | Subject CRUD |
| Students | [06-students.md](./06-students.md) | Student profiles, enrollment, promotion |
| Staff Management | [07-staff.md](./07-staff.md) | Staff profiles, assignments, qualifications |
| Fee Management | [08-fees.md](./08-fees.md) | Fee structures, student fees, payments, receipts |
| Attendance | [09-attendance.md](./09-attendance.md) | Student & staff attendance tracking |
| Exams & Marks | [10-exams.md](./10-exams.md) | Exam management, marks entry, report cards, analytics |
| Timetable | [11-timetable.md](./11-timetable.md) | Time slots, timetable entries, PDF export |
| Salary & Payroll | [12-salary.md](./12-salary.md) | Salary structures, payments, bulk processing, payroll |
| Transport | [13-transport.md](./13-transport.md) | Routes, vehicles, student assignments, fee sync |
| Inventory | [14-inventory.md](./14-inventory.md) | Categories, items, stock transactions |
| Holidays | [15-holidays.md](./15-holidays.md) | Holiday calendar management |
| Notifications | [16-notifications.md](./16-notifications.md) | Notification templates and sending |
| Reports | [17-reports.md](./17-reports.md) | Fee reports, salary reports, dashboard |

## Architecture Pattern (All Modules Follow This)

```
Controller (API) → MediatR Send → Command/Query → Handler → DbContext → PostgreSQL
Frontend Page → Service/Hook → Axios → Backend API
```

## Key Conventions
- **Backend CQRS**: Commands for writes, Queries for reads, via MediatR
- **Validation**: FluentValidation in pipeline behaviors
- **Database**: EF Core with snake_case naming, PostgreSQL
- **Frontend**: React + TypeScript, Material UI, TanStack React Query for data fetching
- **API versioning**: Some controllers use `api/v1/`, others use `api/`
