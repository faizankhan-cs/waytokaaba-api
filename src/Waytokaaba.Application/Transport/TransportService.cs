using Microsoft.EntityFrameworkCore;
using Waytokaaba.Application.Common;
using Waytokaaba.Core.Entities;

namespace Waytokaaba.Application.Transport;

public interface ITransportService
{
    /// <summary>Vehicles, routes and prices. The public view hides inactive vehicles/routes and their prices.</summary>
    Task<TransportCatalogDto> GetCatalogAsync(bool adminView = false, CancellationToken ct = default);

    Task<TransportVehicleDto> GetVehicleAsync(int id, CancellationToken ct = default);
    Task<TransportVehicleDto> CreateVehicleAsync(UpsertTransportVehicleRequest request, CancellationToken ct = default);
    Task<TransportVehicleDto> UpdateVehicleAsync(int id, UpsertTransportVehicleRequest request, CancellationToken ct = default);
    Task DeleteVehicleAsync(int id, CancellationToken ct = default);

    Task<TransportRouteDto> GetRouteAsync(int id, CancellationToken ct = default);
    Task<TransportRouteDto> CreateRouteAsync(UpsertTransportRouteRequest request, CancellationToken ct = default);
    Task<TransportRouteDto> UpdateRouteAsync(int id, UpsertTransportRouteRequest request, CancellationToken ct = default);
    Task DeleteRouteAsync(int id, CancellationToken ct = default);
}

public sealed class TransportService(IAppDbContext db) : ITransportService
{
    private const string Currency = "PKR";

    public async Task<TransportCatalogDto> GetCatalogAsync(bool adminView = false, CancellationToken ct = default)
    {
        var vehicles = await db.TransportVehicles.AsNoTracking()
            .Where(v => adminView || v.IsActive)
            .OrderBy(v => v.SortOrder).ThenBy(v => v.Id)
            .ToListAsync(ct);

        var routes = await db.TransportRoutes.AsNoTracking().Include(r => r.Rates)
            .Where(r => adminView || r.IsActive)
            .OrderBy(r => r.SortOrder).ThenBy(r => r.Id)
            .ToListAsync(ct);

        // Only prices for vehicles that are listed; in the public view a route with no visible price is dropped.
        var vehicleIds = vehicles.Select(v => v.Id).ToHashSet();
        var routeDtos = routes
            .Select(r => ToDto(r, r.Rates.Where(x => vehicleIds.Contains(x.VehicleId))))
            .Where(r => adminView || r.Rates.Count > 0)
            .ToList();

        var lowest = routeDtos.SelectMany(r => r.Rates)
            .GroupBy(x => x.VehicleId)
            .ToDictionary(g => g.Key, g => g.Min(x => x.Price));

        var vehicleDtos = vehicles
            .Select(v => ToDto(v, lowest.TryGetValue(v.Id, out var min) ? min : null))
            .ToList();

        return new TransportCatalogDto(vehicleDtos, routeDtos, Currency);
    }

    // ---------------- Vehicles ----------------

    public async Task<TransportVehicleDto> GetVehicleAsync(int id, CancellationToken ct = default)
    {
        var vehicle = await db.TransportVehicles.AsNoTracking().Include(v => v.Rates).FirstOrDefaultAsync(v => v.Id == id, ct)
            ?? throw new NotFoundException("Vehicle", id);
        return ToDto(vehicle, vehicle.Rates.Count == 0 ? null : vehicle.Rates.Min(r => r.Price));
    }

    public async Task<TransportVehicleDto> CreateVehicleAsync(UpsertTransportVehicleRequest request, CancellationToken ct = default)
    {
        var vehicle = new TransportVehicle { Name = "", ImageUrl = "" };
        Apply(vehicle, request);
        db.TransportVehicles.Add(vehicle);
        await db.SaveChangesAsync(ct);
        return await GetVehicleAsync(vehicle.Id, ct);
    }

    public async Task<TransportVehicleDto> UpdateVehicleAsync(int id, UpsertTransportVehicleRequest request, CancellationToken ct = default)
    {
        var vehicle = await db.TransportVehicles.FirstOrDefaultAsync(v => v.Id == id, ct)
            ?? throw new NotFoundException("Vehicle", id);
        Apply(vehicle, request);
        await db.SaveChangesAsync(ct);
        return await GetVehicleAsync(id, ct);
    }

    /// <summary>Deletes the vehicle and (by cascade) its prices on every route.</summary>
    public async Task DeleteVehicleAsync(int id, CancellationToken ct = default)
    {
        var vehicle = await db.TransportVehicles.FirstOrDefaultAsync(v => v.Id == id, ct)
            ?? throw new NotFoundException("Vehicle", id);
        db.TransportVehicles.Remove(vehicle);
        await db.SaveChangesAsync(ct);
    }

    // ---------------- Routes ----------------

    public async Task<TransportRouteDto> GetRouteAsync(int id, CancellationToken ct = default)
    {
        var route = await db.TransportRoutes.AsNoTracking().Include(r => r.Rates).FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new NotFoundException("Route", id);
        return ToDto(route, route.Rates);
    }

    public async Task<TransportRouteDto> CreateRouteAsync(UpsertTransportRouteRequest request, CancellationToken ct = default)
    {
        await EnsureVehiclesExistAsync(request, ct);
        var route = new TransportRoute { From = "" };
        Apply(route, request);
        db.TransportRoutes.Add(route);
        await db.SaveChangesAsync(ct);
        return await GetRouteAsync(route.Id, ct);
    }

    public async Task<TransportRouteDto> UpdateRouteAsync(int id, UpsertTransportRouteRequest request, CancellationToken ct = default)
    {
        var route = await db.TransportRoutes.Include(r => r.Rates).FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new NotFoundException("Route", id);
        await EnsureVehiclesExistAsync(request, ct);

        db.TransportRates.RemoveRange(route.Rates);
        route.Rates.Clear();

        Apply(route, request);
        await db.SaveChangesAsync(ct);
        return await GetRouteAsync(id, ct);
    }

    public async Task DeleteRouteAsync(int id, CancellationToken ct = default)
    {
        var route = await db.TransportRoutes.FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new NotFoundException("Route", id);
        db.TransportRoutes.Remove(route);
        await db.SaveChangesAsync(ct);
    }

    private async Task EnsureVehiclesExistAsync(UpsertTransportRouteRequest request, CancellationToken ct)
    {
        var ids = request.Prices.Select(p => p.VehicleId).Distinct().ToList();
        var found = await db.TransportVehicles.CountAsync(v => ids.Contains(v.Id), ct);
        if (found != ids.Count)
            throw new RequestValidationException(nameof(request.Prices), "One or more vehicles do not exist.");
    }

    private static void Apply(TransportVehicle v, UpsertTransportVehicleRequest r)
    {
        v.Name = r.Name.Trim();
        v.ImageUrl = r.ImageUrl.Trim();
        v.Seats = r.Seats;
        v.Bags = r.Bags;
        v.Description = string.IsNullOrWhiteSpace(r.Description) ? null : r.Description.Trim();
        v.SortOrder = r.SortOrder;
        v.IsActive = r.IsActive;
        v.IsDemoContent = r.IsDemoContent;
    }

    private static void Apply(TransportRoute route, UpsertTransportRouteRequest r)
    {
        route.From = r.From.Trim();
        route.To = string.IsNullOrWhiteSpace(r.To) ? null : r.To.Trim();
        route.Note = string.IsNullOrWhiteSpace(r.Note) ? null : r.Note.Trim();
        route.SortOrder = r.SortOrder;
        route.IsActive = r.IsActive;
        route.IsDemoContent = r.IsDemoContent;
        route.Rates = r.Prices.Select(p => new TransportRate { VehicleId = p.VehicleId, Price = p.Price }).ToList();
    }

    private static TransportVehicleDto ToDto(TransportVehicle v, decimal? startingPrice) => new(
        v.Id, v.Name, v.ImageUrl, v.Seats, v.Bags, v.Description, v.SortOrder, v.IsActive, v.IsDemoContent, startingPrice);

    private static TransportRouteDto ToDto(TransportRoute r, IEnumerable<TransportRate> rates) => new(
        r.Id, r.From, r.To, r.Note, r.SortOrder, r.IsActive, r.IsDemoContent,
        rates.OrderBy(x => x.VehicleId).Select(x => new TransportRateDto(x.VehicleId, x.Price)).ToList());
}
