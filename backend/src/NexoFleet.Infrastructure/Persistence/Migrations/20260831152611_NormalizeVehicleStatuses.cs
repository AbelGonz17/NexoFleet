using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexoFleet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeVehicleStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE vehicles
                SET status = 'Operational'
                WHERE status IN ('Available', 'InService');

                UPDATE vehicles
                SET approval_status = 'Rejected'
                WHERE approval_status = 'ChangesRequested';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE vehicles
                SET status = 'Available'
                WHERE status = 'Operational';
                """);
        }
    }
}
