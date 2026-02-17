-- Phase 3: Teacher Management Tables
CREATE TABLE IF NOT EXISTS teachers (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL UNIQUE,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    phone VARCHAR(20),
    qualification VARCHAR(500),
    experience_years INTEGER NOT NULL DEFAULT 0,
    joining_date DATE NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100),
    updated_by VARCHAR(100)
);

CREATE INDEX IF NOT EXISTS ix_teachers_email ON teachers(email);
CREATE INDEX IF NOT EXISTS ix_teachers_is_active ON teachers(is_active);
CREATE INDEX IF NOT EXISTS ix_teachers_joining_date ON teachers(joining_date);
CREATE INDEX IF NOT EXISTS ix_teachers_user_id ON teachers(user_id);

CREATE TABLE IF NOT EXISTS teacher_assignments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    teacher_id UUID NOT NULL REFERENCES teachers(id) ON DELETE RESTRICT,
    class_id UUID NOT NULL,
    subject_id UUID NOT NULL,
    assignment_date DATE NOT NULL,
    removal_date DATE,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100),
    updated_by VARCHAR(100)
);

CREATE INDEX IF NOT EXISTS ix_teacher_assignments_class_id ON teacher_assignments(class_id);
CREATE INDEX IF NOT EXISTS ix_teacher_assignments_removal_date ON teacher_assignments(removal_date);
CREATE INDEX IF NOT EXISTS ix_teacher_assignments_subject_id ON teacher_assignments(subject_id);
CREATE INDEX IF NOT EXISTS ix_teacher_assignments_teacher_id ON teacher_assignments(teacher_id);
CREATE UNIQUE INDEX IF NOT EXISTS ix_teacher_assignments_teacher_id_class_id_subject_id_removal_date 
    ON teacher_assignments(teacher_id, class_id, subject_id, removal_date) WHERE removal_date IS NULL;

-- Phase 3: Fee Management Tables
CREATE TABLE IF NOT EXISTS fee_structures (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(200) NOT NULL,
    academic_year INTEGER NOT NULL,
    frequency VARCHAR(50) NOT NULL,
    total_amount NUMERIC(10,2) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100),
    updated_by VARCHAR(100)
);

CREATE INDEX IF NOT EXISTS ix_fee_structures_academic_year ON fee_structures(academic_year);
CREATE INDEX IF NOT EXISTS ix_fee_structures_is_active ON fee_structures(is_active);
CREATE INDEX IF NOT EXISTS ix_fee_structures_name ON fee_structures(name);

CREATE TABLE IF NOT EXISTS fee_structure_categories (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    fee_structure_id UUID NOT NULL REFERENCES fee_structures(id) ON DELETE CASCADE,
    category VARCHAR(100) NOT NULL,
    amount NUMERIC(10,2) NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100),
    updated_by VARCHAR(100)
);

CREATE INDEX IF NOT EXISTS ix_fee_structure_categories_fee_structure_id ON fee_structure_categories(fee_structure_id);

CREATE TABLE IF NOT EXISTS student_fees (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    student_id UUID NOT NULL,
    fee_structure_id UUID NOT NULL REFERENCES fee_structures(id) ON DELETE RESTRICT,
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    total_amount NUMERIC(10,2) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100),
    updated_by VARCHAR(100)
);

CREATE INDEX IF NOT EXISTS ix_student_fees_fee_structure_id ON student_fees(fee_structure_id);
CREATE INDEX IF NOT EXISTS ix_student_fees_is_active ON student_fees(is_active);
CREATE INDEX IF NOT EXISTS ix_student_fees_student_id ON student_fees(student_id);
CREATE INDEX IF NOT EXISTS ix_student_fees_student_id_fee_structure_id_dates 
    ON student_fees(student_id, fee_structure_id, start_date, end_date);

CREATE TABLE IF NOT EXISTS fee_payments (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    student_fee_id UUID NOT NULL REFERENCES student_fees(id) ON DELETE RESTRICT,
    amount_paid NUMERIC(10,2) NOT NULL,
    payment_date DATE NOT NULL,
    receipt_number VARCHAR(50) NOT NULL UNIQUE,
    payment_method VARCHAR(50) NOT NULL,
    notes TEXT,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100),
    updated_by VARCHAR(100)
);

CREATE INDEX IF NOT EXISTS ix_fee_payments_payment_date ON fee_payments(payment_date);
CREATE INDEX IF NOT EXISTS ix_fee_payments_receipt_number ON fee_payments(receipt_number);
CREATE INDEX IF NOT EXISTS ix_fee_payments_student_fee_id ON fee_payments(student_fee_id);

-- Phase 3: Attendance Tables
CREATE TABLE IF NOT EXISTS student_attendances (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    student_id UUID NOT NULL,
    class_id UUID NOT NULL,
    attendance_date DATE NOT NULL,
    status VARCHAR(20) NOT NULL,
    reason VARCHAR(500),
    marked_by_user_id UUID,
    marked_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100),
    updated_by VARCHAR(100),
    CONSTRAINT unique_student_class_date UNIQUE (student_id, class_id, attendance_date)
);

CREATE INDEX IF NOT EXISTS ix_student_attendances_attendance_date ON student_attendances(attendance_date);
CREATE INDEX IF NOT EXISTS ix_student_attendances_class_id ON student_attendances(class_id);
CREATE INDEX IF NOT EXISTS ix_student_attendances_status ON student_attendances(status);
CREATE INDEX IF NOT EXISTS ix_student_attendances_student_id ON student_attendances(student_id);

CREATE TABLE IF NOT EXISTS teacher_attendances (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    teacher_id UUID NOT NULL REFERENCES teachers(id) ON DELETE RESTRICT,
    attendance_date DATE NOT NULL,
    status VARCHAR(20) NOT NULL,
    reason VARCHAR(500),
    recorded_by_user_id UUID,
    recorded_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100),
    updated_by VARCHAR(100),
    CONSTRAINT unique_teacher_date UNIQUE (teacher_id, attendance_date)
);

CREATE INDEX IF NOT EXISTS ix_teacher_attendances_attendance_date ON teacher_attendances(attendance_date);
CREATE INDEX IF NOT EXISTS ix_teacher_attendances_status ON teacher_attendances(status);
CREATE INDEX IF NOT EXISTS ix_teacher_attendances_teacher_id ON teacher_attendances(teacher_id);

-- Record migrations in EF history
INSERT INTO "__EFMigrationsHistory" (migration_id, product_version)
VALUES 
    ('20260113000001_AddTeacherManagement', '10.0.0'),
    ('20260113000002_AddFeeManagement', '10.0.0'),
    ('20260113000003_AddAttendanceManagement', '10.0.0')
ON CONFLICT (migration_id) DO NOTHING;
