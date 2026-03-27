using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SynchronizeStaffAndFixMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalaryPayments_teachers_TeacherId",
                table: "SalaryPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_timetable_entries_teachers_teacher_id",
                table: "timetable_entries");

            migrationBuilder.DropTable(
                name: "teacher_assignments");

            migrationBuilder.DropTable(
                name: "teacher_attendances");

            migrationBuilder.DropTable(
                name: "teachers");

            migrationBuilder.RenameColumn(
                name: "teacher_id",
                table: "timetable_entries",
                newName: "staff_id");

            migrationBuilder.RenameIndex(
                name: "IX_timetable_entries_teacher_id",
                table: "timetable_entries",
                newName: "IX_timetable_entries_staff_id");

            migrationBuilder.RenameIndex(
                name: "IX_timetable_entries_academic_year_id_time_slot_id_teacher_id",
                table: "timetable_entries",
                newName: "IX_timetable_entries_academic_year_id_time_slot_id_staff_id");

            migrationBuilder.RenameColumn(
                name: "TeacherId",
                table: "SalaryPayments",
                newName: "StaffId");

            migrationBuilder.RenameIndex(
                name: "IX_SalaryPayments_TeacherId",
                table: "SalaryPayments",
                newName: "IX_SalaryPayments_StaffId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SalaryStructures",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "exams",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    BloodGroup = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    CurrentAddress = table.Column<string>(type: "text", nullable: true),
                    PermanentAddress = table.Column<string>(type: "text", nullable: true),
                    EmergencyContactName = table.Column<string>(type: "text", nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "text", nullable: true),
                    ImagePath = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    HeadOfDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    HeadOfDepartmentId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "staff",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    department_id = table.Column<Guid>(type: "uuid", nullable: false),
                    designation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    role_type = table.Column<int>(type: "integer", nullable: false),
                    experience_years = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    joining_date = table.Column<DateOnly>(type: "date", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    basic_salary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    salary_structure_id = table.Column<Guid>(type: "uuid", nullable: true),
                    salary_structure_effective_date = table.Column<DateOnly>(type: "date", nullable: true),
                    DepartmentId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    SalaryStructureId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    UserProfileId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff", x => x.id);
                    table.ForeignKey(
                        name: "FK_staff_Departments_DepartmentId1",
                        column: x => x.DepartmentId1,
                        principalTable: "Departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_staff_Departments_department_id",
                        column: x => x.department_id,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_staff_SalaryStructures_SalaryStructureId1",
                        column: x => x.SalaryStructureId1,
                        principalTable: "SalaryStructures",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_staff_SalaryStructures_salary_structure_id",
                        column: x => x.salary_structure_id,
                        principalTable: "SalaryStructures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_staff_UserProfiles_UserProfileId1",
                        column: x => x.UserProfileId1,
                        principalTable: "UserProfiles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_staff_UserProfiles_user_profile_id",
                        column: x => x.user_profile_id,
                        principalTable: "UserProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EducationalQualifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    DegreeName = table.Column<string>(type: "text", nullable: false),
                    Institution = table.Column<string>(type: "text", nullable: false),
                    YearOfPassing = table.Column<int>(type: "integer", nullable: false),
                    GradeOrPercentage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationalQualifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EducationalQualifications_staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "staff",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "staff_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    academic_year_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_date = table.Column<DateOnly>(type: "date", nullable: false),
                    removal_date = table.Column<DateOnly>(type: "date", nullable: true),
                    AcademicYearId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_staff_assignments_academic_years_AcademicYearId1",
                        column: x => x.AcademicYearId1,
                        principalTable: "academic_years",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_staff_assignments_academic_years_academic_year_id",
                        column: x => x.academic_year_id,
                        principalTable: "academic_years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_staff_assignments_staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_staff_assignments_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "staff_attendances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    staff_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_staff_attendances", x => x.id);
                    table.CheckConstraint("ck_staff_attendances_status", "status IN ('present', 'absent', 'leave')");
                    table.ForeignKey(
                        name: "FK_staff_attendances_staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_HeadOfDepartmentId1",
                table: "Departments",
                column: "HeadOfDepartmentId1");

            migrationBuilder.CreateIndex(
                name: "IX_EducationalQualifications_StaffId",
                table: "EducationalQualifications",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_staff_department_id",
                table: "staff",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_staff_DepartmentId1",
                table: "staff",
                column: "DepartmentId1");

            migrationBuilder.CreateIndex(
                name: "IX_staff_is_active",
                table: "staff",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_staff_joining_date",
                table: "staff",
                column: "joining_date");

            migrationBuilder.CreateIndex(
                name: "IX_staff_salary_structure_id",
                table: "staff",
                column: "salary_structure_id");

            migrationBuilder.CreateIndex(
                name: "IX_staff_SalaryStructureId1",
                table: "staff",
                column: "SalaryStructureId1");

            migrationBuilder.CreateIndex(
                name: "IX_staff_user_profile_id",
                table: "staff",
                column: "user_profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_staff_UserProfileId1",
                table: "staff",
                column: "UserProfileId1");

            migrationBuilder.CreateIndex(
                name: "IX_staff_assignments_academic_year_id",
                table: "staff_assignments",
                column: "academic_year_id");

            migrationBuilder.CreateIndex(
                name: "IX_staff_assignments_AcademicYearId1",
                table: "staff_assignments",
                column: "AcademicYearId1");

            migrationBuilder.CreateIndex(
                name: "IX_staff_assignments_class_id",
                table: "staff_assignments",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_staff_assignments_removal_date",
                table: "staff_assignments",
                column: "removal_date");

            migrationBuilder.CreateIndex(
                name: "IX_staff_assignments_staff_id",
                table: "staff_assignments",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_staff_assignments_staff_id_class_id_subject_id_removal_date",
                table: "staff_assignments",
                columns: new[] { "staff_id", "class_id", "subject_id", "removal_date" },
                unique: true,
                filter: "removal_date IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_staff_assignments_subject_id",
                table: "staff_assignments",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_staff_attendances_attendance_date",
                table: "staff_attendances",
                column: "attendance_date");

            migrationBuilder.CreateIndex(
                name: "IX_staff_attendances_staff_id",
                table: "staff_attendances",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_staff_attendances_staff_id_attendance_date",
                table: "staff_attendances",
                columns: new[] { "staff_id", "attendance_date" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryPayments_staff_StaffId",
                table: "SalaryPayments",
                column: "StaffId",
                principalTable: "staff",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_timetable_entries_staff_staff_id",
                table: "timetable_entries",
                column: "staff_id",
                principalTable: "staff",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_staff_HeadOfDepartmentId1",
                table: "Departments",
                column: "HeadOfDepartmentId1",
                principalTable: "staff",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SalaryPayments_staff_StaffId",
                table: "SalaryPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_timetable_entries_staff_staff_id",
                table: "timetable_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_Departments_staff_HeadOfDepartmentId1",
                table: "Departments");

            migrationBuilder.DropTable(
                name: "EducationalQualifications");

            migrationBuilder.DropTable(
                name: "staff_assignments");

            migrationBuilder.DropTable(
                name: "staff_attendances");

            migrationBuilder.DropTable(
                name: "staff");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "exams");

            migrationBuilder.RenameColumn(
                name: "staff_id",
                table: "timetable_entries",
                newName: "teacher_id");

            migrationBuilder.RenameIndex(
                name: "IX_timetable_entries_staff_id",
                table: "timetable_entries",
                newName: "IX_timetable_entries_teacher_id");

            migrationBuilder.RenameIndex(
                name: "IX_timetable_entries_academic_year_id_time_slot_id_staff_id",
                table: "timetable_entries",
                newName: "IX_timetable_entries_academic_year_id_time_slot_id_teacher_id");

            migrationBuilder.RenameColumn(
                name: "StaffId",
                table: "SalaryPayments",
                newName: "TeacherId");

            migrationBuilder.RenameIndex(
                name: "IX_SalaryPayments_StaffId",
                table: "SalaryPayments",
                newName: "IX_SalaryPayments_TeacherId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "SalaryStructures",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateTable(
                name: "teachers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    SalaryStructureId = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    experience_years = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    first_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ImagePath = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    joining_date = table.Column<DateOnly>(type: "date", nullable: false),
                    last_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    qualification = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SalaryStructureEffectiveDate = table.Column<DateOnly>(type: "date", nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teachers", x => x.id);
                    table.ForeignKey(
                        name: "FK_teachers_SalaryStructures_SalaryStructureId",
                        column: x => x.SalaryStructureId,
                        principalTable: "SalaryStructures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "teacher_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    AcademicYearId = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_date = table.Column<DateOnly>(type: "date", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    removal_date = table.Column<DateOnly>(type: "date", nullable: true),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_teacher_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_teacher_assignments_academic_years_AcademicYearId",
                        column: x => x.AcademicYearId,
                        principalTable: "academic_years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teacher_assignments_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_teacher_assignments_teachers_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "teachers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "teacher_attendances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attendance_date = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    recorded_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_teacher_assignments_AcademicYearId",
                table: "teacher_assignments",
                column: "AcademicYearId");

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
                name: "IX_teachers_SalaryStructureId",
                table: "teachers",
                column: "SalaryStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_teachers_user_id",
                table: "teachers",
                column: "user_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SalaryPayments_teachers_TeacherId",
                table: "SalaryPayments",
                column: "TeacherId",
                principalTable: "teachers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_timetable_entries_teachers_teacher_id",
                table: "timetable_entries",
                column: "teacher_id",
                principalTable: "teachers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
