using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Waytokaaba.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTransport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transport_routes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    from = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    to = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    note = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_demo_content = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transport_routes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transport_vehicles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    seats = table.Column<int>(type: "integer", nullable: false),
                    bags = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_demo_content = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transport_vehicles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transport_rates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    route_id = table.Column<int>(type: "integer", nullable: false),
                    vehicle_id = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transport_rates", x => x.id);
                    table.CheckConstraint("ck_transport_rates_price_positive", "price > 0");
                    table.ForeignKey(
                        name: "fk_transport_rates_transport_routes_route_id",
                        column: x => x.route_id,
                        principalTable: "transport_routes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_transport_rates_transport_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "transport_vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_transport_rates_route_id_vehicle_id",
                table: "transport_rates",
                columns: new[] { "route_id", "vehicle_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transport_rates_vehicle_id",
                table: "transport_rates",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "ix_transport_routes_sort_order_id",
                table: "transport_routes",
                columns: new[] { "sort_order", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_transport_vehicles_sort_order_id",
                table: "transport_vehicles",
                columns: new[] { "sort_order", "id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transport_rates");

            migrationBuilder.DropTable(
                name: "transport_routes");

            migrationBuilder.DropTable(
                name: "transport_vehicles");
        }
    }
}
