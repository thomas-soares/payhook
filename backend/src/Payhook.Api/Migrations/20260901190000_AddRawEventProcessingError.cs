using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payhook.Api.Migrations;

public partial class AddRawEventProcessingError : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "processing_error",
            table: "raw_events",
            type: "character varying(1000)",
            maxLength: 1000,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "processing_error",
            table: "raw_events");
    }
}
