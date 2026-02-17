using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClassesAndSections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "classes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    academic_year = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_classes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sections", x => x.id);
                    table.ForeignKey(
                        name: "FK_sections_classes_class_id",
                        column: x => x.class_id,
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_sections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    joined_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    left_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_sections", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_sections_sections_section_id",
                        column: x => x.section_id,
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_student_sections_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_classes_is_active",
                table: "classes",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_classes_name_academic_year",
                table: "classes",
                columns: new[] { "name", "academic_year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sections_class_id",
                table: "sections",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_sections_class_id_section_name",
                table: "sections",
                columns: new[] { "class_id", "section_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sections_is_active",
                table: "sections",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_student_sections_is_current",
                table: "student_sections",
                column: "is_current");

            migrationBuilder.CreateIndex(
                name: "IX_student_sections_section_id",
                table: "student_sections",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_sections_student_id",
                table: "student_sections",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_sections_student_id_is_current",
                table: "student_sections",
                columns: new[] { "student_id", "is_current" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "student_sections");

            migrationBuilder.DropTable(
                name: "sections");

            migrationBuilder.DropTable(
                name: "classes");
        }
    }
}
