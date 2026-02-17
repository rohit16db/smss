using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSalaryStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "SalaryStructureEffectiveDate",
                table: "teachers",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SalaryStructureId",
                table: "teachers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SalaryStructures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BaseSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    HRA = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DA = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    MedicalAllowance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ConveyanceAllowance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    OtherAllowances = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    StandardDeduction = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    MinExperienceYears = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ApplicableQualifications = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EffectiveFromDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveToDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now() at time zone 'UTC'"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalaryStructures", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_teachers_SalaryStructureId",
                table: "teachers",
                column: "SalaryStructureId");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryStructures_EffectiveFromDate",
                table: "SalaryStructures",
                column: "EffectiveFromDate");

            migrationBuilder.CreateIndex(
                name: "IX_SalaryStructures_IsActive",
                table: "SalaryStructures",
                column: "IsActive");

            migrationBuilder.AddForeignKey(
                name: "FK_teachers_SalaryStructures_SalaryStructureId",
                table: "teachers",
                column: "SalaryStructureId",
                principalTable: "SalaryStructures",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_teachers_SalaryStructures_SalaryStructureId",
                table: "teachers");

            migrationBuilder.DropTable(
                name: "SalaryStructures");

            migrationBuilder.DropIndex(
                name: "IX_teachers_SalaryStructureId",
                table: "teachers");

            migrationBuilder.DropColumn(
                name: "SalaryStructureEffectiveDate",
                table: "teachers");

            migrationBuilder.DropColumn(
                name: "SalaryStructureId",
                table: "teachers");
        }
    }
}
