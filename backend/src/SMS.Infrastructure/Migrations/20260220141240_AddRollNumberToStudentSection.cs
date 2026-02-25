using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRollNumberToStudentSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "roll_number",
                table: "student_sections",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "UK_StudentSection_RollNumber",
                table: "student_sections",
                columns: new[] { "section_id", "roll_number" },
                unique: true,
                filter: "\"roll_number\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UK_StudentSection_RollNumber",
                table: "student_sections");

            migrationBuilder.DropColumn(
                name: "roll_number",
                table: "student_sections");
        }
    }
}
