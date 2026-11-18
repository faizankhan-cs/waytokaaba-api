using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waytokaaba.Core.Entities;

namespace Waytokaaba.Infrastructure.Persistence.Configurations;

internal sealed class TransportVehicleConfiguration : IEntityTypeConfiguration<TransportVehicle>
{
    public void Configure(EntityTypeBuilder<TransportVehicle> b)
    {
        b.ToTable("transport_vehicles");
        b.Property(v => v.Name).HasMaxLength(100).IsRequired();
        b.Property(v => v.ImageUrl).HasMaxLength(1000).IsRequired();
        b.Property(v => v.Description).HasMaxLength(500);
        b.Property(v => v.IsActive).HasDefaultValue(true);
        b.HasIndex(v => new { v.SortOrder, v.Id });

        b.HasMany(v => v.Rates).WithOne(r => r.Vehicle).HasForeignKey(r => r.VehicleId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class TransportRouteConfiguration : IEntityTypeConfiguration<TransportRoute>
{
    public void Configure(EntityTypeBuilder<TransportRoute> b)
    {
        b.ToTable("transport_routes");
        b.Property(r => r.From).HasMaxLength(100).IsRequired();
        b.Property(r => r.To).HasMaxLength(100);
        b.Property(r => r.Note).HasMaxLength(300);
        b.Property(r => r.IsActive).HasDefaultValue(true);
        b.HasIndex(r => new { r.SortOrder, r.Id });

        b.HasMany(r => r.Rates).WithOne(r => r.Route).HasForeignKey(r => r.RouteId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class TransportRateConfiguration : IEntityTypeConfiguration<TransportRate>
{
    public void Configure(EntityTypeBuilder<TransportRate> b)
    {
        b.ToTable("transport_rates", t => t.HasCheckConstraint("ck_transport_rates_price_positive", "price > 0"));
        b.HasIndex(r => new { r.RouteId, r.VehicleId }).IsUnique();
        b.HasIndex(r => r.VehicleId);
    }
}
