using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waytokaaba.Core.Entities;

namespace Waytokaaba.Infrastructure.Persistence.Configurations;

internal sealed class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> b)
    {
        b.ToTable("hotels", t =>
        {
            t.HasCheckConstraint("ck_hotels_star_rating", "star_rating BETWEEN 1 AND 5");
            t.HasCheckConstraint("ck_hotels_price_positive", "price_per_night > 0");
            t.HasCheckConstraint("ck_hotels_distance_non_negative", "distance_from_holy_mosque_meters IS NULL OR distance_from_holy_mosque_meters >= 0");
        });

        b.Property(h => h.Slug).HasMaxLength(160).IsRequired();
        b.HasIndex(h => h.Slug).IsUnique();
        b.Property(h => h.Name).HasMaxLength(150).IsRequired();
        b.Property(h => h.ShortDescription).HasMaxLength(300).IsRequired();
        b.Property(h => h.Description).HasMaxLength(5000).IsRequired();
        b.Property(h => h.Address).HasMaxLength(300).IsRequired();
        b.Property(h => h.ImageUrl).HasMaxLength(1000).IsRequired();
        b.Property(h => h.Currency).HasMaxLength(3).IsFixedLength().HasDefaultValue("PKR");
        b.Property(h => h.Amenities).HasColumnType("text[]");
        b.Property(h => h.IsActive).HasDefaultValue(true);

        b.HasIndex(h => new { h.City, h.PricePerNight });
        b.HasIndex(h => new { h.City, h.StarRating });
        b.HasIndex(h => h.IsFeatured);

        b.HasMany(h => h.Images).WithOne(i => i.Hotel).HasForeignKey(i => i.HotelId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(h => h.Rooms).WithOne(r => r.Hotel).HasForeignKey(r => r.HotelId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class HotelImageConfiguration : IEntityTypeConfiguration<HotelImage>
{
    public void Configure(EntityTypeBuilder<HotelImage> b)
    {
        b.ToTable("hotel_images");
        b.Property(i => i.Url).HasMaxLength(1000).IsRequired();
        b.Property(i => i.Caption).HasMaxLength(200);
        b.HasIndex(i => new { i.HotelId, i.SortOrder });
    }
}

internal sealed class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> b)
    {
        b.ToTable("rooms", t =>
        {
            t.HasCheckConstraint("ck_rooms_capacity", "capacity BETWEEN 1 AND 20");
            t.HasCheckConstraint("ck_rooms_price_positive", "price_per_night > 0");
        });
        b.Property(r => r.Name).HasMaxLength(120).IsRequired();
        b.Property(r => r.Description).HasMaxLength(2000);
        b.Property(r => r.BedConfiguration).HasMaxLength(120);
        b.Property(r => r.ImageUrl).HasMaxLength(1000);
        b.HasIndex(r => new { r.HotelId, r.Capacity });
    }
}
