using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Waytokaaba.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTripSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "trip_settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    visa_transport_price_per_person = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trip_settings", x => x.id);
                    table.CheckConstraint("ck_trip_settings_visa_transport_positive", "visa_transport_price_per_person > 0");
                });

            migrationBuilder.InsertData(
                table: "trip_settings",
                columns: new[] { "id", "visa_transport_price_per_person" },
                values: new object[] { 1, 60000m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trip_settings");
        }
    }
}
