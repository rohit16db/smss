# Database Schema & Migrations Design
**Feature**: 002-teacher-fee-attendance  
**Created**: January 12, 2026  
**Phase**: Planning / Design  
**Based on**: [spec.md](../spec.md)

---

## Overview

This document defines the database schema for Teacher, Fee, and Attendance management features. The design follows:
- Clean Architecture patterns (DDD principles)
- PostgreSQL best practices (constraints, indexes, partitioning readiness)
- Audit trail requirements (created_by, updated_by, timestamps)
- Soft-delete for historical preservation
- Snake_case naming convention (existing SMS project standard)

---

## Entity Relationship Diagram (ERD)

```
┌─────────────┐
│   Teacher   │
├─────────────┤
│ id (UUID)   │
│ username*   │ ──┐
│ email*      │   │ (inherits from User via username)
│ first_name  │   │
│ last_name   │   │
│ phone       │   │
│ ...fields   │   │
└─────────────┘   │
      ▲           │
      │           │
      └───────────┘ (1:1 relationship via email/username)

┌──────────────────┐          ┌─────────────┐          ┌──────────────┐
│ TeacherAssignment│          │   Class     │          │   Subject    │
├──────────────────┤          ├─────────────┤          ├──────────────┤
│ id (UUID)        │ ◄────────│ id (UUID)   │          │ id (UUID)    │
│ teacher_id (FK)* │          │ name        │          │ name         │
│ class_id (FK)*   │─────────►├─────────────┤          ├──────────────┤
│ subject_id (FK)  │──────────►            ...          │ ...          │
│ assignment_date  │
│ removal_date     │
│ audit_fields     │
└──────────────────┘


┌──────────────────┐          ┌────────────────┐        ┌──────────────────┐
│  FeeStructure    │◄─────────│ FeeStructureCategory   │
├──────────────────┤          ├────────────────┤        │ id (UUID)        │
│ id (UUID)        │          │ id (UUID)      │        │ fee_structure_id │
│ name             │          │ feestructure_id(FK)*   │ category (enum)  │
│ academic_year    │          │ category (enum)│        │ amount           │
│ frequency (enum) │          │ amount         │        └──────────────────┘
│ total_amount     │          └────────────────┘
│ is_active        │
└──────────────────┘


┌──────────────────┐          ┌─────────────┐          ┌────────────────┐
│  StudentFee      │          │   Student   │          │  FeePayment    │
├──────────────────┤          ├─────────────┤          ├────────────────┤
│ id (UUID)        │◄─────────│ id (UUID)   │          │ id (UUID)      │
│ student_id (FK)* │          │ ...fields   │          │ student_fee_id*│
│ feestructure_id* │          └─────────────┘          │ amount_paid    │
│ start_date       │          ▲                         │ payment_date   │
│ end_date         │          │                         │ receipt_number │
│ total_amount     │          └─────────────────────────┤ payment_method │
└──────────────────┘                                    │ notes          │
                                                        └────────────────┘


┌──────────────────┐          ┌─────────────┐
│ StudentAttendance│          │   Student   │
├──────────────────┤          ├─────────────┤
│ id (UUID)        │◄─────────│ id (UUID)   │
│ student_id (FK)* │          │ ...fields   │
│ class_id (FK)*   │          └─────────────┘
│ date             │          ▲
│ status (enum)*   │          │
│ reason           │          │
│ marked_by (FK)   │          │
│ marked_at        │
│ last_edited_by   │
│ last_edited_at   │
└──────────────────┘


┌──────────────────┐          ┌─────────────┐
│ TeacherAttendance│          │   Teacher   │
├──────────────────┤          ├─────────────┤
│ id (UUID)        │◄─────────│ id (UUID)   │
│ teacher_id (FK)* │          │ ...fields   │
│ date             │          └─────────────┘
│ status (enum)*   │
│ reason           │
│ recorded_by (FK) │
│ recorded_at      │
└──────────────────┘

Legend:
* = NOT NULL constraint
◄────────► = Foreign Key relationship
(FK) = Foreign Key
(enum) = Enumeration type
```

---

## Table Definitions

### 1. Teacher

Extends/complements the User table for teacher-specific attributes.

```sql
CREATE TABLE teachers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL UNIQUE REFERENCES users(id) ON DELETE RESTRICT,
    phone CHARACTER VARYING(20),
    qualification CHARACTER VARYING(500),  -- e.g., "M.Sc. Mathematics, B.Ed."
    experience_years INTEGER DEFAULT 0,
    joining_date DATE NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by TEXT,
    updated_by TEXT
);

CREATE INDEX idx_teachers_user_id ON teachers(user_id);
CREATE INDEX idx_teachers_is_active ON teachers(is_active);
CREATE INDEX idx_teachers_joining_date ON teachers(joining_date);
```

**Audit Trail**: All create/update operations log created_by, updated_by

**Soft Delete**: is_active flag allows marking teacher as inactive without losing history

**Relationship**: Links to users table via user_id (1:1 relationship)

---

### 2. TeacherAssignment

Maps teachers to classes/subjects they teach.

```sql
CREATE TABLE teacher_assignments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    teacher_id UUID NOT NULL REFERENCES teachers(id) ON DELETE RESTRICT,
    class_id UUID NOT NULL REFERENCES classes(id) ON DELETE RESTRICT,
    subject_id UUID NOT NULL REFERENCES subjects(id) ON DELETE RESTRICT,
    assignment_date DATE NOT NULL DEFAULT CURRENT_DATE,
    removal_date DATE,  -- NULL if currently assigned
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by TEXT,
    updated_by TEXT,
    
    CONSTRAINT unique_active_assignment UNIQUE (teacher_id, class_id, subject_id) 
        WHERE removal_date IS NULL
);

CREATE INDEX idx_teacher_assignments_teacher_id ON teacher_assignments(teacher_id);
CREATE INDEX idx_teacher_assignments_class_id ON teacher_assignments(class_id);
CREATE INDEX idx_teacher_assignments_subject_id ON teacher_assignments(subject_id);
CREATE INDEX idx_teacher_assignments_removal_date ON teacher_assignments(removal_date);
```

**Design Notes**:
- UNIQUE constraint prevents duplicate active assignments for same (teacher, class, subject)
- removal_date = NULL indicates current assignment; non-null means removed
- Allows full history via removal_date

---

### 3. FeeStructure

Defines fee templates (tuition, transport, etc.) with frequency and amount.

```sql
CREATE TABLE fee_structures (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name CHARACTER VARYING(100) NOT NULL,  -- e.g., "Regular Annual 2026"
    academic_year INTEGER NOT NULL,  -- e.g., 2026
    frequency CHARACTER VARYING(20) NOT NULL DEFAULT 'monthly',  -- 'monthly', 'quarterly', 'yearly'
    total_amount NUMERIC(10, 2) NOT NULL,  -- ₹5,500.00
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by TEXT,
    updated_by TEXT
);

CREATE INDEX idx_fee_structures_academic_year ON fee_structures(academic_year);
CREATE INDEX idx_fee_structures_is_active ON fee_structures(is_active);
```

**Design Notes**:
- name should be human-readable and include year (e.g., "Regular Monthly 2026")
- frequency uses string (not enum) for flexibility; application layer validates
- Multiple structures can be active simultaneously for different student groups

---

### 4. FeeStructureCategory

Breakdown of FeeStructure by category (tuition, transport, misc).

```sql
CREATE TABLE fee_structure_categories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    fee_structure_id UUID NOT NULL REFERENCES fee_structures(id) ON DELETE CASCADE,
    category CHARACTER VARYING(50) NOT NULL,  -- 'tuition', 'transport', 'miscellaneous', 'lab_fee'
    amount NUMERIC(10, 2) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by TEXT,
    updated_by TEXT,
    
    CONSTRAINT unique_category_per_structure UNIQUE (fee_structure_id, category)
);

CREATE INDEX idx_fsc_fee_structure_id ON fee_structure_categories(fee_structure_id);
```

**Design Notes**:
- ON DELETE CASCADE: If fee_structure is deleted, categories are deleted
- UNIQUE constraint ensures one tuition, one transport per structure
- Sum of all categories = FeeStructure.total_amount (validated at application layer)

---

### 5. StudentFee

Assigns a fee structure to a student for a date range.

```sql
CREATE TABLE student_fees (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    student_id UUID NOT NULL REFERENCES students(id) ON DELETE RESTRICT,
    fee_structure_id UUID NOT NULL REFERENCES fee_structures(id) ON DELETE RESTRICT,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    total_amount NUMERIC(10, 2) NOT NULL,  -- Cached from FeeStructure for performance
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by TEXT,
    updated_by TEXT,
    
    CONSTRAINT valid_date_range CHECK (start_date <= end_date)
);

CREATE INDEX idx_student_fees_student_id ON student_fees(student_id);
CREATE INDEX idx_student_fees_fee_structure_id ON student_fees(fee_structure_id);
CREATE INDEX idx_student_fees_date_range ON student_fees(start_date, end_date);
CREATE INDEX idx_student_fees_is_active ON student_fees(is_active);
```

**Design Notes**:
- total_amount cached from FeeStructure for query performance
- Allows multiple fee structures per student (e.g., lowered fee after scholarship)
- start_date/end_date define period of assignment (e.g., Jan 1 - Dec 31, 2026)

---

### 6. FeePayment

Records individual fee payments by student.

```sql
CREATE TABLE fee_payments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    student_fee_id UUID NOT NULL REFERENCES student_fees(id) ON DELETE RESTRICT,
    amount_paid NUMERIC(10, 2) NOT NULL,
    payment_date DATE NOT NULL,
    receipt_number CHARACTER VARYING(50) NOT NULL UNIQUE,  -- Admin-entered receipt from school
    payment_method CHARACTER VARYING(20) NOT NULL,  -- 'cash', 'check', 'bank_transfer'
    notes TEXT,  -- e.g., "Partial payment for Jan-Feb"
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_by TEXT,
    updated_by TEXT,
    
    CONSTRAINT positive_amount CHECK (amount_paid > 0)
);

CREATE INDEX idx_fee_payments_student_fee_id ON fee_payments(student_fee_id);
CREATE INDEX idx_fee_payments_payment_date ON fee_payments(payment_date);
CREATE INDEX idx_fee_payments_receipt_number ON fee_payments(receipt_number);
```

**Design Notes**:
- receipt_number is UNIQUE to prevent duplicate receipt entry
- payment_date may differ from created_at (admin records past payment)
- notes allow flexibility for partial payments, adjustments, etc.

---

### 7. StudentAttendance

Daily attendance record for each student per class.

```sql
CREATE TABLE student_attendances (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    student_id UUID NOT NULL REFERENCES students(id) ON DELETE RESTRICT,
    class_id UUID NOT NULL REFERENCES classes(id) ON DELETE RESTRICT,
    attendance_date DATE NOT NULL,
    status CHARACTER VARYING(20) NOT NULL,  -- 'present', 'absent', 'leave', 'unexcused'
    reason TEXT,  -- e.g., "Medical leave with approval", "Sick", "School trip"
    marked_by UUID NOT NULL REFERENCES users(id) ON DELETE SET NULL,  -- Teacher who marked
    marked_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_edited_by UUID REFERENCES users(id) ON DELETE SET NULL,
    last_edited_at TIMESTAMP WITH TIME ZONE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT unique_attendance_record UNIQUE (student_id, class_id, attendance_date),
    CONSTRAINT valid_status CHECK (status IN ('present', 'absent', 'leave', 'unexcused'))
);

CREATE INDEX idx_student_attendances_student_id ON student_attendances(student_id);
CREATE INDEX idx_student_attendances_class_id ON student_attendances(class_id);
CREATE INDEX idx_student_attendances_date ON student_attendances(attendance_date);
CREATE INDEX idx_student_attendances_status ON student_attendances(status);
CREATE INDEX idx_student_attendances_date_range ON student_attendances(student_id, attendance_date DESC);
```

**Design Notes**:
- UNIQUE constraint prevents duplicate attendance per (student, class, date)
- marked_by stores teacher user_id (validates teacher logged in)
- last_edited_by / last_edited_at track corrections with full audit trail
- Composite index on (student_id, date) for efficient monthly reports

---

### 8. TeacherAttendance

Daily attendance record for each teacher.

```sql
CREATE TABLE teacher_attendances (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    teacher_id UUID NOT NULL REFERENCES teachers(id) ON DELETE RESTRICT,
    attendance_date DATE NOT NULL,
    status CHARACTER VARYING(20) NOT NULL,  -- 'present', 'absent', 'leave'
    reason TEXT,  -- e.g., "Approved conference", "Medical", "Personal leave"
    recorded_by UUID NOT NULL REFERENCES users(id) ON DELETE SET NULL,  -- Admin who recorded
    recorded_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    CONSTRAINT unique_teacher_attendance UNIQUE (teacher_id, attendance_date),
    CONSTRAINT valid_status CHECK (status IN ('present', 'absent', 'leave'))
);

CREATE INDEX idx_teacher_attendances_teacher_id ON teacher_attendances(teacher_id);
CREATE INDEX idx_teacher_attendances_date ON teacher_attendances(attendance_date);
CREATE INDEX idx_teacher_attendances_date_range ON teacher_attendances(teacher_id, attendance_date DESC);
```

**Design Notes**:
- Similar to student attendance but simpler (no class context)
- recorded_by is admin, not teacher (admin enters all teacher attendance)

---

## Migration Strategy

### Migration 1: Create Teacher Management Tables

**File**: `{timestamp}_AddTeacherManagement.cs`

```csharp
public partial class AddTeacherManagement : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Create teachers table
        migrationBuilder.CreateTable(
            name: "teachers",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                qualification = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                experience_years = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                joining_date = table.Column<DateTime>(type: "date", nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                created_by = table.Column<string>(type: "text", nullable: true),
                updated_by = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_teachers", x => x.id);
                table.ForeignKey("FK_teachers_users_user_id", x => x.user_id, "users", "id", 
                    onDelete: ReferentialAction.Restrict);
                table.UniqueConstraint("UX_teachers_user_id", x => x.user_id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_teachers_user_id", table: "teachers", column: "user_id");
        migrationBuilder.CreateIndex(
            name: "IX_teachers_is_active", table: "teachers", column: "is_active");
        migrationBuilder.CreateIndex(
            name: "IX_teachers_joining_date", table: "teachers", column: "joining_date");

        // Create teacher_assignments table (assumes classes and subjects exist)
        migrationBuilder.CreateTable(
            name: "teacher_assignments",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                class_id = table.Column<Guid>(type: "uuid", nullable: false),
                subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                assignment_date = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "CURRENT_DATE"),
                removal_date = table.Column<DateTime>(type: "date", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                created_by = table.Column<string>(type: "text", nullable: true),
                updated_by = table.Column<string>(type: "text", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_teacher_assignments", x => x.id);
                table.ForeignKey("FK_teacher_assignments_teachers_teacher_id", x => x.teacher_id, "teachers", "id", 
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_teacher_assignments_classes_class_id", x => x.class_id, "classes", "id", 
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_teacher_assignments_subjects_subject_id", x => x.subject_id, "subjects", "id", 
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_teacher_assignments_teacher_id", table: "teacher_assignments", column: "teacher_id");
        migrationBuilder.CreateIndex(
            name: "IX_teacher_assignments_class_id", table: "teacher_assignments", column: "class_id");
        migrationBuilder.CreateIndex(
            name: "IX_teacher_assignments_subject_id", table: "teacher_assignments", column: "subject_id");
        migrationBuilder.CreateIndex(
            name: "IX_teacher_assignments_removal_date", table: "teacher_assignments", column: "removal_date");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "teacher_assignments");
        migrationBuilder.DropTable(name: "teachers");
    }
}
```

### Migration 2: Create Fee Management Tables

**File**: `{timestamp}_AddFeeManagement.cs`

Creates fee_structures, fee_structure_categories, student_fees, fee_payments.

### Migration 3: Create Attendance Tables

**File**: `{timestamp}_AddAttendanceManagement.cs`

Creates student_attendances, teacher_attendances with proper indexes and constraints.

---

## Domain Entities (EF Core Models)

### Teacher Entity

```csharp
public class Teacher : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string? Phone { get; set; }
    public string? Qualification { get; set; }
    public int ExperienceYears { get; set; }
    public DateTime JoiningDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<TeacherAssignment> Assignments { get; set; } = new List<TeacherAssignment>();
    public ICollection<StudentAttendance> MarkedAttendances { get; set; } = new List<StudentAttendance>();
    public ICollection<TeacherAttendance> Attendances { get; set; } = new List<TeacherAttendance>();
}
```

### Other Entities

Similar structure for FeeStructure, StudentFee, FeePayment, StudentAttendance, TeacherAttendance, TeacherAssignment.

---

## Indexes Summary

| Table | Index | Columns | Purpose |
|-------|-------|---------|---------|
| teachers | IX_teachers_is_active | is_active | Filter active teachers in lists |
| teachers | IX_teachers_joining_date | joining_date | Sort by joining date |
| teacher_assignments | IX_teacher_assignments_teacher_id | teacher_id | Find classes for teacher |
| teacher_assignments | IX_teacher_assignments_removal_date | removal_date | Find active assignments (removal_date IS NULL) |
| fee_structures | IX_fee_structures_academic_year | academic_year | Filter by year |
| student_fees | IX_student_fees_date_range | start_date, end_date | Find applicable fees for date |
| fee_payments | IX_fee_payments_payment_date | payment_date | Filter payments by month/year |
| student_attendances | IX_student_attendances_date_range | student_id, attendance_date DESC | Monthly reports (most recent first) |
| teacher_attendances | IX_teacher_attendances_date_range | teacher_id, attendance_date DESC | Payroll period attendance |

---

## Data Integrity Constraints

| Constraint | Table | Purpose |
|-----------|-------|---------|
| UNIQUE(teacher_id, class_id, subject_id) WHERE removal_date IS NULL | teacher_assignments | Prevent duplicate active assignments |
| UNIQUE(student_id, class_id, attendance_date) | student_attendances | One attendance record per student per class per day |
| UNIQUE(teacher_id, attendance_date) | teacher_attendances | One attendance record per teacher per day |
| UNIQUE(fee_structure_id, category) | fee_structure_categories | One category type per structure |
| CHECK(start_date <= end_date) | student_fees | Valid date range |
| CHECK(amount_paid > 0) | fee_payments | Non-negative payments |
| CHECK(status IN ('present', 'absent', 'leave', 'unexcused')) | student_attendances | Valid status values |

---

## Performance Considerations

### Partitioning Strategy (Future)

As data grows:
- **student_attendances**: Partition by month on attendance_date (e.g., 2026_01, 2026_02)
- **fee_payments**: Partition by quarter on payment_date (e.g., Q1_2026, Q2_2026)
- **teacher_attendances**: Partition by month on attendance_date

### Caching Strategy

- **FeeStructure total_amount**: Cache in StudentFee.total_amount to avoid JOIN on reads
- **Teacher active assignments**: Cache assignment list in application after retrieving
- **Monthly attendance percentages**: Calculate on-demand; cache in session for same user

### Query Optimization

```sql
-- Get all students with fees due
SELECT 
    s.id, s.first_name, s.last_name,
    COALESCE(SUM(sf.total_amount) - COALESCE(SUM(fp.amount_paid), 0), 0) as balance_due
FROM students s
LEFT JOIN student_fees sf ON s.id = sf.student_id 
    AND sf.start_date <= CURRENT_DATE AND sf.end_date >= CURRENT_DATE
LEFT JOIN fee_payments fp ON sf.id = fp.student_fee_id
WHERE sf.is_active = true
GROUP BY s.id, s.first_name, s.last_name
HAVING COALESCE(SUM(sf.total_amount) - COALESCE(SUM(fp.amount_paid), 0), 0) > 0
ORDER BY balance_due DESC;
```

---

## Audit Trail Implementation

All tables include:
- `created_at`: Automatically set to CURRENT_TIMESTAMP
- `updated_at`: Automatically set to CURRENT_TIMESTAMP, updated on modification
- `created_by`: USERNAME of user who created record
- `updated_by`: USERNAME of user who last updated record

Application layer logs:
- All fee corrections/reversals as new FeePayment records with negative amounts
- All attendance edits with edit_reason
- All teacher assignment changes

---

## Rollback Plan

Each migration is reversible:
1. Migration 1 (Teachers): Down() drops teacher_assignments, teachers
2. Migration 2 (Fees): Down() drops fee_payments, student_fees, fee_structure_categories, fee_structures
3. Migration 3 (Attendance): Down() drops teacher_attendances, student_attendances

To rollback: `dotnet ef database update {previous_migration_name}`

