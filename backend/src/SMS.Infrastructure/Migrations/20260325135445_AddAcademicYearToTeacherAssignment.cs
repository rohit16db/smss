using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicYearToTeacherAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_exams_academic_years_AcademicYearId",
                table: "exams");

            migrationBuilder.DropForeignKey(
                name: "FK_teacher_assignments_teachers_teacher_id",
                table: "teacher_assignments");

            migrationBuilder.RenameColumn(
                name: "AcademicYearId",
                table: "exams",
                newName: "academic_year_id");

            migrationBuilder.RenameIndex(
                name: "IX_exams_AcademicYearId",
                table: "exams",
                newName: "IX_exams_academic_year_id");

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId",
                table: "teacher_assignments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Backfill existing assignments with the active academic year
            migrationBuilder.Sql("UPDATE teacher_assignments SET \"AcademicYearId\" = (SELECT \"Id\" FROM academic_years WHERE is_active = true LIMIT 1)");

            migrationBuilder.CreateIndex(
                name: "IX_teacher_assignments_AcademicYearId",
                table: "teacher_assignments",
                column: "AcademicYearId");

            migrationBuilder.AddForeignKey(
                name: "FK_exams_academic_years_academic_year_id",
                table: "exams",
                column: "academic_year_id",
                principalTable: "academic_years",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_teacher_assignments_academic_years_AcademicYearId",
                table: "teacher_assignments",
                column: "AcademicYearId",
                principalTable: "academic_years",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_teacher_assignments_teachers_teacher_id",
                table: "teacher_assignments",
                column: "teacher_id",
                principalTable: "teachers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_exams_academic_years_academic_year_id",
                table: "exams");

            migrationBuilder.DropForeignKey(
                name: "FK_teacher_assignments_academic_years_AcademicYearId",
                table: "teacher_assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_teacher_assignments_teachers_teacher_id",
                table: "teacher_assignments");

            migrationBuilder.DropIndex(
                name: "IX_teacher_assignments_AcademicYearId",
                table: "teacher_assignments");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "teacher_assignments");

            migrationBuilder.RenameColumn(
                name: "academic_year_id",
                table: "exams",
                newName: "AcademicYearId");

            migrationBuilder.RenameIndex(
                name: "IX_exams_academic_year_id",
                table: "exams",
                newName: "IX_exams_AcademicYearId");

            migrationBuilder.AddForeignKey(
                name: "FK_exams_academic_years_AcademicYearId",
                table: "exams",
                column: "AcademicYearId",
                principalTable: "academic_years",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_teacher_assignments_teachers_teacher_id",
                table: "teacher_assignments",
                column: "teacher_id",
                principalTable: "teachers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
