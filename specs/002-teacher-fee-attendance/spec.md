# Feature Specification: Teacher, Fee, and Attendance Management

**Feature Branch**: `002-teacher-fee-attendance`  
**Created**: January 12, 2026  
**Status**: Draft  
**Input**: User description: "Teacher, Fee and Attendance management as per constitution document"  
**Related PRD**: [prd_school_management_software.md](../../PRD/prd_school_management_software.md)

---

## Overview

This specification covers three interconnected features for Phase 2 of the School Management System:
1. **Teacher Management**: Complete lifecycle management of teachers/staff with qualifications, assignments, and status tracking
2. **Fee Management**: Flexible fee structure definition, student-wise fee assignment, payment tracking, and financial reporting
3. **Attendance Management**: Daily student and teacher attendance tracking with reporting capabilities

These features work together to enable comprehensive school administration: teachers teach classes, students pay fees, and attendance is tracked across both groups.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Teacher CRUD & Class Assignment (Priority: P1)

**Business Value**: Teachers are the backbone of a school. Administrators must efficiently manage teacher profiles, track qualifications, and assign them to classes. This is foundational to all other operations (students need teachers, attendance needs teachers).

**Scenario**: A school administrator recruits a new teacher named "Mrs. Priya Kumar" with M.Sc. in Mathematics, 5 years experience. She joins on Jan 15, 2026 and is assigned to teach Math to Classes 10A and 10B. The system stores all this information and validates that no teacher teaches conflicting class/time combinations.

**Why this priority**: Teacher assignment is required before students can be taught. Without this, attendance and performance tracking are meaningless. This is P1.

**Independent Test**: Can be fully tested by creating a teacher, assigning to classes, and verifying the assignment is saved and appears in class roster.

**Acceptance Scenarios**:

1. **Given** admin is logged in, **When** admin clicks "Add Teacher", **Then** form displays fields for name, qualification, experience, joining date, and status
2. **Given** admin fills valid teacher data, **When** admin submits form, **Then** system creates teacher record and shows success message
3. **Given** a teacher exists, **When** admin assigns teacher to class, **Then** system validates assignment and stores it (no duplicate assignments allowed)
4. **Given** a teacher is assigned to multiple classes, **When** admin views teacher detail, **Then** all assigned classes appear in a list
5. **Given** a teacher exists, **When** admin updates teacher status to "Inactive", **Then** system marks teacher inactive but preserves all historical data
6. **Given** a teacher is active, **When** admin views teacher list, **Then** teachers appear with their assigned classes, qualification, and status
7. **Given** an inactive teacher, **When** admin tries to assign to a new class, **Then** system prevents assignment and shows error message

### User Story 2 - Fee Structure Definition & Collection Tracking (Priority: P1)

**Business Value**: Fees are the primary revenue source. Administrators must define flexible fee structures (monthly/quarterly/yearly, multiple categories), assign to students, track payments, and identify outstanding dues. This directly impacts school revenue and cash flow visibility.

**Scenario**: School charges ₹5,000 tuition + ₹500 transport = ₹5,500/month for most students. Lower-income students pay ₹3,000/month. Monthly structure repeats 12 times/year. A student "Rahul" (Class 9) should pay ₹5,500/month starting Jan 2026. By Feb 1, he has paid ₹5,500 (January fee paid). February fee is due by Feb 10 but still pending as of Feb 5. System should show Rahul's payment status clearly.

**Why this priority**: Fee management is core to school operations. The PRD explicitly states reducing fee errors is a business goal. This enables cash flow visibility, dunning notifications, and financial reports. P1.

**Independent Test**: Can be tested by defining fee structure, assigning to student, recording payment, and verifying dashboard shows correct paid/pending status.

**Acceptance Scenarios**:

1. **Given** admin is logged in, **When** admin creates new fee structure, **Then** form accepts name (e.g., "Regular 2026"), category breakdown, frequency (monthly/quarterly/yearly), and amount
2. **Given** admin creates fee structure with tuition + transport categories, **When** admin saves, **Then** system totals amount correctly (₹5,000 + ₹500 = ₹5,500) and stores each category separately
3. **Given** a fee structure exists, **When** admin assigns to a student for academic year 2026, **Then** system generates payment obligations for each period (12 months if monthly structure)
4. **Given** a fee payment is due, **When** admin records payment via "Add Payment" with amount, date, and receipt number, **Then** system updates fee status to "Paid" and decrements remaining balance
5. **Given** a student has unpaid fees, **When** admin views fee dashboard, **Then** system shows outstanding amount and due date in clear red/amber indicators
6. **Given** a student with ₹10,000 overdue fees, **When** admin generates "Outstanding Fees Report", **Then** report shows student name, amount, due date, days overdue, and status
7. **Given** a student overpays fee by ₹500, **When** admin records payment, **Then** system shows overpayment/credit and allows adjustment in next period

### User Story 3 - Daily Attendance Tracking for Students (Priority: P1)

**Business Value**: Attendance is critical for student progress monitoring and regulatory compliance. Teachers need to quickly mark attendance, and administrators need insights into attendance patterns (who is frequently absent, overall school attendance rate).

**Scenario**: At 9:00 AM on Jan 15, 2026, Teacher "Mr. Sharma" opens Class 10A attendance form. 45 students are listed. He marks 40 present, 3 absent (due to illness noted), 2 on leave (with leave application reference). System saves attendance immediately. Later, admin runs "Class 10A Attendance Report" for January and sees Rahul was absent 8 days (due to health issues).

**Why this priority**: Attendance is foundational for student welfare, performance tracking, and compliance. Teachers use it daily. Without this, schools cannot track student engagement. P1.

**Independent Test**: Can be tested by marking attendance for a class, generating attendance report, and verifying accuracy.

**Acceptance Scenarios**:

1. **Given** a teacher is logged in, **When** teacher selects class and date, **Then** system displays all students in that class with checkboxes for Present/Absent/Leave
2. **Given** teacher marks attendance and adds notes ("Absent due to illness"), **When** teacher submits, **Then** system saves attendance record with timestamp and teacher name
3. **Given** attendance is marked, **When** teacher tries to change attendance for past date, **Then** system allows change with edit history logged
4. **Given** a student is absent, **When** admin views "Student Attendance Detail" for that student, **Then** system shows date, class, reason (if provided), and notes
5. **Given** a class has 45 students and 40 marked present on a date, **When** admin views class attendance summary, **Then** system shows "88.9% attendance" for that date
6. **Given** multiple dates of attendance exist, **When** admin generates "Monthly Attendance Report", **Then** report shows each student's present/absent/leave count and percentage for the month
7. **Given** a student has <75% attendance, **When** admin views the student detail, **Then** system highlights low attendance with warning indicator

### User Story 4 - Teacher Attendance Tracking (Priority: P2)

**Business Value**: Schools must track teacher attendance for payroll, compliance, and performance management. This enables accurate salary calculations and identifies attendance issues early.

**Scenario**: Mr. Sharma works 5 days/week. In January, he was present 20 days, absent 2 days (with approval for conference), and on leave 1 day. Admin generates payroll report for January and sees his attendance as "95.5%" (needed for salary deductions if applicable).

**Why this priority**: Teacher attendance is important for payroll accuracy and compliance, but less immediate than student attendance (which affects daily class operations). P2.

**Independent Test**: Can be tested by marking teacher attendance and verifying it appears in payroll calculations.

**Acceptance Scenarios**:

1. **Given** admin is in "Teacher Attendance" section, **When** admin selects teacher and date, **Then** system displays Present/Absent/Leave options with reason field
2. **Given** teacher absence is marked, **When** admin records, **Then** system stores attendance with optional reason (medical, approved leave, etc.)
3. **Given** a teacher has attendance records, **When** admin generates "Teacher Attendance Report", **Then** report shows attendance percentage and details for payroll period
4. **Given** teacher has low attendance, **When** admin runs monthly report, **Then** system flags unusually high absence count (e.g., >3 unplanned absences)

### User Story 5 - Financial Dashboard & Reports (Priority: P2)

**Business Value**: Admin needs quick visibility into school finances: how much fee collected, what's outstanding, who owes, what's the cash position. This enables financial decision-making and debt collection prioritization.

**Scenario**: On Feb 28, admin opens Dashboard and sees: "₹4,50,000 collected this month | ₹2,30,000 outstanding | Top 10 defaulters owed ₹87,000". He clicks "Generate Outstanding Fees Report" and gets a spreadsheet showing each defaulting student, amount, and due date.

**Why this priority**: Critical for financial health visibility but comes after core fee tracking works (P1). P2.

**Independent Test**: Can be tested by creating fee structures, recording payments, and verifying dashboard shows correct totals.

**Acceptance Scenarios**:

1. **Given** multiple fee payments exist, **When** admin views Dashboard, **Then** system displays "Total Collected This Month" and "Total Outstanding" as summary cards
2. **Given** fees exist with various due dates, **When** admin generates "Outstanding Fees Report", **Then** report shows per-student breakdown sorted by amount outstanding (highest first)
3. **Given** multiple students have overdue fees, **When** admin filters "Overdue >30 Days", **Then** system shows only fees overdue by more than 30 days with count and total amount
4. **Given** a student has been added to fee system, **When** admin generates "Fee Collection Trend", **Then** report shows monthly collected vs. expected for past 12 months

### User Story 6 - Attendance-to-Salary Integration (Priority: P3)

**Business Value**: Schools may tie salary deductions to attendance. If teacher has <90% attendance, they may lose bonus or have deduction. This feature enables that calculation in payroll module (not fully in scope here, but attendance feeds into it).

**Scenario**: Policy: Teachers with <90% attendance lose 10% bonus. Mr. Sharma has 92% attendance (20 present / 22 working days). System calculates his eligibility for bonus in payroll module. Mrs. Priya has 85% attendance and system flags her for no bonus.

**Why this priority**: Enhancement to attendance tracking, not a core requirement. Depends on P1 attendance working first. P3.

**Independent Test**: Can be tested by running attendance report and verifying "Bonus Eligibility" column shows correct calculation.

**Acceptance Scenarios**:

1. **Given** attendance records exist, **When** admin generates "Attendance-Based Salary Adjustments" report, **Then** system calculates attendance percentage and applies policy (e.g., 10% deduction if <90%)
2. **Given** a teacher has 88% attendance, **When** admin views report, **Then** system highlights "Bonus Not Eligible" in red

### Edge Cases

- What happens when a student is assigned a fee structure mid-month? (System should prorate or allow custom amount entry)
- How does system handle fee corrections/reversals? (System should log correction as new transaction, not delete original, for audit trail)
- What if a class has no teacher assigned? (System should allow marking attendance as "Unmanaged" or prevent attendance entry with warning)
- Can a teacher be in two classes at same time? (System should prevent overlapping class assignments based on timing rules)
- What if attendance is marked after the grace period (e.g., 7 days late)? (System should allow with "Late Entry" flag for audit)
- How to handle students who withdraw mid-month? (System should mark student as "Transferred Out" and prorate remaining fees)

---

## Requirements *(mandatory)*

### Functional Requirements

#### Teacher Management

- **FR-001**: System MUST allow admin to create teacher record with fields: first name, last name, email, phone, qualification(s), years of experience, joining date, status (Active/Inactive)
- **FR-002**: System MUST allow admin to assign teacher to classes/subjects with no overlapping time slot assignments for same teacher
- **FR-003**: System MUST maintain history of all teacher changes (qualification updates, status changes, assignments) for audit trail
- **FR-004**: System MUST allow soft-delete of teachers (mark as Inactive) rather than hard delete, preserving all historical records
- **FR-005**: System MUST display assigned classes/subjects when viewing teacher detail page
- **FR-006**: System MUST validate that teacher qualifications are relevant to assigned subjects (advisory/warning only, not blocking)

#### Fee Management

- **FR-007**: System MUST allow admin to define fee structures with: name, academic year, frequency (monthly/quarterly/yearly), fee categories (tuition, transport, misc.), and amount per category
- **FR-008**: System MUST allow admin to assign fee structure to individual students with start date and end date
- **FR-009**: System MUST auto-generate payment obligations (expected due dates) based on frequency and duration
- **FR-010**: System MUST allow admin to record fee payments with: student, amount paid, payment date, receipt number, payment method (cash/check/bank transfer), and optional notes
- **FR-011**: System MUST track fee payment status as: Paid (full), Partial (part-paid), Pending (not yet paid), Overdue (past due date and unpaid)
- **FR-012**: System MUST calculate and display: amount due, amount paid, balance remaining, days overdue for each student
- **FR-013**: System MUST allow partial payment without forcing full payment (student can pay ₹1,000 of ₹5,500 due)
- **FR-014**: System MUST handle overpayment: if student pays ₹6,000 for ₹5,500 due, system shows ₹500 credit for next period
- **FR-015**: System MUST generate "Outstanding Fees Report" showing: student name, class, amount outstanding, due date, days overdue, last payment date, sorted by amount (highest first)
- **FR-016**: System MUST support fee corrections/reversals: admin can reverse a payment with reason, creating new audit log entry
- **FR-017**: System MUST allow pro-rata fee calculation for students joining/leaving mid-period
- **FR-018**: System MUST show fee collection summary on dashboard: total collected this month, total outstanding, count of students with pending fees

#### Attendance Management (Students)

- **FR-019**: System MUST allow teacher to mark attendance for a class on a specific date with options: Present, Absent, Leave, Unexcused Absence
- **FR-020**: System MUST require attendance marking before end of school day (or allow grace period, configurable)
- **FR-021**: System MUST allow optional reason/notes for absences (e.g., "Medical reason", "Leave application", "Sick")
- **FR-022**: System MUST store attendance with timestamp and teacher who recorded it
- **FR-023**: System MUST prevent duplicate attendance entries for same student on same date/class
- **FR-024**: System MUST allow teacher to edit attendance for past dates with full history logged (original entry, edit reason, date/time of edit)
- **FR-025**: System MUST calculate monthly attendance percentage: (Present days / Working days) × 100
- **FR-026**: System MUST generate "Student Attendance Report" showing: student name, month, present/absent/leave count, percentage, with notes
- **FR-027**: System MUST generate "Class Attendance Summary" showing attendance % for each class on each date
- **FR-028**: System MUST flag students with <75% attendance in student detail page (visual warning)
- **FR-029**: System MUST track "Unexcused Absence" separately from excused absences (leave with approval)

#### Attendance Management (Teachers)

- **FR-030**: System MUST allow admin to mark teacher attendance (Present/Absent/Leave) for each working day
- **FR-031**: System MUST allow optional reason for teacher absences
- **FR-032**: System MUST calculate teacher attendance percentage for payroll period (month/quarter)
- **FR-033**: System MUST generate "Teacher Attendance Report" for payroll module with: name, period, working days, present/absent/leave count, percentage

#### Integration Requirements

- **FR-034**: System MUST prevent attendance marking for classes with no assigned teacher (warning message)
- **FR-035**: System MUST link fee payment records to student (when student detail page loads, show related fees and payments)
- **FR-036**: System MUST link attendance records to teacher assignment (attendance visible in teacher detail)
- **FR-037**: System MUST export all reports to CSV/Excel format for further analysis
- **FR-038**: System MUST log all financial transactions (fee creation, payment, reversal) in audit log with user, timestamp, and change details

### Key Entities

- **Teacher**: id (UUID), first_name, last_name, email, phone, qualification, experience_years, joining_date, status (enum), is_active, created_at, updated_at
- **TeacherAssignment**: id (UUID), teacher_id (FK), class_id (FK), subject_id (FK), assignment_date, removal_date (nullable), audit fields
- **FeeStructure**: id (UUID), name, academic_year (int), frequency (enum: monthly/quarterly/yearly), total_amount, created_at, updated_at
- **FeeStructureCategory**: id (UUID), fee_structure_id (FK), category (enum: tuition/transport/misc), amount
- **StudentFee**: id (UUID), student_id (FK), fee_structure_id (FK), start_date, end_date, total_amount, created_at, updated_at
- **FeePayment**: id (UUID), student_fee_id (FK), amount_paid, payment_date, receipt_number, payment_method (enum), notes, created_at, updated_at
- **StudentAttendance**: id (UUID), student_id (FK), class_id (FK), date, status (enum: present/absent/leave/unexcused), reason (nullable), marked_by (FK to user), marked_at, last_edited_by, last_edited_at
- **TeacherAttendance**: id (UUID), teacher_id (FK), date, status (enum: present/absent/leave), reason (nullable), recorded_by (FK to user), recorded_at
- **AttendanceReport**: Derived view joining StudentAttendance with Student and Class, used for reporting

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Admin can complete teacher CRUD (create, view, list, update, assign to classes) operations in under 5 minutes per teacher, with <2 validation errors per 100 entries
- **SC-002**: Fee dashboard loads in <2 seconds showing summary (total collected, outstanding, overdue count)
- **SC-003**: Admin can run "Outstanding Fees Report" including 200+ students in <3 seconds and export to Excel in <5 seconds
- **SC-004**: Teacher can mark attendance for class of 50 students in <3 minutes using checklist interface
- **SC-005**: Attendance accuracy: >98% of marked attendance records are correct when spot-checked (no data entry errors)
- **SC-006**: Fee payment tracking achieves 100% accuracy: for any student, "sum of payments" + "remaining balance" = "total amount due" within rounding
- **SC-007**: Financial reporting: Outstanding fees report shows discrepancy <0.1% when compared to manual ledger audit
- **SC-008**: Teacher satisfaction: >90% of teachers can mark attendance on first attempt without help (usability metric)
- **SC-009**: Admin workload reduction: School can manage fees for 200 students with <4 hours/month manual work (vs. estimated 20+ hours with Excel)
- **SC-010**: System reduces fee collection delays: <10% of fees remain >30 days overdue (vs. 40% previously)

---

## Assumptions

1. Academic year = Calendar year (Jan-Dec 2026, etc.)
2. School operates 240 working days/year (5 days/week, minus holidays)
3. Fee payment methods (cash, check, bank transfer) are tracked by receipt number, not integrated to external payment gateways in Phase 2
4. Attendance is marked daily; no sub-hourly tracking
5. Teacher qualifications are text fields; no pre-defined qualification list (can be expanded in future)
6. All dates/times are in school's local timezone
7. No role-based access control for this phase (all features available to Admin only; RBAC added in Phase 3)
8. Fee structures are annual; multiple structures can be active simultaneously (for different student groups)

---

## Dependencies

- Student Management (Phase 1): Students must exist to assign fees
- User Authentication (Phase 1): Teachers must be users to mark attendance
- Class Management (Phase 1): Classes must exist to assign teachers
- Database Tables: All entities require EF Core migrations

---

## Open Questions / To Be Clarified

1. **Fee Proration**: If a student joins on Jan 15 and fee is ₹5,500/month, do we charge ₹2,917 (prorated) or full ₹5,500 for January? Or allow admin to enter custom amount?
2. **Attendance Grace Period**: How many days after class date can attendance be entered? Suggest default 7 days with admin override.
3. **Fee Penalty for Late Payment**: Should system support late fees/penalties (e.g., 5% per month overdue)? Suggest Phase 2 enhancement.
4. **Attendance Batch Operations**: Should teachers be able to mark "all present" and then uncheck absences, or enter each individually? Suggest "Quick Mark All Present" button.
5. **Holiday Configuration**: Should system have a configurable holiday calendar to exclude from attendance calculations? Currently assuming manual "working days" count.
6. **Teacher Subject Expertise**: Should system validate teacher qualification matches subject (e.g., M.Sc. Math cannot teach English)? Suggest advisory warning only in Phase 2.

