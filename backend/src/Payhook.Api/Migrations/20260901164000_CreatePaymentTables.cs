using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payhook.Api.Migrations;

public partial class CreatePaymentTables : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "contract_statuses",
            columns: table => new
            {
                contract_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                payment_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_contract_statuses", x => x.contract_id);
            });

        migrationBuilder.CreateTable(
            name: "raw_events",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                transaction_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                payload_json = table.Column<string>(type: "jsonb", nullable: false),
                received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                processing_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_raw_events", x => x.id);
            });

        migrationBuilder.CreateIndex(
            name: "ix_raw_events_transaction_id",
            table: "raw_events",
            column: "transaction_id",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "contract_statuses");

        migrationBuilder.DropTable(
            name: "raw_events");
    }
}
