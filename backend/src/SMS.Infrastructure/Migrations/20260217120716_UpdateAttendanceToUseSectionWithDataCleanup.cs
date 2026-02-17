using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttendanceToUseSectionWithDataCleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First, delete any attendance records with invalid/empty class_ids (before renaming)
            migrationBuilder.Sql(
                "DELETE FROM student_attendances WHERE class_id = '00000000-0000-0000-0000-000000000000' OR class_id IS NULL;");

            migrationBuilder.RenameColumn(
                name: "class_id",
                table: "student_attendances",
                newName: "section_id");

            migrationBuilder.RenameIndex(
                name: "IX_student_attendances_student_id_class_id_attendance_date",
                table: "student_attendances",
                newName: "IX_student_attendances_student_id_section_id_attendance_date");

            migrationBuilder.RenameIndex(
                name: "IX_student_attendances_class_id",
                table: "student_attendances",
                newName: "IX_student_attendances_section_id");

            migrationBuilder.AddForeignKey(
                name: "FK_student_attendances_sections_section_id",
                table: "student_attendances",
                column: "section_id",
                principalTable: "sections",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_student_attendances_sections_section_id",
                table: "student_attendances");

            migrationBuilder.RenameColumn(
                name: "section_id",
                table: "student_attendances",
                newName: "class_id");

            migrationBuilder.RenameIndex(
                name: "IX_student_attendances_student_id_section_id_attendance_date",
                table: "student_attendances",
                newName: "IX_student_attendances_student_id_class_id_attendance_date");

            migrationBuilder.RenameIndex(
                name: "IX_student_attendances_section_id",
                table: "student_attendances",
                newName: "IX_student_attendances_class_id");
        }
    }
}
