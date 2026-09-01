using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payhook.Api.Migrations;

public partial class AddRawEventContractId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "contract_id",
            table: "raw_events",
            type: "character varying(100)",
            maxLength: 100,
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateIndex(
            name: "ix_raw_events_contract_id",
            table: "raw_events",
            column: "contract_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_raw_events_contract_id",
            table: "raw_events");

        migrationBuilder.DropColumn(
            name: "contract_id",
            table: "raw_events");
    }
}
