using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixShadowProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_staff_SalaryStructures_SalaryStructureId1",
                table: "staff");

            migrationBuilder.DropForeignKey(
                name: "FK_staff_UserProfiles_UserProfileId1",
                table: "staff");

            migrationBuilder.DropForeignKey(
                name: "FK_staff_assignments_academic_years_AcademicYearId1",
                table: "staff_assignments");

            migrationBuilder.DropIndex(
                name: "IX_staff_assignments_AcademicYearId1",
                table: "staff_assignments");

            migrationBuilder.DropIndex(
                name: "IX_staff_SalaryStructureId1",
                table: "staff");

            migrationBuilder.DropIndex(
                name: "IX_staff_UserProfileId1",
                table: "staff");

            migrationBuilder.DropColumn(
                name: "AcademicYearId1",
                table: "staff_assignments");

            migrationBuilder.DropColumn(
                name: "SalaryStructureId1",
                table: "staff");

            migrationBuilder.DropColumn(
                name: "UserProfileId1",
                table: "staff");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId1",
                table: "staff_assignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SalaryStructureId1",
                table: "staff",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserProfileId1",
                table: "staff",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_staff_assignments_AcademicYearId1",
                table: "staff_assignments",
                column: "AcademicYearId1");

            migrationBuilder.CreateIndex(
                name: "IX_staff_SalaryStructureId1",
                table: "staff",
                column: "SalaryStructureId1");

            migrationBuilder.CreateIndex(
                name: "IX_staff_UserProfileId1",
                table: "staff",
                column: "UserProfileId1");

            migrationBuilder.AddForeignKey(
                name: "FK_staff_SalaryStructures_SalaryStructureId1",
                table: "staff",
                column: "SalaryStructureId1",
                principalTable: "SalaryStructures",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_staff_UserProfiles_UserProfileId1",
                table: "staff",
                column: "UserProfileId1",
                principalTable: "UserProfiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_staff_assignments_academic_years_AcademicYearId1",
                table: "staff_assignments",
                column: "AcademicYearId1",
                principalTable: "academic_years",
                principalColumn: "Id");
        }
    }
}
