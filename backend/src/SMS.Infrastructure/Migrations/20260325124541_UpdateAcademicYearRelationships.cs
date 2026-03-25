using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAcademicYearRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_holidays_academic_year",
                table: "holidays");

            migrationBuilder.DropIndex(
                name: "IX_holidays_academic_year_holiday_date",
                table: "holidays");

            migrationBuilder.DropIndex(
                name: "IX_holidays_holiday_date_academic_year",
                table: "holidays");

            migrationBuilder.DropIndex(
                name: "IX_fee_structures_academic_year",
                table: "fee_structures");

            migrationBuilder.DropColumn(
                name: "academic_year",
                table: "holidays");

            migrationBuilder.DropColumn(
                name: "academic_year",
                table: "fee_structures");

            migrationBuilder.AddColumn<Guid>(
                name: "academic_year_id",
                table: "holidays",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "academic_year_id",
                table: "fee_structures",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_holidays_academic_year_id",
                table: "holidays",
                column: "academic_year_id");

            migrationBuilder.CreateIndex(
                name: "IX_holidays_academic_year_id_holiday_date",
                table: "holidays",
                columns: new[] { "academic_year_id", "holiday_date" });

            migrationBuilder.CreateIndex(
                name: "IX_holidays_holiday_date_academic_year_id",
                table: "holidays",
                columns: new[] { "holiday_date", "academic_year_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fee_structures_academic_year_id",
                table: "fee_structures",
                column: "academic_year_id");

            migrationBuilder.AddForeignKey(
                name: "FK_fee_structures_academic_years_academic_year_id",
                table: "fee_structures",
                column: "academic_year_id",
                principalTable: "academic_years",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_holidays_academic_years_academic_year_id",
                table: "holidays",
                column: "academic_year_id",
                principalTable: "academic_years",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_fee_structures_academic_years_academic_year_id",
                table: "fee_structures");

            migrationBuilder.DropForeignKey(
                name: "FK_holidays_academic_years_academic_year_id",
                table: "holidays");

            migrationBuilder.DropIndex(
                name: "IX_holidays_academic_year_id",
                table: "holidays");

            migrationBuilder.DropIndex(
                name: "IX_holidays_academic_year_id_holiday_date",
                table: "holidays");

            migrationBuilder.DropIndex(
                name: "IX_holidays_holiday_date_academic_year_id",
                table: "holidays");

            migrationBuilder.DropIndex(
                name: "IX_fee_structures_academic_year_id",
                table: "fee_structures");

            migrationBuilder.DropColumn(
                name: "academic_year_id",
                table: "holidays");

            migrationBuilder.DropColumn(
                name: "academic_year_id",
                table: "fee_structures");

            migrationBuilder.AddColumn<string>(
                name: "academic_year",
                table: "holidays",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "academic_year",
                table: "fee_structures",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_holidays_academic_year",
                table: "holidays",
                column: "academic_year");

            migrationBuilder.CreateIndex(
                name: "IX_holidays_academic_year_holiday_date",
                table: "holidays",
                columns: new[] { "academic_year", "holiday_date" });

            migrationBuilder.CreateIndex(
                name: "IX_holidays_holiday_date_academic_year",
                table: "holidays",
                columns: new[] { "holiday_date", "academic_year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fee_structures_academic_year",
                table: "fee_structures",
                column: "academic_year");
        }
    }
}
