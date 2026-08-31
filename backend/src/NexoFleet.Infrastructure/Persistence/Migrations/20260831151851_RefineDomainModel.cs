using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexoFleet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefineDomainModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "uses_own_vehicle",
                table: "employees");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "uses_own_vehicle",
                table: "employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
