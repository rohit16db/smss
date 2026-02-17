using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

#nullable disable

namespace SMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    [Migration("20260113000003")]
    public partial class AddAttendanceManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    table.PrimaryKey("pk_student_attendances", x => x.id);
                    table.CheckConstraint("ck_student_attendances_status", 
                        "status IN ('present', 'absent', 'leave', 'unexcused')");
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
                    table.PrimaryKey("pk_teacher_attendances", x => x.id);
                    table.CheckConstraint("ck_teacher_attendances_status", 
                        "status IN ('present', 'absent', 'leave')");
                    table.ForeignKey(
                        name: "fk_teacher_attendances_teachers_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "teachers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_student_attendances_attendance_date",
                table: "student_attendances",
                column: "attendance_date");

            migrationBuilder.CreateIndex(
                name: "ix_student_attendances_class_id",
                table: "student_attendances",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_attendances_status",
                table: "student_attendances",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_student_attendances_student_id",
                table: "student_attendances",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_attendances_student_id_attendance_date",
                table: "student_attendances",
                columns: new[] { "student_id", "attendance_date" });

            migrationBuilder.CreateIndex(
                name: "ix_student_attendances_student_id_class_id_attendance_date",
                table: "student_attendances",
                columns: new[] { "student_id", "class_id", "attendance_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teacher_attendances_attendance_date",
                table: "teacher_attendances",
                column: "attendance_date");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_attendances_teacher_id",
                table: "teacher_attendances",
                column: "teacher_id");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_attendances_teacher_id_attendance_date",
                table: "teacher_attendances",
                columns: new[] { "teacher_id", "attendance_date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "student_attendances");

            migrationBuilder.DropTable(
                name: "teacher_attendances");
        }
    }
}
