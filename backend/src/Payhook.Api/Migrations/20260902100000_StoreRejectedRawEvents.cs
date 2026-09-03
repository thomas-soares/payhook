using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payhook.Api.Migrations;

public partial class StoreRejectedRawEvents : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP INDEX IF EXISTS ix_raw_events_transaction_id;");
        migrationBuilder.Sql("ALTER TABLE raw_events ALTER COLUMN transaction_id DROP NOT NULL;");
        migrationBuilder.Sql("ALTER TABLE raw_events ALTER COLUMN payload_json TYPE text USING payload_json::text;");
        migrationBuilder.Sql("ALTER TABLE raw_events ALTER COLUMN contract_id DROP NOT NULL;");
        migrationBuilder.Sql(
            "ALTER TABLE raw_events ADD COLUMN IF NOT EXISTS is_processable boolean NOT NULL DEFAULT TRUE;");
        migrationBuilder.Sql(
            "CREATE UNIQUE INDEX IF NOT EXISTS ix_raw_events_transaction_id ON raw_events (transaction_id) WHERE transaction_id IS NOT NULL;");
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
