using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waytokaaba.Core.Entities;

namespace Waytokaaba.Infrastructure.Persistence.Configurations;

internal sealed class VisaConfiguration : IEntityTypeConfiguration<Visa>
{
    public void Configure(EntityTypeBuilder<Visa> b)
    {
        b.ToTable("visas");

        b.Property(v => v.Slug).HasMaxLength(160).IsRequired();
        b.HasIndex(v => v.Slug).IsUnique();
        b.Property(v => v.Country).HasMaxLength(100).IsRequired();
        b.Property(v => v.CountryCode).HasMaxLength(2).IsFixedLength().IsRequired();
        b.Property(v => v.ImageUrl).HasMaxLength(1000).IsRequired();
        b.Property(v => v.Summary).HasMaxLength(500).IsRequired();
        b.Property(v => v.ProcessingTime).HasMaxLength(80).IsRequired();
        b.Property(v => v.Validity).HasMaxLength(80).IsRequired();
        b.Property(v => v.AllowedStay).HasMaxLength(80).IsRequired();
        b.Property(v => v.Disclaimer).HasMaxLength(1000);
        b.Property(v => v.IsActive).HasDefaultValue(true);

        b.HasIndex(v => v.IsFeatured);
        b.HasIndex(v => new { v.SortOrder, v.Country });

        b.HasMany(v => v.Tiers).WithOne(t => t.Visa).HasForeignKey(t => t.VisaId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class VisaTierConfiguration : IEntityTypeConfiguration<VisaTier>
{
    public void Configure(EntityTypeBuilder<VisaTier> b)
    {
        b.ToTable("visa_tiers", t => t.HasCheckConstraint("ck_visa_tiers_price_positive", "price > 0"));
        b.Property(t => t.Name).HasMaxLength(150).IsRequired();
        b.Property(t => t.Description).HasMaxLength(500);
        b.HasIndex(t => new { t.VisaId, t.SortOrder });
    }
}
