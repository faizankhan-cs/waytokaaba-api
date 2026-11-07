using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waytokaaba.Core.Entities;

namespace Waytokaaba.Infrastructure.Persistence.Configurations;

internal sealed class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> b)
    {
        b.ToTable("packages", t =>
        {
            t.HasCheckConstraint("ck_packages_duration", "duration_in_days BETWEEN 1 AND 90");
            t.HasCheckConstraint("ck_packages_city_days", "makkah_days >= 0 AND madinah_days >= 0 AND makkah_days + madinah_days <= duration_in_days");
            t.HasCheckConstraint("ck_packages_price_positive", "price > 0");
        });

        b.Property(p => p.Slug).HasMaxLength(160).IsRequired();
        b.HasIndex(p => p.Slug).IsUnique();
        b.Property(p => p.Name).HasMaxLength(150).IsRequired();
        b.Property(p => p.ShortDescription).HasMaxLength(300).IsRequired();
        b.Property(p => p.Description).HasMaxLength(5000).IsRequired();
        b.Property(p => p.ImageUrl).HasMaxLength(1000).IsRequired();
        b.Property(p => p.Currency).HasMaxLength(3).IsFixedLength().HasDefaultValue("PKR");
        b.Property(p => p.IsActive).HasDefaultValue(true);

        b.HasIndex(p => p.Price);
        b.HasIndex(p => p.DurationInDays);
        b.HasIndex(p => p.PackageType);
        b.HasIndex(p => p.IsFeatured);

        b.HasMany(p => p.Images).WithOne(i => i.Package).HasForeignKey(i => i.PackageId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(p => p.Services).WithOne(s => s.Package).HasForeignKey(s => s.PackageId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(p => p.Hotels).WithOne(h => h.Package).HasForeignKey(h => h.PackageId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class PackageImageConfiguration : IEntityTypeConfiguration<PackageImage>
{
    public void Configure(EntityTypeBuilder<PackageImage> b)
    {
        b.ToTable("package_images");
        b.Property(i => i.Url).HasMaxLength(1000).IsRequired();
        b.Property(i => i.Caption).HasMaxLength(200);
        b.HasIndex(i => new { i.PackageId, i.SortOrder });
    }
}

internal sealed class PackageHotelConfiguration : IEntityTypeConfiguration<PackageHotel>
{
    public void Configure(EntityTypeBuilder<PackageHotel> b)
    {
        b.ToTable("package_hotels", t => t.HasCheckConstraint("ck_package_hotels_nights", "nights BETWEEN 1 AND 90"));
        b.HasIndex(ph => new { ph.PackageId, ph.HotelId }).IsUnique();
        b.HasIndex(ph => ph.HotelId);
        // A hotel referenced by a package cannot be deleted until it is removed from the package.
        b.HasOne(ph => ph.Hotel).WithMany(h => h.PackageHotels).HasForeignKey(ph => ph.HotelId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PackageServiceConfiguration : IEntityTypeConfiguration<PackageService>
{
    public void Configure(EntityTypeBuilder<PackageService> b)
    {
        b.ToTable("package_services");
        b.Property(s => s.ServiceName).HasMaxLength(120).IsRequired();
        b.Property(s => s.Description).HasMaxLength(500);
        b.HasIndex(s => new { s.PackageId, s.SortOrder });
    }
}
