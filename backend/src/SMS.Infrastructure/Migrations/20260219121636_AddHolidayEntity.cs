using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHolidayEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "holidays",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    holiday_date = table.Column<DateOnly>(type: "date", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    academic_year = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_holidays", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_holidays_academic_year",
                table: "holidays",
                column: "academic_year");

            migrationBuilder.CreateIndex(
                name: "IX_holidays_academic_year_holiday_date",
                table: "holidays",
                columns: new[] { "academic_year", "holiday_date" });

            migrationBuilder.CreateIndex(
                name: "IX_holidays_holiday_date",
                table: "holidays",
                column: "holiday_date");

            migrationBuilder.CreateIndex(
                name: "IX_holidays_holiday_date_academic_year",
                table: "holidays",
                columns: new[] { "holiday_date", "academic_year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_holidays_type",
                table: "holidays",
                column: "type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "holidays");
        }
    }
}
