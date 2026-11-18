using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Waytokaaba.Application.Common;
using Waytokaaba.Core.Entities;

namespace Waytokaaba.Application.Hotels;

public sealed class HotelService(IAppDbContext db) : IHotelService
{
    public async Task<PagedResult<HotelSummaryDto>> SearchAsync(HotelQuery query, bool adminView = false, CancellationToken ct = default)
    {
        var hotels = db.Hotels.AsNoTracking();

        if (!adminView)
            hotels = hotels.Where(h => h.IsActive);

        if (query.City is { } city)
            hotels = hotels.Where(h => h.City == city);

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var pattern = $"%{EscapeLike(query.Q.Trim().ToLowerInvariant())}%";
            hotels = hotels.Where(h => EF.Functions.Like(h.Name.ToLower(), pattern, "\\")
                                    || EF.Functions.Like(h.Address.ToLower(), pattern, "\\"));
        }

        if (query.MinPrice is { } min)
            hotels = hotels.Where(h => h.PricePerNight >= min);

        if (query.MaxPrice is { } max)
            hotels = hotels.Where(h => h.PricePerNight <= max);

        if (query.Stars is { Length: > 0 } stars)
            hotels = hotels.Where(h => stars.Contains(h.StarRating));

        if (query.Guests is { } guests)
            hotels = hotels.Where(h => h.Rooms.Any(r => r.Capacity >= guests));

        if (query.Featured is { } featured)
            hotels = hotels.Where(h => h.IsFeatured == featured);

        hotels = (query.Sort ?? HotelSort.Recommended) switch
        {
            HotelSort.PriceAsc => hotels.OrderBy(h => h.PricePerNight).ThenBy(h => h.Id),
            HotelSort.PriceDesc => hotels.OrderByDescending(h => h.PricePerNight).ThenBy(h => h.Id),
            HotelSort.StarsDesc => hotels.OrderByDescending(h => h.StarRating).ThenBy(h => h.PricePerNight).ThenBy(h => h.Id),
            // Unverified distances are unknown to the public, so they sort last.
            HotelSort.DistanceAsc => hotels
                .OrderBy(h => h.IsDistanceVerified && h.DistanceFromHolyMosqueMeters != null ? h.DistanceFromHolyMosqueMeters : int.MaxValue)
                .ThenBy(h => h.Id),
            _ => hotels.OrderByDescending(h => h.IsFeatured).ThenByDescending(h => h.StarRating).ThenBy(h => h.PricePerNight).ThenBy(h => h.Id),
        };

        return await hotels.Select(ToSummary(adminView)).ToPagedResultAsync(query.Page, query.PageSize, ct);
    }

    public async Task<HotelDetailDto> GetAsync(string idOrSlug, bool adminView = false, CancellationToken ct = default)
    {
        IQueryable<Hotel> hotels = db.Hotels.AsNoTracking()
            .Include(h => h.Images.OrderBy(i => i.SortOrder))
            .Include(h => h.Rooms.OrderBy(r => r.PricePerNight));

        if (!adminView)
            hotels = hotels.Where(h => h.IsActive);

        var slug = idOrSlug.ToLowerInvariant();

        var hotel = int.TryParse(idOrSlug, out var id)
            ? await hotels.FirstOrDefaultAsync(h => h.Id == id, ct)
            : await hotels.FirstOrDefaultAsync(h => h.Slug == slug, ct);

        return hotel is null ? throw new NotFoundException("Hotel", idOrSlug) : ToDetail(hotel, adminView);
    }

    public async Task<HotelDetailDto> CreateAsync(UpsertHotelRequest request, CancellationToken ct = default)
    {
        var slug = await EnsureUniqueSlugAsync(request.Slug, request.Name, excludeId: null, ct);
        var hotel = new Hotel
        {
            Slug = slug,
            Name = request.Name,
            ShortDescription = request.ShortDescription,
            Description = request.Description,
            Address = request.Address,
            ImageUrl = request.ImageUrl,
        };
        Apply(hotel, request);
        db.Hotels.Add(hotel);
        await db.SaveChangesAsync(ct);
        return ToDetail(hotel, adminView: true);
    }

    public async Task<HotelDetailDto> UpdateAsync(int id, UpsertHotelRequest request, CancellationToken ct = default)
    {
        var hotel = await db.Hotels
            .Include(h => h.Images)
            .Include(h => h.Rooms)
            .FirstOrDefaultAsync(h => h.Id == id, ct)
            ?? throw new NotFoundException("Hotel", id);

        hotel.Slug = await EnsureUniqueSlugAsync(request.Slug ?? hotel.Slug, request.Name, excludeId: id, ct);
        db.HotelImages.RemoveRange(hotel.Images);
        hotel.Images.Clear();
        Apply(hotel, request);
        await db.SaveChangesAsync(ct);
        return ToDetail(hotel, adminView: true);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var hotel = await db.Hotels.FirstOrDefaultAsync(h => h.Id == id, ct)
            ?? throw new NotFoundException("Hotel", id);

        if (await db.PackageHotels.AnyAsync(ph => ph.HotelId == id, ct))
            throw new ConflictException("This hotel is part of a package. Remove it from the package first, or mark it inactive instead.");

        db.Hotels.Remove(hotel);
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<RoomDto>> GetRoomsAsync(int hotelId, CancellationToken ct = default)
    {
        if (!await db.Hotels.AnyAsync(h => h.Id == hotelId && h.IsActive, ct))
            throw new NotFoundException("Hotel", hotelId);

        return await db.Rooms.AsNoTracking()
            .Where(r => r.HotelId == hotelId)
            .OrderBy(r => r.PricePerNight)
            .Select(r => new RoomDto(r.Id, r.HotelId, r.Name, r.Description, r.Capacity, r.BedConfiguration, r.PricePerNight, r.ImageUrl))
            .ToListAsync(ct);
    }

    public async Task<RoomDto> AddRoomAsync(int hotelId, UpsertRoomRequest request, CancellationToken ct = default)
    {
        if (!await db.Hotels.AnyAsync(h => h.Id == hotelId, ct))
            throw new NotFoundException("Hotel", hotelId);

        var room = new Room { HotelId = hotelId, Name = request.Name };
        Apply(room, request);
        db.Rooms.Add(room);
        await db.SaveChangesAsync(ct);
        return ToDto(room);
    }

    public async Task<RoomDto> UpdateRoomAsync(int hotelId, int roomId, UpsertRoomRequest request, CancellationToken ct = default)
    {
        var room = await db.Rooms.FirstOrDefaultAsync(r => r.Id == roomId && r.HotelId == hotelId, ct)
            ?? throw new NotFoundException("Room", roomId);
        Apply(room, request);
        await db.SaveChangesAsync(ct);
        return ToDto(room);
    }

    public async Task DeleteRoomAsync(int hotelId, int roomId, CancellationToken ct = default)
    {
        var room = await db.Rooms.FirstOrDefaultAsync(r => r.Id == roomId && r.HotelId == hotelId, ct)
            ?? throw new NotFoundException("Room", roomId);
        db.Rooms.Remove(room);
        await db.SaveChangesAsync(ct);
    }

    // ---- helpers -------------------------------------------------------------------------------

    private Task<string> EnsureUniqueSlugAsync(string? requested, string name, int? excludeId, CancellationToken ct) =>
        SlugHelper.ResolveUniqueAsync(requested, name, "hotel",
            slug => db.Hotels.AnyAsync(h => h.Slug == slug && h.Id != excludeId, ct));

    private static void Apply(Hotel hotel, UpsertHotelRequest r)
    {
        hotel.Name = r.Name.Trim();
        hotel.ShortDescription = r.ShortDescription.Trim();
        hotel.Description = r.Description.Trim();
        hotel.City = r.City;
        hotel.Address = r.Address.Trim();
        hotel.StarRating = r.StarRating;
        hotel.DistanceFromHolyMosqueMeters = r.DistanceFromHolyMosqueMeters;
        hotel.IsDistanceVerified = r.IsDistanceVerified;
        hotel.ImageUrl = r.ImageUrl.Trim();
        hotel.PricePerNight = r.PricePerNight;
        hotel.Currency = r.Currency.ToUpperInvariant();
        hotel.Amenities = r.Amenities.Select(a => a.Trim()).Where(a => a.Length > 0).Distinct().ToList();
        hotel.IsFeatured = r.IsFeatured;
        hotel.IsActive = r.IsActive;
        hotel.IsDemoContent = r.IsDemoContent;
        hotel.Images = r.Images
            .Select((img, i) => new HotelImage { Url = img.Url.Trim(), Caption = img.Caption?.Trim(), SortOrder = i })
            .ToList();
    }

    private static void Apply(Room room, UpsertRoomRequest r)
    {
        room.Name = r.Name.Trim();
        room.Description = r.Description?.Trim();
        room.Capacity = r.Capacity;
        room.BedConfiguration = r.BedConfiguration?.Trim();
        room.PricePerNight = r.PricePerNight;
        room.ImageUrl = string.IsNullOrWhiteSpace(r.ImageUrl) ? null : r.ImageUrl.Trim();
    }

    private static Expression<Func<Hotel, HotelSummaryDto>> ToSummary(bool adminView) => h => new HotelSummaryDto(
        h.Id, h.Slug, h.Name, h.City, h.Address, h.StarRating,
        adminView || h.IsDistanceVerified ? h.DistanceFromHolyMosqueMeters : null,
        h.ShortDescription, h.ImageUrl, h.PricePerNight, h.Currency,
        h.Rooms.Max(r => (int?)r.Capacity) ?? 0,
        h.IsFeatured, h.IsActive, h.IsDemoContent);

    private static RoomDto ToDto(Room r) =>
        new(r.Id, r.HotelId, r.Name, r.Description, r.Capacity, r.BedConfiguration, r.PricePerNight, r.ImageUrl);

    private static HotelDetailDto ToDetail(Hotel h, bool adminView) => new(
        h.Id, h.Slug, h.Name, h.City, h.Address, h.StarRating,
        adminView || h.IsDistanceVerified ? h.DistanceFromHolyMosqueMeters : null,
        h.IsDistanceVerified,
        h.ShortDescription, h.Description, h.ImageUrl, h.PricePerNight, h.Currency,
        h.Amenities,
        h.IsFeatured, h.IsActive, h.IsDemoContent,
        h.Images.OrderBy(i => i.SortOrder).Select(i => new HotelImageDto(i.Id, i.Url, i.Caption, i.SortOrder)).ToList(),
        h.Rooms.OrderBy(r => r.PricePerNight).Select(ToDto).ToList(),
        h.CreatedAt, h.UpdatedAt);

    internal static string EscapeLike(string input) =>
        input.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
