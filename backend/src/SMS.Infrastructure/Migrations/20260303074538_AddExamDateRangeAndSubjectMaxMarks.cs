using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExamDateRangeAndSubjectMaxMarks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "exam_date",
                table: "exams",
                newName: "start_date");

            migrationBuilder.RenameIndex(
                name: "IX_exams_name_exam_date",
                table: "exams",
                newName: "IX_exams_name_start_date");

            migrationBuilder.RenameIndex(
                name: "IX_exams_exam_date",
                table: "exams",
                newName: "IX_exams_start_date");

            migrationBuilder.AddColumn<DateTime>(
                name: "end_date",
                table: "exams",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_exams_end_date",
                table: "exams",
                column: "end_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_exams_end_date",
                table: "exams");

            migrationBuilder.DropColumn(
                name: "end_date",
                table: "exams");

            migrationBuilder.RenameColumn(
                name: "start_date",
                table: "exams",
                newName: "exam_date");

            migrationBuilder.RenameIndex(
                name: "IX_exams_start_date",
                table: "exams",
                newName: "IX_exams_exam_date");

            migrationBuilder.RenameIndex(
                name: "IX_exams_name_start_date",
                table: "exams",
                newName: "IX_exams_name_exam_date");
        }
    }
}
