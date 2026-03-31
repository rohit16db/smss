Build started...
Build succeeded.
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE academic_years (
    "Id" uuid NOT NULL,
    name character varying(50) NOT NULL,
    start_date timestamp with time zone NOT NULL,
    end_date timestamp with time zone NOT NULL,
    is_current boolean NOT NULL,
    is_active boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" text,
    "UpdatedBy" text,
    CONSTRAINT "PK_academic_years" PRIMARY KEY ("Id")
);

CREATE TABLE classes (
    id uuid NOT NULL,
    name character varying(100) NOT NULL,
    is_active boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by text,
    updated_by text,
    CONSTRAINT "PK_classes" PRIMARY KEY (id)
);

CREATE TABLE fee_structures (
    id uuid NOT NULL DEFAULT (gen_random_uuid()),
    name character varying(100) NOT NULL,
    academic_year integer NOT NULL,
    frequency character varying(20) NOT NULL,
    total_amount numeric(12,2) NOT NULL,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by character varying(100),
    updated_by character varying(100),
    CONSTRAINT "PK_fee_structures" PRIMARY KEY (id)
);

CREATE TABLE grade_configuration (
    id uuid NOT NULL,
    grade_name character varying(10) NOT NULL,
    min_percentage numeric(5,2) NOT NULL,
    max_percentage numeric(5,2) NOT NULL,
    description character varying(255),
    school_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    "CreatedBy" text,
    "UpdatedBy" text,
    CONSTRAINT "PK_grade_configuration" PRIMARY KEY (id)
);

CREATE TABLE holidays (
    id uuid NOT NULL DEFAULT (gen_random_uuid()),
    name character varying(200) NOT NULL,
    holiday_date date NOT NULL,
    description character varying(500),
    type character varying(50),
    academic_year character varying(20) NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by character varying(100),
    updated_by character varying(100),
    CONSTRAINT "PK_holidays" PRIMARY KEY (id)
);

CREATE TABLE "SalaryStructures" (
    "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "Name" character varying(100) NOT NULL,
    "Description" character varying(500),
    "BaseSalary" numeric(18,2) NOT NULL,
    "HRA" numeric(18,2) NOT NULL DEFAULT 0.0,
    "DA" numeric(18,2) NOT NULL DEFAULT 0.0,
    "MedicalAllowance" numeric(18,2) NOT NULL DEFAULT 0.0,
    "ConveyanceAllowance" numeric(18,2) NOT NULL DEFAULT 0.0,
    "OtherAllowances" numeric(18,2) NOT NULL DEFAULT 0.0,
    "StandardDeduction" numeric(18,2) NOT NULL DEFAULT 0.0,
    "MinExperienceYears" integer NOT NULL DEFAULT 0,
    "ApplicableQualifications" character varying(500),
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "EffectiveFromDate" date NOT NULL,
    "EffectiveToDate" date,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now() at time zone 'UTC'),
    "UpdatedAt" timestamp with time zone,
    "CreatedBy" text,
    "UpdatedBy" text,
    CONSTRAINT "PK_SalaryStructures" PRIMARY KEY ("Id")
);

CREATE TABLE "Schools" (
    "Id" uuid NOT NULL,
    "Name" character varying(255) NOT NULL,
    "Code" character varying(50) NOT NULL,
    "Address" text,
    "City" text,
    "State" text,
    "PostalCode" text,
    "PhoneNumber" text,
    "EmailAddress" text,
    "Website" text,
    "LogoImage" bytea,
    "LogoFileName" text,
    "EstablishedDate" timestamp with time zone NOT NULL,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "PrimaryColor" character varying(7) NOT NULL DEFAULT '#1976D2',
    "SecondaryColor" character varying(7) NOT NULL DEFAULT '#DC004E',
    "AccentColor" character varying(7) NOT NULL DEFAULT '#FF6F00',
    "HeaderText" text,
    "FooterText" text,
    "DateFormat" character varying(20) NOT NULL DEFAULT 'dd/MM/yyyy',
    "CurrencyCode" character varying(3) NOT NULL DEFAULT 'INR',
    "CurrencySymbol" character varying(5) NOT NULL DEFAULT 'Γé╣',
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" text,
    "UpdatedBy" text,
    CONSTRAINT "PK_Schools" PRIMARY KEY ("Id")
);

CREATE TABLE students (
    id uuid NOT NULL,
    first_name character varying(50) NOT NULL,
    last_name character varying(50) NOT NULL,
    email character varying(100) NOT NULL,
    phone_number character varying(20),
    date_of_birth timestamp with time zone NOT NULL,
    address character varying(500),
    city character varying(50),
    state character varying(50),
    postal_code character varying(10),
    enrollment_number character varying(50) NOT NULL,
    enrollment_date timestamp with time zone NOT NULL,
    is_active boolean NOT NULL,
    guardian_name character varying(100),
    guardian_phone character varying(20),
    guardian_email character varying(100),
    "ImagePath" text,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by text,
    updated_by text,
    CONSTRAINT "PK_students" PRIMARY KEY (id)
);

CREATE TABLE subjects (
    id uuid NOT NULL,
    name character varying(100) NOT NULL,
    code character varying(20) NOT NULL,
    description character varying(500),
    credits integer,
    is_active boolean NOT NULL DEFAULT TRUE,
    display_order integer NOT NULL DEFAULT 0,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    "CreatedBy" text,
    "UpdatedBy" text,
    CONSTRAINT "PK_subjects" PRIMARY KEY (id)
);

CREATE TABLE users (
    id uuid NOT NULL,
    username character varying(50) NOT NULL,
    email character varying(100) NOT NULL,
    password_hash text NOT NULL,
    first_name character varying(50) NOT NULL,
    last_name character varying(50) NOT NULL,
    role integer NOT NULL,
    is_active boolean NOT NULL,
    refresh_token text,
    refresh_token_expiry_time timestamp with time zone,
    last_login_at timestamp with time zone,
    "PasswordResetToken" text,
    "PasswordResetTokenExpiry" timestamp with time zone,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by text,
    updated_by text,
    CONSTRAINT "PK_users" PRIMARY KEY (id)
);

CREATE TABLE sections (
    id uuid NOT NULL,
    class_id uuid NOT NULL,
    section_name character varying(50) NOT NULL,
    is_active boolean NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by text,
    updated_by text,
    CONSTRAINT "PK_sections" PRIMARY KEY (id),
    CONSTRAINT "FK_sections_classes_class_id" FOREIGN KEY (class_id) REFERENCES classes (id) ON DELETE CASCADE
);

CREATE TABLE fee_structure_categories (
    id uuid NOT NULL DEFAULT (gen_random_uuid()),
    fee_structure_id uuid NOT NULL,
    category character varying(50) NOT NULL,
    amount numeric(12,2) NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by character varying(100),
    updated_by character varying(100),
    CONSTRAINT "PK_fee_structure_categories" PRIMARY KEY (id),
    CONSTRAINT "FK_fee_structure_categories_fee_structures_fee_structure_id" FOREIGN KEY (fee_structure_id) REFERENCES fee_structures (id) ON DELETE CASCADE
);

CREATE TABLE teachers (
    id uuid NOT NULL DEFAULT (gen_random_uuid()),
    user_id uuid NOT NULL,
    first_name character varying(50) NOT NULL,
    last_name character varying(50) NOT NULL,
    email character varying(100) NOT NULL,
    phone character varying(20),
    qualification character varying(500),
    experience_years integer NOT NULL DEFAULT 0,
    joining_date date NOT NULL,
    is_active boolean NOT NULL DEFAULT TRUE,
    "SalaryStructureId" uuid,
    "SalaryStructureEffectiveDate" date,
    "ImagePath" text,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by character varying(100),
    updated_by character varying(100),
    CONSTRAINT "PK_teachers" PRIMARY KEY (id),
    CONSTRAINT "FK_teachers_SalaryStructures_SalaryStructureId" FOREIGN KEY ("SalaryStructureId") REFERENCES "SalaryStructures" ("Id") ON DELETE SET NULL
);

CREATE TABLE exams (
    id uuid NOT NULL,
    name character varying(255) NOT NULL,
    description character varying(1000),
    start_date timestamp with time zone NOT NULL,
    end_date timestamp with time zone NOT NULL,
    total_marks numeric(5,2) NOT NULL,
    pass_marks numeric(5,2) NOT NULL,
    status text NOT NULL,
    "AcademicYearId" uuid NOT NULL,
    created_by uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    "UpdatedBy" text,
    CONSTRAINT "PK_exams" PRIMARY KEY (id),
    CONSTRAINT "FK_exams_academic_years_AcademicYearId" FOREIGN KEY ("AcademicYearId") REFERENCES academic_years ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_exams_users_created_by" FOREIGN KEY (created_by) REFERENCES users (id) ON DELETE RESTRICT
);

CREATE TABLE enrollments (
    id uuid NOT NULL DEFAULT (gen_random_uuid()),
    student_id uuid NOT NULL,
    academic_year_id uuid NOT NULL,
    class_id uuid NOT NULL,
    section_id uuid,
    roll_number integer,
    status character varying(20) NOT NULL,
    enrollment_date timestamp with time zone NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by text,
    updated_by text,
    CONSTRAINT "PK_enrollments" PRIMARY KEY (id),
    CONSTRAINT "FK_enrollments_academic_years_academic_year_id" FOREIGN KEY (academic_year_id) REFERENCES academic_years ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_enrollments_classes_class_id" FOREIGN KEY (class_id) REFERENCES classes (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_enrollments_sections_section_id" FOREIGN KEY (section_id) REFERENCES sections (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_enrollments_students_student_id" FOREIGN KEY (student_id) REFERENCES students (id) ON DELETE CASCADE
);

CREATE TABLE "SalaryPayments" (
    "Id" uuid NOT NULL DEFAULT (gen_random_uuid()),
    "TeacherId" uuid NOT NULL,
    "PeriodStartDate" date NOT NULL,
    "PeriodEndDate" date NOT NULL,
    "BaseSalary" numeric(18,2) NOT NULL,
    "Deductions" numeric(18,2) NOT NULL DEFAULT 0.0,
    "Bonus" numeric(18,2) NOT NULL DEFAULT 0.0,
    "NetSalary" numeric(18,2) NOT NULL,
    "Status" text NOT NULL DEFAULT 'Pending',
    "PaidDate" date,
    "ReferenceNumber" character varying(100),
    "PaymentMethod" text,
    "Remarks" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now() at time zone 'UTC'),
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_SalaryPayments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_SalaryPayments_teachers_TeacherId" FOREIGN KEY ("TeacherId") REFERENCES teachers (id) ON DELETE RESTRICT
);

CREATE TABLE teacher_assignments (
    id uuid NOT NULL DEFAULT (gen_random_uuid()),
    teacher_id uuid NOT NULL,
    class_id uuid NOT NULL,
    subject_id uuid NOT NULL,
    assignment_date date NOT NULL,
    removal_date date,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by character varying(100),
    updated_by character varying(100),
    CONSTRAINT "PK_teacher_assignments" PRIMARY KEY (id),
    CONSTRAINT "FK_teacher_assignments_subjects_subject_id" FOREIGN KEY (subject_id) REFERENCES subjects (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_teacher_assignments_teachers_teacher_id" FOREIGN KEY (teacher_id) REFERENCES teachers (id) ON DELETE RESTRICT
);

CREATE TABLE teacher_attendances (
    id uuid NOT NULL DEFAULT (gen_random_uuid()),
    teacher_id uuid NOT NULL,
    attendance_date date NOT NULL,
    status character varying(20) NOT NULL,
    reason character varying(500),
    recorded_by_user_id uuid,
    recorded_at timestamp with time zone NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by character varying(100),
    updated_by character varying(100),
    CONSTRAINT "PK_teacher_attendances" PRIMARY KEY (id),
    CONSTRAINT ck_teacher_attendances_status CHECK (status IN ('present', 'absent', 'leave')),
    CONSTRAINT "FK_teacher_attendances_teachers_teacher_id" FOREIGN KEY (teacher_id) REFERENCES teachers (id) ON DELETE RESTRICT
);

CREATE TABLE exam_classes (
    id uuid NOT NULL,
    exam_id uuid NOT NULL,
    class_id uuid NOT NULL,
    marks_entry_status text NOT NULL,
    submitted_at timestamp with time zone,
    submitted_by uuid,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    "CreatedBy" text,
    "UpdatedBy" text,
    CONSTRAINT "PK_exam_classes" PRIMARY KEY (id),
    CONSTRAINT "FK_exam_classes_classes_class_id" FOREIGN KEY (class_id) REFERENCES classes (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_exam_classes_exams_exam_id" FOREIGN KEY (exam_id) REFERENCES exams (id) ON DELETE CASCADE,
    CONSTRAINT "FK_exam_classes_users_submitted_by" FOREIGN KEY (submitted_by) REFERENCES users (id) ON DELETE SET NULL
);

CREATE TABLE exam_subjects (
    id uuid NOT NULL,
    exam_id uuid NOT NULL,
    subject_id uuid NOT NULL,
    max_marks numeric(5,2) NOT NULL,
    pass_marks numeric(5,2) NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    "CreatedBy" text,
    "UpdatedBy" text,
    CONSTRAINT "PK_exam_subjects" PRIMARY KEY (id),
    CONSTRAINT "AK_exam_subjects_exam_id_subject_id" UNIQUE (exam_id, subject_id),
    CONSTRAINT "FK_exam_subjects_exams_exam_id" FOREIGN KEY (exam_id) REFERENCES exams (id) ON DELETE CASCADE,
    CONSTRAINT "FK_exam_subjects_subjects_subject_id" FOREIGN KEY (subject_id) REFERENCES subjects (id) ON DELETE RESTRICT
);

CREATE TABLE student_attendances (
    id uuid NOT NULL DEFAULT (gen_random_uuid()),
    enrollment_id uuid NOT NULL,
    attendance_date date NOT NULL,
    status character varying(20) NOT NULL,
    reason character varying(500),
    marked_by_user_id uuid,
    marked_at timestamp with time zone NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by character varying(100),
    updated_by character varying(100),
    CONSTRAINT "PK_student_attendances" PRIMARY KEY (id),
    CONSTRAINT ck_student_attendances_status CHECK (status IN ('present', 'absent', 'leave', 'unexcused')),
    CONSTRAINT "FK_student_attendances_enrollments_enrollment_id" FOREIGN KEY (enrollment_id) REFERENCES enrollments (id) ON DELETE RESTRICT
);

CREATE TABLE student_fees (
    id uuid NOT NULL DEFAULT (gen_random_uuid()),
    enrollment_id uuid NOT NULL,
    fee_structure_id uuid NOT NULL,
    start_date date NOT NULL,
    end_date date,
    total_amount numeric(12,2) NOT NULL,
    is_active boolean NOT NULL DEFAULT TRUE,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by character varying(100),
    updated_by character varying(100),
    CONSTRAINT "PK_student_fees" PRIMARY KEY (id),
    CONSTRAINT ck_student_fees_date_range CHECK (end_date IS NULL OR start_date <= end_date),
    CONSTRAINT "FK_student_fees_enrollments_enrollment_id" FOREIGN KEY (enrollment_id) REFERENCES enrollments (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_student_fees_fee_structures_fee_structure_id" FOREIGN KEY (fee_structure_id) REFERENCES fee_structures (id) ON DELETE RESTRICT
);

CREATE TABLE student_report_cards (
    id uuid NOT NULL,
    exam_id uuid NOT NULL,
    enrollment_id uuid NOT NULL,
    total_marks_obtained numeric(7,2) NOT NULL,
    total_marks numeric(7,2) NOT NULL,
    percentage numeric(5,2) NOT NULL,
    overall_grade character varying(10) NOT NULL,
    class_position integer NOT NULL,
    pass boolean NOT NULL,
    remarks character varying(1000),
    generated_at timestamp with time zone,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    "CreatedBy" text,
    "UpdatedBy" text,
    CONSTRAINT "PK_student_report_cards" PRIMARY KEY (id),
    CONSTRAINT "FK_student_report_cards_enrollments_enrollment_id" FOREIGN KEY (enrollment_id) REFERENCES enrollments (id) ON DELETE CASCADE,
    CONSTRAINT "FK_student_report_cards_exams_exam_id" FOREIGN KEY (exam_id) REFERENCES exams (id) ON DELETE CASCADE
);

CREATE TABLE student_marks (
    id uuid NOT NULL,
    exam_id uuid NOT NULL,
    enrollment_id uuid NOT NULL,
    subject_id uuid NOT NULL,
    marks_obtained numeric(5,2),
    is_absent boolean NOT NULL DEFAULT FALSE,
    remarks character varying(500),
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    "CreatedBy" text,
    "UpdatedBy" text,
    CONSTRAINT "PK_student_marks" PRIMARY KEY (id),
    CONSTRAINT "FK_student_marks_enrollments_enrollment_id" FOREIGN KEY (enrollment_id) REFERENCES enrollments (id) ON DELETE CASCADE,
    CONSTRAINT "FK_student_marks_exam_subjects_exam_id_subject_id" FOREIGN KEY (exam_id, subject_id) REFERENCES exam_subjects (exam_id, subject_id) ON DELETE RESTRICT,
    CONSTRAINT "FK_student_marks_exams_exam_id" FOREIGN KEY (exam_id) REFERENCES exams (id) ON DELETE CASCADE
);

CREATE TABLE fee_payments (
    id uuid NOT NULL DEFAULT (gen_random_uuid()),
    student_fee_id uuid NOT NULL,
    amount_paid numeric(12,2) NOT NULL,
    payment_date date NOT NULL,
    receipt_number character varying(50) NOT NULL,
    payment_method character varying(20) NOT NULL,
    notes character varying(500),
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by character varying(100),
    updated_by character varying(100),
    CONSTRAINT "PK_fee_payments" PRIMARY KEY (id),
    CONSTRAINT ck_fee_payments_amount_positive CHECK (amount_paid > 0),
    CONSTRAINT "FK_fee_payments_student_fees_student_fee_id" FOREIGN KEY (student_fee_id) REFERENCES student_fees (id) ON DELETE RESTRICT
);

CREATE INDEX "IX_classes_is_active" ON classes (is_active);

CREATE UNIQUE INDEX "IX_classes_name" ON classes (name);

CREATE INDEX "IX_enrollments_academic_year_id" ON enrollments (academic_year_id);

CREATE INDEX "IX_enrollments_class_id" ON enrollments (class_id);

CREATE INDEX "IX_enrollments_section_id" ON enrollments (section_id);

CREATE INDEX "IX_enrollments_status" ON enrollments (status);

CREATE INDEX "IX_enrollments_student_id" ON enrollments (student_id);

CREATE UNIQUE INDEX "IX_enrollments_student_id_academic_year_id" ON enrollments (student_id, academic_year_id) WHERE status = 'Enrolled';

CREATE INDEX "IX_exam_classes_class_id" ON exam_classes (class_id);

CREATE UNIQUE INDEX "IX_exam_classes_exam_id_class_id" ON exam_classes (exam_id, class_id);

CREATE INDEX "IX_exam_classes_marks_entry_status" ON exam_classes (marks_entry_status);

CREATE INDEX "IX_exam_classes_submitted_by" ON exam_classes (submitted_by);

CREATE UNIQUE INDEX "IX_exam_subjects_exam_id_subject_id" ON exam_subjects (exam_id, subject_id);

CREATE INDEX "IX_exam_subjects_subject_id" ON exam_subjects (subject_id);

CREATE INDEX "IX_exams_AcademicYearId" ON exams ("AcademicYearId");

CREATE INDEX "IX_exams_created_by" ON exams (created_by);

CREATE INDEX "IX_exams_end_date" ON exams (end_date);

CREATE UNIQUE INDEX "IX_exams_name_start_date" ON exams (name, start_date);

CREATE INDEX "IX_exams_start_date" ON exams (start_date);

CREATE INDEX "IX_exams_status" ON exams (status);

CREATE INDEX "IX_fee_payments_payment_date" ON fee_payments (payment_date);

CREATE UNIQUE INDEX "IX_fee_payments_receipt_number" ON fee_payments (receipt_number);

CREATE INDEX "IX_fee_payments_student_fee_id" ON fee_payments (student_fee_id);

CREATE INDEX "IX_fee_structure_categories_fee_structure_id" ON fee_structure_categories (fee_structure_id);

CREATE UNIQUE INDEX "IX_fee_structure_categories_fee_structure_id_category" ON fee_structure_categories (fee_structure_id, category);

CREATE INDEX "IX_fee_structures_academic_year" ON fee_structures (academic_year);

CREATE INDEX "IX_fee_structures_is_active" ON fee_structures (is_active);

CREATE INDEX "IX_grade_configuration_school_id" ON grade_configuration (school_id);

CREATE UNIQUE INDEX "IX_grade_configuration_school_id_grade_name" ON grade_configuration (school_id, grade_name);

CREATE INDEX "IX_holidays_academic_year" ON holidays (academic_year);

CREATE INDEX "IX_holidays_academic_year_holiday_date" ON holidays (academic_year, holiday_date);

CREATE INDEX "IX_holidays_holiday_date" ON holidays (holiday_date);

CREATE UNIQUE INDEX "IX_holidays_holiday_date_academic_year" ON holidays (holiday_date, academic_year);

CREATE INDEX "IX_holidays_type" ON holidays (type);

CREATE INDEX "IX_SalaryPayments_CreatedAt" ON "SalaryPayments" ("CreatedAt");

CREATE INDEX "IX_SalaryPayments_PeriodStartDate_PeriodEndDate" ON "SalaryPayments" ("PeriodStartDate", "PeriodEndDate");

CREATE INDEX "IX_SalaryPayments_Status" ON "SalaryPayments" ("Status");

CREATE INDEX "IX_SalaryPayments_TeacherId" ON "SalaryPayments" ("TeacherId");

CREATE INDEX "IX_SalaryStructures_EffectiveFromDate" ON "SalaryStructures" ("EffectiveFromDate");

CREATE INDEX "IX_SalaryStructures_IsActive" ON "SalaryStructures" ("IsActive");

CREATE UNIQUE INDEX "IX_Schools_Code" ON "Schools" ("Code");

CREATE UNIQUE INDEX "IX_Schools_EmailAddress" ON "Schools" ("EmailAddress");

CREATE INDEX "IX_sections_class_id" ON sections (class_id);

CREATE UNIQUE INDEX "IX_sections_class_id_section_name" ON sections (class_id, section_name);

CREATE INDEX "IX_sections_is_active" ON sections (is_active);

CREATE INDEX "IX_student_attendances_attendance_date" ON student_attendances (attendance_date);

CREATE INDEX "IX_student_attendances_enrollment_id" ON student_attendances (enrollment_id);

CREATE UNIQUE INDEX "IX_student_attendances_enrollment_id_attendance_date" ON student_attendances (enrollment_id, attendance_date) INCLUDE (status);

CREATE INDEX "IX_student_attendances_status" ON student_attendances (status);

CREATE INDEX "IX_student_fees_enrollment_id" ON student_fees (enrollment_id);

CREATE INDEX "IX_student_fees_fee_structure_id" ON student_fees (fee_structure_id);

CREATE INDEX "IX_student_fees_start_date_end_date" ON student_fees (start_date, end_date);

CREATE INDEX "IX_student_marks_enrollment_id" ON student_marks (enrollment_id);

CREATE INDEX "IX_student_marks_exam_id" ON student_marks (exam_id);

CREATE UNIQUE INDEX "IX_student_marks_exam_id_enrollment_id_subject_id" ON student_marks (exam_id, enrollment_id, subject_id);

CREATE INDEX "IX_student_marks_exam_id_subject_id" ON student_marks (exam_id, subject_id);

CREATE INDEX "IX_student_report_cards_enrollment_id" ON student_report_cards (enrollment_id);

CREATE INDEX "IX_student_report_cards_exam_id" ON student_report_cards (exam_id);

CREATE UNIQUE INDEX "IX_student_report_cards_exam_id_enrollment_id" ON student_report_cards (exam_id, enrollment_id);

CREATE INDEX "IX_student_report_cards_pass" ON student_report_cards (pass);

CREATE INDEX "IX_students_city" ON students (city);

CREATE UNIQUE INDEX "IX_students_email" ON students (email);

CREATE UNIQUE INDEX "IX_students_enrollment_number" ON students (enrollment_number);

CREATE INDEX "IX_students_is_active" ON students (is_active);

CREATE UNIQUE INDEX idx_subjects_code ON subjects (code);

CREATE INDEX idx_subjects_is_active ON subjects (is_active);

CREATE INDEX idx_subjects_name ON subjects (name);

CREATE INDEX "IX_teacher_assignments_class_id" ON teacher_assignments (class_id);

CREATE INDEX "IX_teacher_assignments_removal_date" ON teacher_assignments (removal_date);

CREATE INDEX "IX_teacher_assignments_subject_id" ON teacher_assignments (subject_id);

CREATE INDEX "IX_teacher_assignments_teacher_id" ON teacher_assignments (teacher_id);

CREATE UNIQUE INDEX "IX_teacher_assignments_teacher_id_class_id_subject_id_removal_~" ON teacher_assignments (teacher_id, class_id, subject_id, removal_date) WHERE removal_date IS NULL;

CREATE INDEX "IX_teacher_attendances_attendance_date" ON teacher_attendances (attendance_date);

CREATE INDEX "IX_teacher_attendances_teacher_id" ON teacher_attendances (teacher_id);

CREATE UNIQUE INDEX "IX_teacher_attendances_teacher_id_attendance_date" ON teacher_attendances (teacher_id, attendance_date) INCLUDE (status);

CREATE UNIQUE INDEX "IX_teachers_email" ON teachers (email);

CREATE INDEX "IX_teachers_is_active" ON teachers (is_active);

CREATE INDEX "IX_teachers_joining_date" ON teachers (joining_date);

CREATE INDEX "IX_teachers_SalaryStructureId" ON teachers ("SalaryStructureId");

CREATE UNIQUE INDEX "IX_teachers_user_id" ON teachers (user_id);

CREATE UNIQUE INDEX "IX_users_email" ON users (email);

CREATE UNIQUE INDEX "IX_users_username" ON users (username);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260325122843_InitialAcademicYear', '10.0.1');

COMMIT;

START TRANSACTION;
DROP INDEX "IX_holidays_academic_year";

DROP INDEX "IX_holidays_academic_year_holiday_date";

DROP INDEX "IX_holidays_holiday_date_academic_year";

DROP INDEX "IX_fee_structures_academic_year";

ALTER TABLE holidays DROP COLUMN academic_year;

ALTER TABLE fee_structures DROP COLUMN academic_year;

ALTER TABLE holidays ADD academic_year_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

ALTER TABLE fee_structures ADD academic_year_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

CREATE INDEX "IX_holidays_academic_year_id" ON holidays (academic_year_id);

CREATE INDEX "IX_holidays_academic_year_id_holiday_date" ON holidays (academic_year_id, holiday_date);

CREATE UNIQUE INDEX "IX_holidays_holiday_date_academic_year_id" ON holidays (holiday_date, academic_year_id);

CREATE INDEX "IX_fee_structures_academic_year_id" ON fee_structures (academic_year_id);

ALTER TABLE fee_structures ADD CONSTRAINT "FK_fee_structures_academic_years_academic_year_id" FOREIGN KEY (academic_year_id) REFERENCES academic_years ("Id") ON DELETE RESTRICT;

ALTER TABLE holidays ADD CONSTRAINT "FK_holidays_academic_years_academic_year_id" FOREIGN KEY (academic_year_id) REFERENCES academic_years ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260325124541_UpdateAcademicYearRelationships', '10.0.1');

COMMIT;

START TRANSACTION;
ALTER TABLE exams DROP CONSTRAINT "FK_exams_academic_years_AcademicYearId";

ALTER TABLE teacher_assignments DROP CONSTRAINT "FK_teacher_assignments_teachers_teacher_id";

ALTER TABLE exams RENAME COLUMN "AcademicYearId" TO academic_year_id;

ALTER INDEX "IX_exams_AcademicYearId" RENAME TO "IX_exams_academic_year_id";

ALTER TABLE teacher_assignments ADD "AcademicYearId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

UPDATE teacher_assignments SET "AcademicYearId" = (SELECT "Id" FROM academic_years WHERE is_active = true LIMIT 1)

CREATE INDEX "IX_teacher_assignments_AcademicYearId" ON teacher_assignments ("AcademicYearId");

ALTER TABLE exams ADD CONSTRAINT "FK_exams_academic_years_academic_year_id" FOREIGN KEY (academic_year_id) REFERENCES academic_years ("Id") ON DELETE RESTRICT;

ALTER TABLE teacher_assignments ADD CONSTRAINT "FK_teacher_assignments_academic_years_AcademicYearId" FOREIGN KEY ("AcademicYearId") REFERENCES academic_years ("Id") ON DELETE RESTRICT;

ALTER TABLE teacher_assignments ADD CONSTRAINT "FK_teacher_assignments_teachers_teacher_id" FOREIGN KEY (teacher_id) REFERENCES teachers (id) ON DELETE CASCADE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260325135445_AddAcademicYearToTeacherAssignment', '10.0.1');

COMMIT;

START TRANSACTION;
CREATE TABLE time_slots (
    id uuid NOT NULL,
    day_of_week integer NOT NULL,
    start_time interval NOT NULL,
    end_time interval NOT NULL,
    name character varying(100) NOT NULL,
    is_break boolean NOT NULL,
    academic_year_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by text,
    updated_by text,
    CONSTRAINT "PK_time_slots" PRIMARY KEY (id),
    CONSTRAINT "FK_time_slots_academic_years_academic_year_id" FOREIGN KEY (academic_year_id) REFERENCES academic_years ("Id") ON DELETE RESTRICT
);

CREATE TABLE timetable_entries (
    id uuid NOT NULL,
    time_slot_id uuid NOT NULL,
    section_id uuid NOT NULL,
    subject_id uuid NOT NULL,
    teacher_id uuid NOT NULL,
    room_number character varying(50),
    academic_year_id uuid NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by text,
    updated_by text,
    CONSTRAINT "PK_timetable_entries" PRIMARY KEY (id),
    CONSTRAINT "FK_timetable_entries_academic_years_academic_year_id" FOREIGN KEY (academic_year_id) REFERENCES academic_years ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_timetable_entries_sections_section_id" FOREIGN KEY (section_id) REFERENCES sections (id) ON DELETE CASCADE,
    CONSTRAINT "FK_timetable_entries_subjects_subject_id" FOREIGN KEY (subject_id) REFERENCES subjects (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_timetable_entries_teachers_teacher_id" FOREIGN KEY (teacher_id) REFERENCES teachers (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_timetable_entries_time_slots_time_slot_id" FOREIGN KEY (time_slot_id) REFERENCES time_slots (id) ON DELETE CASCADE
);

CREATE INDEX "IX_time_slots_academic_year_id" ON time_slots (academic_year_id);

CREATE UNIQUE INDEX "IX_time_slots_academic_year_id_day_of_week_start_time_end_time" ON time_slots (academic_year_id, day_of_week, start_time, end_time);

CREATE INDEX "IX_timetable_entries_academic_year_id" ON timetable_entries (academic_year_id);

CREATE UNIQUE INDEX "IX_timetable_entries_academic_year_id_time_slot_id_section_id" ON timetable_entries (academic_year_id, time_slot_id, section_id);

CREATE UNIQUE INDEX "IX_timetable_entries_academic_year_id_time_slot_id_teacher_id" ON timetable_entries (academic_year_id, time_slot_id, teacher_id);

CREATE INDEX "IX_timetable_entries_section_id" ON timetable_entries (section_id);

CREATE INDEX "IX_timetable_entries_subject_id" ON timetable_entries (subject_id);

CREATE INDEX "IX_timetable_entries_teacher_id" ON timetable_entries (teacher_id);

CREATE INDEX "IX_timetable_entries_time_slot_id" ON timetable_entries (time_slot_id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260325150155_AddTimetableEntities', '10.0.1');

COMMIT;

START TRANSACTION;
ALTER TABLE "SalaryPayments" DROP CONSTRAINT "FK_SalaryPayments_teachers_TeacherId";

ALTER TABLE timetable_entries DROP CONSTRAINT "FK_timetable_entries_teachers_teacher_id";

DROP TABLE teacher_assignments;

DROP TABLE teacher_attendances;

DROP TABLE teachers;

ALTER TABLE timetable_entries RENAME COLUMN teacher_id TO staff_id;

ALTER INDEX "IX_timetable_entries_teacher_id" RENAME TO "IX_timetable_entries_staff_id";

ALTER INDEX "IX_timetable_entries_academic_year_id_time_slot_id_teacher_id" RENAME TO "IX_timetable_entries_academic_year_id_time_slot_id_staff_id";

ALTER TABLE "SalaryPayments" RENAME COLUMN "TeacherId" TO "StaffId";

ALTER INDEX "IX_SalaryPayments_TeacherId" RENAME TO "IX_SalaryPayments_StaffId";

UPDATE "SalaryStructures" SET "UpdatedAt" = TIMESTAMPTZ '-infinity' WHERE "UpdatedAt" IS NULL;
ALTER TABLE "SalaryStructures" ALTER COLUMN "UpdatedAt" SET NOT NULL;
ALTER TABLE "SalaryStructures" ALTER COLUMN "UpdatedAt" SET DEFAULT TIMESTAMPTZ '-infinity';

ALTER TABLE exams ADD "CreatedBy" text;

CREATE TABLE "UserProfiles" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "FirstName" text NOT NULL,
    "LastName" text NOT NULL,
    "Email" text NOT NULL,
    "Phone" text,
    "DateOfBirth" date,
    "BloodGroup" text,
    "Gender" text,
    "CurrentAddress" text,
    "PermanentAddress" text,
    "EmergencyContactName" text,
    "EmergencyContactPhone" text,
    "ImagePath" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" text,
    "UpdatedBy" text,
    CONSTRAINT "PK_UserProfiles" PRIMARY KEY ("Id")
);

CREATE TABLE "Departments" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "Description" text,
    "HeadOfDepartmentId" uuid,
    "HeadOfDepartmentId1" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" text,
    "UpdatedBy" text,
    CONSTRAINT "PK_Departments" PRIMARY KEY ("Id")
);

CREATE TABLE staff (
    id uuid NOT NULL DEFAULT (gen_random_uuid()),
    user_profile_id uuid NOT NULL,
    department_id uuid NOT NULL,
    designation character varying(100) NOT NULL,
    role_type integer NOT NULL,
    experience_years integer NOT NULL DEFAULT 0,
    joining_date date NOT NULL,
    is_active boolean NOT NULL DEFAULT TRUE,
    basic_salary numeric(18,2) NOT NULL,
    salary_structure_id uuid,
    salary_structure_effective_date date,
    "DepartmentId1" uuid,
    "SalaryStructureId1" uuid,
    "UserProfileId1" uuid,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by character varying(100),
    updated_by character varying(100),
    CONSTRAINT "PK_staff" PRIMARY KEY (id),
    CONSTRAINT "FK_staff_Departments_DepartmentId1" FOREIGN KEY ("DepartmentId1") REFERENCES "Departments" ("Id"),
    CONSTRAINT "FK_staff_Departments_department_id" FOREIGN KEY (department_id) REFERENCES "Departments" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_staff_SalaryStructures_SalaryStructureId1" FOREIGN KEY ("SalaryStructureId1") REFERENCES "SalaryStructures" ("Id"),
    CONSTRAINT "FK_staff_SalaryStructures_salary_structure_id" FOREIGN KEY (salary_structure_id) REFERENCES "SalaryStructures" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_staff_UserProfiles_UserProfileId1" FOREIGN KEY ("UserProfileId1") REFERENCES "UserProfiles" ("Id"),
    CONSTRAINT "FK_staff_UserProfiles_user_profile_id" FOREIGN KEY (user_profile_id) REFERENCES "UserProfiles" ("Id") ON DELETE CASCADE
);

CREATE TABLE "EducationalQualifications" (
    "Id" uuid NOT NULL,
    "StaffId" uuid NOT NULL,
    "DegreeName" text NOT NULL,
    "Institution" text NOT NULL,
    "YearOfPassing" integer NOT NULL,
    "GradeOrPercentage" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" text,
    "UpdatedBy" text,
    CONSTRAINT "PK_EducationalQualifications" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_EducationalQualifications_staff_StaffId" FOREIGN KEY ("StaffId") REFERENCES staff (id) ON DELETE CASCADE
);

CREATE TABLE staff_assignments (
    id uuid NOT NULL DEFAULT (gen_random_uuid()),
    staff_id uuid NOT NULL,
    class_id uuid NOT NULL,
    subject_id uuid NOT NULL,
    academic_year_id uuid NOT NULL,
    assignment_date date NOT NULL,
    removal_date date,
    "AcademicYearId1" uuid,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by character varying(100),
    updated_by character varying(100),
    CONSTRAINT "PK_staff_assignments" PRIMARY KEY (id),
    CONSTRAINT "FK_staff_assignments_academic_years_AcademicYearId1" FOREIGN KEY ("AcademicYearId1") REFERENCES academic_years ("Id"),
    CONSTRAINT "FK_staff_assignments_academic_years_academic_year_id" FOREIGN KEY (academic_year_id) REFERENCES academic_years ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_staff_assignments_staff_staff_id" FOREIGN KEY (staff_id) REFERENCES staff (id) ON DELETE RESTRICT,
    CONSTRAINT "FK_staff_assignments_subjects_subject_id" FOREIGN KEY (subject_id) REFERENCES subjects (id) ON DELETE RESTRICT
);

CREATE TABLE staff_attendances (
    id uuid NOT NULL DEFAULT (gen_random_uuid()),
    staff_id uuid NOT NULL,
    attendance_date date NOT NULL,
    status character varying(20) NOT NULL,
    reason character varying(500),
    recorded_by_user_id uuid,
    recorded_at timestamp with time zone NOT NULL,
    created_at timestamp with time zone NOT NULL,
    updated_at timestamp with time zone NOT NULL,
    created_by character varying(100),
    updated_by character varying(100),
    CONSTRAINT "PK_staff_attendances" PRIMARY KEY (id),
    CONSTRAINT ck_staff_attendances_status CHECK (status IN ('present', 'absent', 'leave')),
    CONSTRAINT "FK_staff_attendances_staff_staff_id" FOREIGN KEY (staff_id) REFERENCES staff (id) ON DELETE RESTRICT
);

CREATE INDEX "IX_Departments_HeadOfDepartmentId1" ON "Departments" ("HeadOfDepartmentId1");

CREATE INDEX "IX_EducationalQualifications_StaffId" ON "EducationalQualifications" ("StaffId");

CREATE INDEX "IX_staff_department_id" ON staff (department_id);

CREATE INDEX "IX_staff_DepartmentId1" ON staff ("DepartmentId1");

CREATE INDEX "IX_staff_is_active" ON staff (is_active);

CREATE INDEX "IX_staff_joining_date" ON staff (joining_date);

CREATE INDEX "IX_staff_salary_structure_id" ON staff (salary_structure_id);

CREATE INDEX "IX_staff_SalaryStructureId1" ON staff ("SalaryStructureId1");

CREATE INDEX "IX_staff_user_profile_id" ON staff (user_profile_id);

CREATE INDEX "IX_staff_UserProfileId1" ON staff ("UserProfileId1");

CREATE INDEX "IX_staff_assignments_academic_year_id" ON staff_assignments (academic_year_id);

CREATE INDEX "IX_staff_assignments_AcademicYearId1" ON staff_assignments ("AcademicYearId1");

CREATE INDEX "IX_staff_assignments_class_id" ON staff_assignments (class_id);

CREATE INDEX "IX_staff_assignments_removal_date" ON staff_assignments (removal_date);

CREATE INDEX "IX_staff_assignments_staff_id" ON staff_assignments (staff_id);

CREATE UNIQUE INDEX "IX_staff_assignments_staff_id_class_id_subject_id_removal_date" ON staff_assignments (staff_id, class_id, subject_id, removal_date) WHERE removal_date IS NULL;

CREATE INDEX "IX_staff_assignments_subject_id" ON staff_assignments (subject_id);

CREATE INDEX "IX_staff_attendances_attendance_date" ON staff_attendances (attendance_date);

CREATE INDEX "IX_staff_attendances_staff_id" ON staff_attendances (staff_id);

CREATE UNIQUE INDEX "IX_staff_attendances_staff_id_attendance_date" ON staff_attendances (staff_id, attendance_date);

ALTER TABLE "SalaryPayments" ADD CONSTRAINT "FK_SalaryPayments_staff_StaffId" FOREIGN KEY ("StaffId") REFERENCES staff (id) ON DELETE RESTRICT;

ALTER TABLE timetable_entries ADD CONSTRAINT "FK_timetable_entries_staff_staff_id" FOREIGN KEY (staff_id) REFERENCES staff (id) ON DELETE RESTRICT;

ALTER TABLE "Departments" ADD CONSTRAINT "FK_Departments_staff_HeadOfDepartmentId1" FOREIGN KEY ("HeadOfDepartmentId1") REFERENCES staff (id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260326131030_SynchronizeStaffAndFixMappings', '10.0.1');

COMMIT;

START TRANSACTION;
ALTER TABLE "Departments" DROP CONSTRAINT "FK_Departments_staff_HeadOfDepartmentId1";

ALTER TABLE staff DROP CONSTRAINT "FK_staff_Departments_DepartmentId1";

ALTER TABLE staff DROP CONSTRAINT "FK_staff_Departments_department_id";

DROP INDEX "IX_staff_DepartmentId1";

DROP INDEX "IX_Departments_HeadOfDepartmentId1";

ALTER TABLE staff DROP COLUMN "DepartmentId1";

ALTER TABLE "Departments" DROP COLUMN "HeadOfDepartmentId1";

ALTER TABLE staff ALTER COLUMN department_id DROP NOT NULL;

ALTER TABLE "Departments" ALTER COLUMN "Name" TYPE character varying(100);

ALTER TABLE "Departments" ALTER COLUMN "Description" TYPE character varying(500);

ALTER TABLE "Departments" ALTER COLUMN "CreatedAt" SET DEFAULT (now() at time zone 'UTC');

ALTER TABLE "Departments" ALTER COLUMN "Id" SET DEFAULT (gen_random_uuid());

CREATE INDEX "IX_Departments_HeadOfDepartmentId" ON "Departments" ("HeadOfDepartmentId");

CREATE UNIQUE INDEX "IX_Departments_Name" ON "Departments" ("Name");

ALTER TABLE "Departments" ADD CONSTRAINT "FK_Departments_staff_HeadOfDepartmentId" FOREIGN KEY ("HeadOfDepartmentId") REFERENCES staff (id) ON DELETE SET NULL;

ALTER TABLE staff ADD CONSTRAINT "FK_staff_Departments_department_id" FOREIGN KEY (department_id) REFERENCES "Departments" ("Id") ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260326150741_AddDepartmentsFinal', '10.0.1');

COMMIT;

START TRANSACTION;
TRUNCATE TABLE timetable_entries;

TRUNCATE TABLE staff_assignments CASCADE;

ALTER TABLE timetable_entries DROP CONSTRAINT "FK_timetable_entries_sections_section_id";

ALTER TABLE timetable_entries DROP CONSTRAINT "FK_timetable_entries_staff_staff_id";

ALTER TABLE timetable_entries DROP CONSTRAINT "FK_timetable_entries_subjects_subject_id";

DROP INDEX "IX_timetable_entries_academic_year_id_time_slot_id_section_id";

DROP INDEX "IX_timetable_entries_academic_year_id_time_slot_id_staff_id";

DROP INDEX "IX_timetable_entries_section_id";

DROP INDEX "IX_timetable_entries_staff_id";

DROP INDEX "IX_staff_assignments_staff_id_class_id_subject_id_removal_date";

ALTER TABLE timetable_entries DROP COLUMN section_id;

ALTER TABLE timetable_entries DROP COLUMN staff_id;

ALTER TABLE timetable_entries RENAME COLUMN subject_id TO staff_assignment_id;

ALTER INDEX "IX_timetable_entries_subject_id" RENAME TO "IX_timetable_entries_staff_assignment_id";

ALTER TABLE staff_assignments ADD section_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';

CREATE UNIQUE INDEX "IX_timetable_entries_academic_year_id_time_slot_id_staff_assig~" ON timetable_entries (academic_year_id, time_slot_id, staff_assignment_id);

CREATE INDEX "IX_staff_assignments_section_id" ON staff_assignments (section_id);

CREATE UNIQUE INDEX "IX_staff_assignments_staff_id_section_id_subject_id_removal_da~" ON staff_assignments (staff_id, section_id, subject_id, removal_date) WHERE removal_date IS NULL;

ALTER TABLE staff_assignments ADD CONSTRAINT "FK_staff_assignments_classes_class_id" FOREIGN KEY (class_id) REFERENCES classes (id) ON DELETE RESTRICT;

ALTER TABLE staff_assignments ADD CONSTRAINT "FK_staff_assignments_sections_section_id" FOREIGN KEY (section_id) REFERENCES sections (id) ON DELETE RESTRICT;

ALTER TABLE timetable_entries ADD CONSTRAINT "FK_timetable_entries_staff_assignments_staff_assignment_id" FOREIGN KEY (staff_assignment_id) REFERENCES staff_assignments (id) ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260330062158_MoveSectionToStaffAssignment', '10.0.1');

COMMIT;

START TRANSACTION;
ALTER TABLE staff DROP CONSTRAINT "FK_staff_SalaryStructures_SalaryStructureId1";

ALTER TABLE staff DROP CONSTRAINT "FK_staff_UserProfiles_UserProfileId1";

ALTER TABLE staff_assignments DROP CONSTRAINT "FK_staff_assignments_academic_years_AcademicYearId1";

DROP INDEX "IX_staff_assignments_AcademicYearId1";

DROP INDEX "IX_staff_SalaryStructureId1";

DROP INDEX "IX_staff_UserProfileId1";

ALTER TABLE staff_assignments DROP COLUMN "AcademicYearId1";

ALTER TABLE staff DROP COLUMN "SalaryStructureId1";

ALTER TABLE staff DROP COLUMN "UserProfileId1";

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260330064821_FixShadowProperties', '10.0.1');

COMMIT;

START TRANSACTION;
ALTER TABLE staff_assignments DROP CONSTRAINT "FK_staff_assignments_subjects_SubjectId1";

DROP INDEX "IX_staff_assignments_SubjectId1";

ALTER TABLE staff_assignments DROP COLUMN "SubjectId1";

ALTER TABLE student_fees RENAME COLUMN total_amount TO structure_amount;

ALTER TABLE student_fees ADD paid_amount numeric(12,2) NOT NULL DEFAULT 0.0;

ALTER TABLE student_fees ADD transport_fee_amount numeric(12,2) NOT NULL DEFAULT 0.0;

CREATE TABLE "Vehicles" (
    "Id" uuid NOT NULL,
    "RegistrationNumber" text NOT NULL,
    "Model" text NOT NULL,
    "Capacity" integer NOT NULL,
    "DriverName" text NOT NULL,
    "DriverPhone" text NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" text,
    "UpdatedBy" text,
    CONSTRAINT "PK_Vehicles" PRIMARY KEY ("Id")
);

CREATE TABLE "TransportRoutes" (
    "Id" uuid NOT NULL,
    "RouteName" text NOT NULL,
    "Description" text NOT NULL,
    "VehicleId" uuid,
    "MonthlyFee" numeric NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" text,
    "UpdatedBy" text,
    CONSTRAINT "PK_TransportRoutes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_TransportRoutes_Vehicles_VehicleId" FOREIGN KEY ("VehicleId") REFERENCES "Vehicles" ("Id")
);

CREATE TABLE "RouteStops" (
    "Id" uuid NOT NULL,
    "RouteId" uuid NOT NULL,
    "StopName" text NOT NULL,
    "PickupTime" text NOT NULL,
    "DropoffTime" text NOT NULL,
    "Sequence" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" text,
    "UpdatedBy" text,
    CONSTRAINT "PK_RouteStops" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RouteStops_TransportRoutes_RouteId" FOREIGN KEY ("RouteId") REFERENCES "TransportRoutes" ("Id") ON DELETE CASCADE
);

CREATE TABLE "StudentTransportAssignments" (
    "Id" uuid NOT NULL,
    "EnrollmentId" uuid NOT NULL,
    "RouteId" uuid NOT NULL,
    "RouteStopId" uuid,
    "EffectiveDate" timestamp with time zone NOT NULL,
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    "CreatedBy" text,
    "UpdatedBy" text,
    CONSTRAINT "PK_StudentTransportAssignments" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_StudentTransportAssignments_RouteStops_RouteStopId" FOREIGN KEY ("RouteStopId") REFERENCES "RouteStops" ("Id"),
    CONSTRAINT "FK_StudentTransportAssignments_TransportRoutes_RouteId" FOREIGN KEY ("RouteId") REFERENCES "TransportRoutes" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_StudentTransportAssignments_enrollments_EnrollmentId" FOREIGN KEY ("EnrollmentId") REFERENCES enrollments (id) ON DELETE CASCADE
);

CREATE INDEX "IX_RouteStops_RouteId" ON "RouteStops" ("RouteId");

CREATE INDEX "IX_StudentTransportAssignments_EnrollmentId" ON "StudentTransportAssignments" ("EnrollmentId");

CREATE INDEX "IX_StudentTransportAssignments_RouteId" ON "StudentTransportAssignments" ("RouteId");

CREATE INDEX "IX_StudentTransportAssignments_RouteStopId" ON "StudentTransportAssignments" ("RouteStopId");

CREATE INDEX "IX_TransportRoutes_VehicleId" ON "TransportRoutes" ("VehicleId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260331124145_AddTransportModule', '10.0.1');

COMMIT;


