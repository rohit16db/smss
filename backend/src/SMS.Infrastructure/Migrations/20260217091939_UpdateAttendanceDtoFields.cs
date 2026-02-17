using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAttendanceDtoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_student_fees_date_range",
                table: "student_fees");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "end_date",
                table: "student_fees",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddCheckConstraint(
                name: "ck_student_fees_date_range",
                table: "student_fees",
                sql: "end_date IS NULL OR start_date <= end_date");

            migrationBuilder.AddForeignKey(
                name: "FK_student_fees_students_student_id",
                table: "student_fees",
                column: "student_id",
                principalTable: "students",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_student_fees_students_student_id",
                table: "student_fees");

            migrationBuilder.DropCheckConstraint(
                name: "ck_student_fees_date_range",
                table: "student_fees");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "end_date",
                table: "student_fees",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_student_fees_date_range",
                table: "student_fees",
                sql: "start_date <= end_date");
        }
    }
}
