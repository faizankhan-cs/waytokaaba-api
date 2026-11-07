using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Waytokaaba.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    last_login_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_admin_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hotels",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    short_description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    city = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    star_rating = table.Column<int>(type: "integer", nullable: false),
                    distance_from_holy_mosque_meters = table.Column<int>(type: "integer", nullable: true),
                    is_distance_verified = table.Column<bool>(type: "boolean", nullable: false),
                    image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    price_per_night = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false, defaultValue: "PKR"),
                    amenities = table.Column<List<string>>(type: "text[]", nullable: false),
                    is_featured = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_demo_content = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hotels", x => x.id);
                    table.CheckConstraint("ck_hotels_distance_non_negative", "distance_from_holy_mosque_meters IS NULL OR distance_from_holy_mosque_meters >= 0");
                    table.CheckConstraint("ck_hotels_price_positive", "price_per_night > 0");
                    table.CheckConstraint("ck_hotels_star_rating", "star_rating BETWEEN 1 AND 5");
                });

            migrationBuilder.CreateTable(
                name: "packages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    slug = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    short_description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    package_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    duration_in_days = table.Column<int>(type: "integer", nullable: false),
                    makkah_days = table.Column<int>(type: "integer", nullable: false),
                    madinah_days = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character(3)", fixedLength: true, maxLength: 3, nullable: false, defaultValue: "PKR"),
                    image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_featured = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_demo_content = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_packages", x => x.id);
                    table.CheckConstraint("ck_packages_city_days", "makkah_days >= 0 AND madinah_days >= 0 AND makkah_days + madinah_days <= duration_in_days");
                    table.CheckConstraint("ck_packages_duration", "duration_in_days BETWEEN 1 AND 90");
                    table.CheckConstraint("ck_packages_price_positive", "price > 0");
                });

            migrationBuilder.CreateTable(
                name: "hotel_images",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hotel_id = table.Column<int>(type: "integer", nullable: false),
                    url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    caption = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_hotel_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_hotel_images_hotels_hotel_id",
                        column: x => x.hotel_id,
                        principalTable: "hotels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hotel_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    capacity = table.Column<int>(type: "integer", nullable: false),
                    bed_configuration = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    price_per_night = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    image_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rooms", x => x.id);
                    table.CheckConstraint("ck_rooms_capacity", "capacity BETWEEN 1 AND 20");
                    table.CheckConstraint("ck_rooms_price_positive", "price_per_night > 0");
                    table.ForeignKey(
                        name: "fk_rooms_hotels_hotel_id",
                        column: x => x.hotel_id,
                        principalTable: "hotels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inquiries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customer_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    inquiry_type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    package_id = table.Column<int>(type: "integer", nullable: true),
                    hotel_id = table.Column<int>(type: "integer", nullable: true),
                    check_in = table.Column<DateOnly>(type: "date", nullable: true),
                    check_out = table.Column<DateOnly>(type: "date", nullable: true),
                    guests = table.Column<int>(type: "integer", nullable: true),
                    message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inquiries", x => x.id);
                    table.CheckConstraint("ck_inquiries_dates", "check_in IS NULL OR check_out IS NULL OR check_out > check_in");
                    table.CheckConstraint("ck_inquiries_guests", "guests IS NULL OR guests BETWEEN 1 AND 50");
                    table.ForeignKey(
                        name: "fk_inquiries_hotels_hotel_id",
                        column: x => x.hotel_id,
                        principalTable: "hotels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_inquiries_packages_package_id",
                        column: x => x.package_id,
                        principalTable: "packages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "package_hotels",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    package_id = table.Column<int>(type: "integer", nullable: false),
                    hotel_id = table.Column<int>(type: "integer", nullable: false),
                    city = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    nights = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_package_hotels", x => x.id);
                    table.CheckConstraint("ck_package_hotels_nights", "nights BETWEEN 1 AND 90");
                    table.ForeignKey(
                        name: "fk_package_hotels_hotels_hotel_id",
                        column: x => x.hotel_id,
                        principalTable: "hotels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_package_hotels_packages_package_id",
                        column: x => x.package_id,
                        principalTable: "packages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "package_images",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    package_id = table.Column<int>(type: "integer", nullable: false),
                    url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    caption = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_package_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_package_images_packages_package_id",
                        column: x => x.package_id,
                        principalTable: "packages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "package_services",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    package_id = table.Column<int>(type: "integer", nullable: false),
                    service_name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_package_services", x => x.id);
                    table.ForeignKey(
                        name: "fk_package_services_packages_package_id",
                        column: x => x.package_id,
                        principalTable: "packages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_admin_users_email",
                table: "admin_users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_hotel_images_hotel_id_sort_order",
                table: "hotel_images",
                columns: new[] { "hotel_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_hotels_city_price_per_night",
                table: "hotels",
                columns: new[] { "city", "price_per_night" });

            migrationBuilder.CreateIndex(
                name: "ix_hotels_city_star_rating",
                table: "hotels",
                columns: new[] { "city", "star_rating" });

            migrationBuilder.CreateIndex(
                name: "ix_hotels_is_featured",
                table: "hotels",
                column: "is_featured");

            migrationBuilder.CreateIndex(
                name: "ix_hotels_slug",
                table: "hotels",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_inquiries_created_at",
                table: "inquiries",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_inquiries_hotel_id",
                table: "inquiries",
                column: "hotel_id");

            migrationBuilder.CreateIndex(
                name: "ix_inquiries_package_id",
                table: "inquiries",
                column: "package_id");

            migrationBuilder.CreateIndex(
                name: "ix_inquiries_status_created_at",
                table: "inquiries",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_package_hotels_hotel_id",
                table: "package_hotels",
                column: "hotel_id");

            migrationBuilder.CreateIndex(
                name: "ix_package_hotels_package_id_hotel_id",
                table: "package_hotels",
                columns: new[] { "package_id", "hotel_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_package_images_package_id_sort_order",
                table: "package_images",
                columns: new[] { "package_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_package_services_package_id_sort_order",
                table: "package_services",
                columns: new[] { "package_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_packages_duration_in_days",
                table: "packages",
                column: "duration_in_days");

            migrationBuilder.CreateIndex(
                name: "ix_packages_is_featured",
                table: "packages",
                column: "is_featured");

            migrationBuilder.CreateIndex(
                name: "ix_packages_package_type",
                table: "packages",
                column: "package_type");

            migrationBuilder.CreateIndex(
                name: "ix_packages_price",
                table: "packages",
                column: "price");

            migrationBuilder.CreateIndex(
                name: "ix_packages_slug",
                table: "packages",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_rooms_hotel_id_capacity",
                table: "rooms",
                columns: new[] { "hotel_id", "capacity" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_users");

            migrationBuilder.DropTable(
                name: "hotel_images");

            migrationBuilder.DropTable(
                name: "inquiries");

            migrationBuilder.DropTable(
                name: "package_hotels");

            migrationBuilder.DropTable(
                name: "package_images");

            migrationBuilder.DropTable(
                name: "package_services");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "packages");

            migrationBuilder.DropTable(
                name: "hotels");
        }
    }
}
