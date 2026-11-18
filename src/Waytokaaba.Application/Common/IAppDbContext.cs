using Microsoft.EntityFrameworkCore;
using Waytokaaba.Core.Entities;

namespace Waytokaaba.Application.Common;

/// <summary>Persistence abstraction used by application services. Implemented in Infrastructure.</summary>
public interface IAppDbContext
{
    DbSet<Hotel> Hotels { get; }
    DbSet<HotelImage> HotelImages { get; }
    DbSet<Room> Rooms { get; }
    DbSet<Package> Packages { get; }
    DbSet<PackageImage> PackageImages { get; }
    DbSet<PackageHotel> PackageHotels { get; }
    DbSet<PackageService> PackageServices { get; }
    DbSet<Visa> Visas { get; }
    DbSet<VisaTier> VisaTiers { get; }
    DbSet<TransportVehicle> TransportVehicles { get; }
    DbSet<TransportRoute> TransportRoutes { get; }
    DbSet<TransportRate> TransportRates { get; }
    DbSet<TripSettings> TripSettings { get; }
    DbSet<Inquiry> Inquiries { get; }
    DbSet<AdminUser> AdminUsers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
