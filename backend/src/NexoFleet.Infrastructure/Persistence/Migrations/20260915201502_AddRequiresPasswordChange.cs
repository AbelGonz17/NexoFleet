using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexoFleet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiresPasswordChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "requires_password_change",
                table: "asp_net_users",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "requires_password_change",
                table: "asp_net_users");
        }
    }
}
