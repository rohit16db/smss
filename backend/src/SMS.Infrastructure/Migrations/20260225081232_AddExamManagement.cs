using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExamManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exams",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    exam_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    total_marks = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    pass_marks = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exams", x => x.id);
                    table.ForeignKey(
                        name: "FK_exams_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "grade_configuration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    grade_name = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    min_percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    max_percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    school_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grade_configuration", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exam_classes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false),
                    class_id = table.Column<Guid>(type: "uuid", nullable: false),
                    marks_entry_status = table.Column<string>(type: "text", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    submitted_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_classes", x => x.id);
                    table.ForeignKey(
                        name: "FK_exam_classes_classes_class_id",
                        column: x => x.class_id,
                        principalTable: "classes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_exam_classes_exams_exam_id",
                        column: x => x.exam_id,
                        principalTable: "exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exam_classes_users_submitted_by",
                        column: x => x.submitted_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "exam_subjects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    max_marks = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    pass_marks = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_subjects", x => x.id);
                    table.UniqueConstraint("AK_exam_subjects_exam_id_subject_id", x => new { x.exam_id, x.subject_id });
                    table.ForeignKey(
                        name: "FK_exam_subjects_exams_exam_id",
                        column: x => x.exam_id,
                        principalTable: "exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_exam_subjects_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "student_report_cards",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_marks_obtained = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    total_marks = table.Column<decimal>(type: "numeric(7,2)", precision: 7, scale: 2, nullable: false),
                    percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    overall_grade = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    class_position = table.Column<int>(type: "integer", nullable: false),
                    pass = table.Column<bool>(type: "boolean", nullable: false),
                    remarks = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    generated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_report_cards", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_report_cards_exams_exam_id",
                        column: x => x.exam_id,
                        principalTable: "exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_student_report_cards_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_marks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_id = table.Column<Guid>(type: "uuid", nullable: false),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_id = table.Column<Guid>(type: "uuid", nullable: false),
                    marks_obtained = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    is_absent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    remarks = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_marks", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_marks_exam_subjects_exam_id_subject_id",
                        columns: x => new { x.exam_id, x.subject_id },
                        principalTable: "exam_subjects",
                        principalColumns: new[] { "exam_id", "subject_id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_student_marks_exams_exam_id",
                        column: x => x.exam_id,
                        principalTable: "exams",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_student_marks_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exam_classes_class_id",
                table: "exam_classes",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_exam_classes_exam_id_class_id",
                table: "exam_classes",
                columns: new[] { "exam_id", "class_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_classes_marks_entry_status",
                table: "exam_classes",
                column: "marks_entry_status");

            migrationBuilder.CreateIndex(
                name: "IX_exam_classes_submitted_by",
                table: "exam_classes",
                column: "submitted_by");

            migrationBuilder.CreateIndex(
                name: "IX_exam_subjects_exam_id_subject_id",
                table: "exam_subjects",
                columns: new[] { "exam_id", "subject_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exam_subjects_subject_id",
                table: "exam_subjects",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_exams_created_by",
                table: "exams",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_exams_exam_date",
                table: "exams",
                column: "exam_date");

            migrationBuilder.CreateIndex(
                name: "IX_exams_name_exam_date",
                table: "exams",
                columns: new[] { "name", "exam_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exams_status",
                table: "exams",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_grade_configuration_school_id",
                table: "grade_configuration",
                column: "school_id");

            migrationBuilder.CreateIndex(
                name: "IX_grade_configuration_school_id_grade_name",
                table: "grade_configuration",
                columns: new[] { "school_id", "grade_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_marks_exam_id",
                table: "student_marks",
                column: "exam_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_marks_exam_id_student_id_subject_id",
                table: "student_marks",
                columns: new[] { "exam_id", "student_id", "subject_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_marks_exam_id_subject_id",
                table: "student_marks",
                columns: new[] { "exam_id", "subject_id" });

            migrationBuilder.CreateIndex(
                name: "IX_student_marks_student_id",
                table: "student_marks",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_report_cards_exam_id",
                table: "student_report_cards",
                column: "exam_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_report_cards_exam_id_student_id",
                table: "student_report_cards",
                columns: new[] { "exam_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_report_cards_pass",
                table: "student_report_cards",
                column: "pass");

            migrationBuilder.CreateIndex(
                name: "IX_student_report_cards_student_id",
                table: "student_report_cards",
                column: "student_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "exam_classes");

            migrationBuilder.DropTable(
                name: "grade_configuration");

            migrationBuilder.DropTable(
                name: "student_marks");

            migrationBuilder.DropTable(
                name: "student_report_cards");

            migrationBuilder.DropTable(
                name: "exam_subjects");

            migrationBuilder.DropTable(
                name: "exams");
        }
    }
}
