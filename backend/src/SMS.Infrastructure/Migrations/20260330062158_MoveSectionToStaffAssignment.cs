using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveSectionToStaffAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("TRUNCATE TABLE timetable_entries;");
            migrationBuilder.Sql("TRUNCATE TABLE staff_assignments CASCADE;");

            migrationBuilder.DropForeignKey(
                name: "FK_timetable_entries_sections_section_id",
                table: "timetable_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_timetable_entries_staff_staff_id",
                table: "timetable_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_timetable_entries_subjects_subject_id",
                table: "timetable_entries");

            migrationBuilder.DropIndex(
                name: "IX_timetable_entries_academic_year_id_time_slot_id_section_id",
                table: "timetable_entries");

            migrationBuilder.DropIndex(
                name: "IX_timetable_entries_academic_year_id_time_slot_id_staff_id",
                table: "timetable_entries");

            migrationBuilder.DropIndex(
                name: "IX_timetable_entries_section_id",
                table: "timetable_entries");

            migrationBuilder.DropIndex(
                name: "IX_timetable_entries_staff_id",
                table: "timetable_entries");

            migrationBuilder.DropIndex(
                name: "IX_staff_assignments_staff_id_class_id_subject_id_removal_date",
                table: "staff_assignments");

            migrationBuilder.DropColumn(
                name: "section_id",
                table: "timetable_entries");

            migrationBuilder.DropColumn(
                name: "staff_id",
                table: "timetable_entries");

            migrationBuilder.RenameColumn(
                name: "subject_id",
                table: "timetable_entries",
                newName: "staff_assignment_id");

            migrationBuilder.RenameIndex(
                name: "IX_timetable_entries_subject_id",
                table: "timetable_entries",
                newName: "IX_timetable_entries_staff_assignment_id");


            migrationBuilder.AddColumn<Guid>(
                name: "section_id",
                table: "staff_assignments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_academic_year_id_time_slot_id_staff_assig~",
                table: "timetable_entries",
                columns: new[] { "academic_year_id", "time_slot_id", "staff_assignment_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staff_assignments_section_id",
                table: "staff_assignments",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_staff_assignments_staff_id_section_id_subject_id_removal_da~",
                table: "staff_assignments",
                columns: new[] { "staff_id", "section_id", "subject_id", "removal_date" },
                unique: true,
                filter: "removal_date IS NULL");


            migrationBuilder.AddForeignKey(
                name: "FK_staff_assignments_classes_class_id",
                table: "staff_assignments",
                column: "class_id",
                principalTable: "classes",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_staff_assignments_sections_section_id",
                table: "staff_assignments",
                column: "section_id",
                principalTable: "sections",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);


            migrationBuilder.AddForeignKey(
                name: "FK_timetable_entries_staff_assignments_staff_assignment_id",
                table: "timetable_entries",
                column: "staff_assignment_id",
                principalTable: "staff_assignments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_staff_assignments_classes_class_id",
                table: "staff_assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_staff_assignments_sections_section_id",
                table: "staff_assignments");


            migrationBuilder.DropForeignKey(
                name: "FK_timetable_entries_staff_assignments_staff_assignment_id",
                table: "timetable_entries");

            migrationBuilder.DropIndex(
                name: "IX_timetable_entries_academic_year_id_time_slot_id_staff_assig~",
                table: "timetable_entries");

            migrationBuilder.DropIndex(
                name: "IX_staff_assignments_section_id",
                table: "staff_assignments");

            migrationBuilder.DropIndex(
                name: "IX_staff_assignments_staff_id_section_id_subject_id_removal_da~",
                table: "staff_assignments");



            migrationBuilder.DropColumn(
                name: "section_id",
                table: "staff_assignments");

            migrationBuilder.RenameColumn(
                name: "staff_assignment_id",
                table: "timetable_entries",
                newName: "subject_id");

            migrationBuilder.RenameIndex(
                name: "IX_timetable_entries_staff_assignment_id",
                table: "timetable_entries",
                newName: "IX_timetable_entries_subject_id");

            migrationBuilder.AddColumn<Guid>(
                name: "section_id",
                table: "timetable_entries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "staff_id",
                table: "timetable_entries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_academic_year_id_time_slot_id_section_id",
                table: "timetable_entries",
                columns: new[] { "academic_year_id", "time_slot_id", "section_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_academic_year_id_time_slot_id_staff_id",
                table: "timetable_entries",
                columns: new[] { "academic_year_id", "time_slot_id", "staff_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_section_id",
                table: "timetable_entries",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_staff_id",
                table: "timetable_entries",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_staff_assignments_staff_id_class_id_subject_id_removal_date",
                table: "staff_assignments",
                columns: new[] { "staff_id", "class_id", "subject_id", "removal_date" },
                unique: true,
                filter: "removal_date IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_timetable_entries_sections_section_id",
                table: "timetable_entries",
                column: "section_id",
                principalTable: "sections",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_timetable_entries_staff_staff_id",
                table: "timetable_entries",
                column: "staff_id",
                principalTable: "staff",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_timetable_entries_subjects_subject_id",
                table: "timetable_entries",
                column: "subject_id",
                principalTable: "subjects",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
