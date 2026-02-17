using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

#nullable disable

namespace SMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    [Migration("20260113000001")]
    public partial class AddTeacherManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    table.PrimaryKey("pk_teachers", x => x.id);
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
                    table.PrimaryKey("pk_teacher_assignments", x => x.id);
                    table.ForeignKey(
                        name: "fk_teacher_assignments_teachers_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "teachers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_teacher_assignments_class_id",
                table: "teacher_assignments",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_assignments_removal_date",
                table: "teacher_assignments",
                column: "removal_date");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_assignments_subject_id",
                table: "teacher_assignments",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_assignments_teacher_id",
                table: "teacher_assignments",
                column: "teacher_id");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_assignments_teacher_id_class_id_subject_id_removal_date",
                table: "teacher_assignments",
                columns: new[] { "teacher_id", "class_id", "subject_id", "removal_date" },
                unique: true,
                filter: "removal_date IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_teachers_email",
                table: "teachers",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teachers_is_active",
                table: "teachers",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_teachers_joining_date",
                table: "teachers",
                column: "joining_date");

            migrationBuilder.CreateIndex(
                name: "ix_teachers_user_id",
                table: "teachers",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "teacher_assignments");

            migrationBuilder.DropTable(
                name: "teachers");
        }
    }
}
