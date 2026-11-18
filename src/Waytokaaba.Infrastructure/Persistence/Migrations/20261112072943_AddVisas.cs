using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Waytokaaba.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVisas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "visas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    country_code = table.Column<string>(type: "character(2)", fixedLength: true, maxLength: 2, nullable: false),
                    image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    visa_format = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    processing_time = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    validity = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    allowed_stay = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    entry_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    includes_embassy_fee = table.Column<bool>(type: "boolean", nullable: false),
                    includes_insurance = table.Column<bool>(type: "boolean", nullable: false),
                    includes_service_charge = table.Column<bool>(type: "boolean", nullable: false),
                    required_documents = table.Column<List<string>>(type: "text[]", nullable: false),
                    disclaimer = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_featured = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_demo_content = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_visas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "visa_tiers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    visa_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_visa_tiers", x => x.id);
                    table.CheckConstraint("ck_visa_tiers_price_positive", "price > 0");
                    table.ForeignKey(
                        name: "fk_visa_tiers_visas_visa_id",
                        column: x => x.visa_id,
                        principalTable: "visas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_visa_tiers_visa_id_sort_order",
                table: "visa_tiers",
                columns: new[] { "visa_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_visas_is_featured",
                table: "visas",
                column: "is_featured");

            migrationBuilder.CreateIndex(
                name: "ix_visas_slug",
                table: "visas",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_visas_sort_order_country",
                table: "visas",
                columns: new[] { "sort_order", "country" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "visa_tiers");

            migrationBuilder.DropTable(
                name: "visas");
        }
    }
}
