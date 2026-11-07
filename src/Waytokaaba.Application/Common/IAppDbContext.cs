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
    DbSet<Inquiry> Inquiries { get; }
    DbSet<AdminUser> AdminUsers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
