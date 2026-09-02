using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payhook.Api.Migrations;

public partial class StoreRejectedRawEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_raw_events_transaction_id",
            table: "raw_events");

        migrationBuilder.AlterColumn<string>(
            name: "transaction_id",
            table: "raw_events",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(100)",
            oldMaxLength: 100);

        migrationBuilder.Sql(
            "ALTER TABLE raw_events ALTER COLUMN payload_json TYPE text USING payload_json::text;");

        migrationBuilder.AlterColumn<string>(
            name: "contract_id",
            table: "raw_events",
            type: "character varying(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "character varying(100)",
            oldMaxLength: 100);

        migrationBuilder.AddColumn<bool>(
            name: "is_processable",
            table: "raw_events",
            type: "boolean",
            nullable: false,
            defaultValue: true);

        migrationBuilder.CreateIndex(
            name: "ix_raw_events_transaction_id",
            table: "raw_events",
            column: "transaction_id",
            unique: true,
            filter: "transaction_id IS NOT NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_raw_events_transaction_id",
            table: "raw_events");

        migrationBuilder.DropColumn(
            name: "is_processable",
            table: "raw_events");

        migrationBuilder.AlterColumn<string>(
            name: "transaction_id",
            table: "raw_events",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "character varying(100)",
            oldMaxLength: 100,
            oldNullable: true);

        migrationBuilder.Sql(
            "ALTER TABLE raw_events ALTER COLUMN payload_json TYPE jsonb USING payload_json::jsonb;");

        migrationBuilder.AlterColumn<string>(
            name: "contract_id",
            table: "raw_events",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "character varying(100)",
            oldMaxLength: 100,
            oldNullable: true);

        migrationBuilder.CreateIndex(
            name: "ix_raw_events_transaction_id",
            table: "raw_events",
            column: "transaction_id",
            unique: true);
    }
}
