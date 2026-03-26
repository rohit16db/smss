using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTimetableEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "time_slots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_break = table.Column<bool>(type: "boolean", nullable: false),
                    academic_year_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_time_slots", x => x.id);
                    table.ForeignKey(
                        name: "FK_time_slots_academic_years_academic_year_id",
                        column: x => x.academic_year_id,
                        principalTable: "academic_years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "timetable_entries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    time_slot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    academic_year_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    updated_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timetable_entries", x => x.id);
                    table.ForeignKey(
                        name: "FK_timetable_entries_academic_years_academic_year_id",
                        column: x => x.academic_year_id,
                        principalTable: "academic_years",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_timetable_entries_sections_section_id",
                        column: x => x.section_id,
                        principalTable: "sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_timetable_entries_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_timetable_entries_teachers_teacher_id",
                        column: x => x.teacher_id,
                        principalTable: "teachers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_timetable_entries_time_slots_time_slot_id",
                        column: x => x.time_slot_id,
                        principalTable: "time_slots",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_time_slots_academic_year_id",
                table: "time_slots",
                column: "academic_year_id");

            migrationBuilder.CreateIndex(
                name: "IX_time_slots_academic_year_id_day_of_week_start_time_end_time",
                table: "time_slots",
                columns: new[] { "academic_year_id", "day_of_week", "start_time", "end_time" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_academic_year_id",
                table: "timetable_entries",
                column: "academic_year_id");

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_academic_year_id_time_slot_id_section_id",
                table: "timetable_entries",
                columns: new[] { "academic_year_id", "time_slot_id", "section_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_academic_year_id_time_slot_id_teacher_id",
                table: "timetable_entries",
                columns: new[] { "academic_year_id", "time_slot_id", "teacher_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_section_id",
                table: "timetable_entries",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_subject_id",
                table: "timetable_entries",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_teacher_id",
                table: "timetable_entries",
                column: "teacher_id");

            migrationBuilder.CreateIndex(
                name: "IX_timetable_entries_time_slot_id",
                table: "timetable_entries",
                column: "time_slot_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "timetable_entries");

            migrationBuilder.DropTable(
                name: "time_slots");
        }
    }
}
