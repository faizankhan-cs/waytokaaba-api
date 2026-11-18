using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Waytokaaba.Core.Entities;

namespace Waytokaaba.Infrastructure.Persistence.Configurations;

internal sealed class TripSettingsConfiguration : IEntityTypeConfiguration<TripSettings>
{
    public void Configure(EntityTypeBuilder<TripSettings> b)
    {
        b.ToTable("trip_settings", t =>
            t.HasCheckConstraint("ck_trip_settings_visa_transport_positive", "visa_transport_price_per_person > 0"));
        b.Property(s => s.Id).ValueGeneratedNever();
        b.Property(s => s.VisaTransportPricePerPerson).HasPrecision(18, 2);

        // The single settings row exists in every environment. The price is a placeholder for the client to set in the admin panel.
        b.HasData(new TripSettings { Id = TripSettings.SingletonId, VisaTransportPricePerPerson = 60_000m });
    }
}
