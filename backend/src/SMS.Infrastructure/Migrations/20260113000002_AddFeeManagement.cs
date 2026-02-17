using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

#nullable disable

namespace SMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    [Migration("20260113000002")]
    public partial class AddFeeManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fee_structures",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    academic_year = table.Column<int>(type: "integer", nullable: false),
                    frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fee_structures", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fee_structure_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    fee_structure_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fee_structure_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_fee_structure_categories_fee_structures_fee_structure_id",
                        column: x => x.fee_structure_id,
                        principalTable: "fee_structures",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_fees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    student_id = table.Column<Guid>(type: "uuid", nullable: false),
                    fee_structure_id = table.Column<Guid>(type: "uuid", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    total_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_student_fees", x => x.id);
                    table.CheckConstraint("ck_student_fees_date_range", "start_date <= end_date");
                    table.ForeignKey(
                        name: "fk_student_fees_fee_structures_fee_structure_id",
                        column: x => x.fee_structure_id,
                        principalTable: "fee_structures",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "fee_payments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    student_fee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount_paid = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    payment_date = table.Column<DateOnly>(type: "date", nullable: false),
                    receipt_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    payment_method = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_fee_payments", x => x.id);
                    table.CheckConstraint("ck_fee_payments_amount_positive", "amount_paid > 0");
                    table.ForeignKey(
                        name: "fk_fee_payments_student_fees_student_fee_id",
                        column: x => x.student_fee_id,
                        principalTable: "student_fees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_fee_structure_categories_fee_structure_id",
                table: "fee_structure_categories",
                column: "fee_structure_id");

            migrationBuilder.CreateIndex(
                name: "ix_fee_structure_categories_fee_structure_id_category",
                table: "fee_structure_categories",
                columns: new[] { "fee_structure_id", "category" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fee_payments_payment_date",
                table: "fee_payments",
                column: "payment_date");

            migrationBuilder.CreateIndex(
                name: "ix_fee_payments_receipt_number",
                table: "fee_payments",
                column: "receipt_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_fee_payments_student_fee_id",
                table: "fee_payments",
                column: "student_fee_id");

            migrationBuilder.CreateIndex(
                name: "ix_fee_structures_academic_year",
                table: "fee_structures",
                column: "academic_year");

            migrationBuilder.CreateIndex(
                name: "ix_fee_structures_is_active",
                table: "fee_structures",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_student_fees_fee_structure_id",
                table: "student_fees",
                column: "fee_structure_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_fees_start_date_end_date",
                table: "student_fees",
                columns: new[] { "start_date", "end_date" });

            migrationBuilder.CreateIndex(
                name: "ix_student_fees_student_id",
                table: "student_fees",
                column: "student_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fee_payments");

            migrationBuilder.DropTable(
                name: "fee_structure_categories");

            migrationBuilder.DropTable(
                name: "student_fees");

            migrationBuilder.DropTable(
                name: "fee_structures");
        }
    }
}
