using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexoFleet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AuditLogEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "actor_email",
                table: "audit_logs",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "actor_role",
                table: "audit_logs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "severity",
                table: "audit_logs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "actor_email",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "actor_role",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "severity",
                table: "audit_logs");
        }
    }
}
