using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fee_structures",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    academic_year = table.Column<int>(type: "integer", nullable: false),
                    frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fee_structures", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "student_attendances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attendance_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    marked_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    marked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_attendances", x => x.id);
                    table.CheckConstraint("ck_student_attendances_status", "status IN ('present', 'absent', 'leave', 'unexcused')");
                });

            migrationBuilder.CreateTable(
                name: "teachers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    last_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    qualification = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    experience_years = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    joining_date = table.Column<DateOnly>(type: "date", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teachers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fee_structure_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    fee_structure_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fee_structure_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_fee_structure_categories_fee_structures_fee_structure_id",
                        column: x => x.fee_structure_id,
                        principalTable: "fee_structures",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_fees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fee_structure_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_fees", x => x.id);
                    table.CheckConstraint("ck_student_fees_date_range", "start_date <= end_date");
                    table.ForeignKey(
                        name: "FK_student_fees_fee_structures_fee_structure_id",
                        column: x => x.fee_structure_id,
                        principalTable: "fee_structures",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SalaryPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    TeacherId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodStartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PeriodEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Deductions = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Bonus = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    NetSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    PaidDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PaymentMethod = table.Column<string>(type: "text", nullable: true),
                    Remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalaryPayments_teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "teachers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "teacher_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_date = table.Column<DateOnly>(type: "date", nullable: false),
                    removal_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teacher_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_teacher_assignments_teachers_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "teachers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "teacher_attendances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attendance_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    recorded_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teacher_attendances", x => x.id);
                    table.CheckConstraint("ck_teacher_attendances_status", "status IN ('present', 'absent', 'leave')");
                    table.ForeignKey(
                        name: "FK_teacher_attendances_teachers_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "teachers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fee_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    student_fee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    payment_date = table.Column<DateOnly>(type: "date", nullable: false),
                    receipt_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    payment_method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fee_payments", x => x.id);
                    table.CheckConstraint("ck_fee_payments_amount_positive", "amount_paid > 0");
                    table.ForeignKey(
                        name: "FK_fee_payments_student_fees_student_fee_id",
                        column: x => x.student_fee_id,
                        principalTable: "student_fees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_fee_payments_payment_date",
                table: "fee_payments",
                column: "payment_date");

            migrationBuilder.CreateIndex(
                name: "IX_fee_payments_receipt_number",
                table: "fee_payments",
                column: "receipt_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fee_payments_student_fee_id",
                table: "fee_payments",
                column: "student_fee_id");

            migrationBuilder.CreateIndex(
                name: "IX_fee_structure_categories_fee_structure_id",
                table: "fee_structure_categories",
                column: "fee_structure_id");

            migrationBuilder.CreateIndex(
                name: "IX_fee_structure_categories_fee_structure_id_category",
                table: "fee_structure_categories",
                columns: new[] { "fee_structure_id", "category" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fee_structures_academic_year",
                table: "fee_structures",
                column: "academic_year");

            migrationBuilder.CreateIndex(
                name: "IX_fee_structures_is_active",
                table: "fee_structures",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryPayments_CreatedAt",
                table: "SalaryPayments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryPayments_PeriodStartDate_PeriodEndDate",
                table: "SalaryPayments",
                columns: new[] { "PeriodStartDate", "PeriodEndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_SalaryPayments_Status",
                table: "SalaryPayments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryPayments_TeacherId",
                table: "SalaryPayments",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_student_attendances_attendance_date",
                table: "student_attendances",
                column: "attendance_date");

            migrationBuilder.CreateIndex(
                name: "IX_student_attendances_class_id",
                table: "student_attendances",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_attendances_status",
                table: "student_attendances",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_student_attendances_student_id",
                table: "student_attendances",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_attendances_student_id_attendance_date",
                table: "student_attendances",
                columns: new[] { "student_id", "attendance_date" })
                .Annotation("Npgsql:IndexInclude", new[] { "status" });

            migrationBuilder.CreateIndex(
                name: "IX_student_attendances_student_id_class_id_attendance_date",
                table: "student_attendances",
                columns: new[] { "student_id", "class_id", "attendance_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_fees_fee_structure_id",
                table: "student_fees",
                column: "fee_structure_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_fees_start_date_end_date",
                table: "student_fees",
                columns: new[] { "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "IX_student_fees_student_id",
                table: "student_fees",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_assignments_class_id",
                table: "teacher_assignments",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_assignments_removal_date",
                table: "teacher_assignments",
                column: "removal_date");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_assignments_subject_id",
                table: "teacher_assignments",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_assignments_teacher_id",
                table: "teacher_assignments",
                column: "teacher_id");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_assignments_teacher_id_class_id_subject_id_removal_~",
                table: "teacher_assignments",
                columns: new[] { "teacher_id", "class_id", "subject_id", "removal_date" },
                unique: true,
                filter: "removal_date IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_attendances_attendance_date",
                table: "teacher_attendances",
                column: "attendance_date");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_attendances_teacher_id",
                table: "teacher_attendances",
                column: "teacher_id");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_attendances_teacher_id_attendance_date",
                table: "teacher_attendances",
                columns: new[] { "teacher_id", "attendance_date" },
                unique: true)
                .Annotation("Npgsql:IndexInclude", new[] { "status" });

            migrationBuilder.CreateIndex(
                name: "IX_teachers_email",
                table: "teachers",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_teachers_is_active",
                table: "teachers",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_teachers_joining_date",
                table: "teachers",
                column: "joining_date");

            migrationBuilder.CreateIndex(
                name: "IX_teachers_user_id",
                table: "teachers",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fee_payments");

            migrationBuilder.DropTable(
                name: "fee_structure_categories");

            migrationBuilder.DropTable(
                name: "SalaryPayments");

            migrationBuilder.DropTable(
                name: "student_attendances");

            migrationBuilder.DropTable(
                name: "teacher_assignments");

            migrationBuilder.DropTable(
                name: "teacher_attendances");

            migrationBuilder.DropTable(
                name: "student_fees");

            migrationBuilder.DropTable(
                name: "teachers");

            migrationBuilder.DropTable(
                name: "fee_structures");
        }
    }
}
