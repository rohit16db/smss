using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUtcTimestamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SalaryPayments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now() at time zone 'UTC'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "SalaryPayments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now() at time zone 'UTC'");
        }
    }
}
