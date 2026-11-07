using Microsoft.EntityFrameworkCore;
using Waytokaaba.Application.Common;
using Waytokaaba.Application.Hotels;
using Waytokaaba.Core.Entities;
using Waytokaaba.Core.Enums;
using PackageServiceEntity = Waytokaaba.Core.Entities.PackageService;

namespace Waytokaaba.Application.Packages;

public interface IPackageService
{
    Task<PagedResult<PackageSummaryDto>> SearchAsync(PackageQuery query, bool adminView = false, CancellationToken ct = default);
    Task<PackageDetailDto> GetAsync(string idOrSlug, bool adminView = false, CancellationToken ct = default);
    Task<PackageDetailDto> CreateAsync(UpsertPackageRequest request, CancellationToken ct = default);
    Task<PackageDetailDto> UpdateAsync(int id, UpsertPackageRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}

public sealed class UmrahPackageService(IAppDbContext db) : IPackageService
{
    public async Task<PagedResult<PackageSummaryDto>> SearchAsync(PackageQuery query, bool adminView = false, CancellationToken ct = default)
    {
        var packages = db.Packages.AsNoTracking();

        if (!adminView)
            packages = packages.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            var pattern = $"%{HotelService.EscapeLike(query.Q.Trim().ToLowerInvariant())}%";
            packages = packages.Where(p => EF.Functions.Like(p.Name.ToLower(), pattern, "\\")
                                        || EF.Functions.Like(p.ShortDescription.ToLower(), pattern, "\\"));
        }

        if (query.MinPrice is { } minPrice) packages = packages.Where(p => p.Price >= minPrice);
        if (query.MaxPrice is { } maxPrice) packages = packages.Where(p => p.Price <= maxPrice);
        if (query.MinDays is { } minDays) packages = packages.Where(p => p.DurationInDays >= minDays);
        if (query.MaxDays is { } maxDays) packages = packages.Where(p => p.DurationInDays <= maxDays);
        if (query.Type is { Length: > 0 } types) packages = packages.Where(p => types.Contains(p.PackageType));
        if (query.Featured is { } featured) packages = packages.Where(p => p.IsFeatured == featured);

        packages = (query.Sort ?? PackageSort.Recommended) switch
        {
            PackageSort.PriceAsc => packages.OrderBy(p => p.Price).ThenBy(p => p.Id),
            PackageSort.PriceDesc => packages.OrderByDescending(p => p.Price).ThenBy(p => p.Id),
            PackageSort.DurationAsc => packages.OrderBy(p => p.DurationInDays).ThenBy(p => p.Price).ThenBy(p => p.Id),
            PackageSort.DurationDesc => packages.OrderByDescending(p => p.DurationInDays).ThenBy(p => p.Price).ThenBy(p => p.Id),
            _ => packages.OrderByDescending(p => p.IsFeatured).ThenBy(p => p.Price).ThenBy(p => p.Id),
        };

        return await packages
            .Select(p => new PackageSummaryDto(
                p.Id, p.Slug, p.Name, p.PackageType, p.DurationInDays, p.MakkahDays, p.MadinahDays,
                p.Price, p.Currency, p.ShortDescription, p.ImageUrl,
                p.Hotels.OrderBy(h => h.City == City.Makkah ? 0 : 1).Select(h => new PackageHotelSummaryDto(
                    h.HotelId, h.Hotel!.Slug, h.Hotel.Name, h.City, h.Hotel.StarRating, h.Nights)).ToList(),
                p.IsFeatured, p.IsActive, p.IsDemoContent))
            .ToPagedResultAsync(query.Page, query.PageSize, ct);
    }

    public async Task<PackageDetailDto> GetAsync(string idOrSlug, bool adminView = false, CancellationToken ct = default)
    {
        IQueryable<Package> packages = db.Packages.AsNoTracking()
            .Include(p => p.Images)
            .Include(p => p.Services)
            .Include(p => p.Hotels).ThenInclude(h => h.Hotel);

        if (!adminView)
            packages = packages.Where(p => p.IsActive);

        var slug = idOrSlug.ToLowerInvariant();

        var package = int.TryParse(idOrSlug, out var id)
            ? await packages.FirstOrDefaultAsync(p => p.Id == id, ct)
            : await packages.FirstOrDefaultAsync(p => p.Slug == slug, ct);

        return package is null ? throw new NotFoundException("Package", idOrSlug) : ToDetail(package, adminView);
    }

    public async Task<PackageDetailDto> CreateAsync(UpsertPackageRequest request, CancellationToken ct = default)
    {
        var slug = await SlugHelper.ResolveUniqueAsync(request.Slug, request.Name, "package",
            s => db.Packages.AnyAsync(p => p.Slug == s, ct));

        var package = new Package
        {
            Slug = slug,
            Name = request.Name,
            ShortDescription = request.ShortDescription,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
        };
        await ApplyAsync(package, request, ct);
        db.Packages.Add(package);
        await db.SaveChangesAsync(ct);
        return await GetAsync(package.Id.ToString(System.Globalization.CultureInfo.InvariantCulture), adminView: true, ct);
    }

    public async Task<PackageDetailDto> UpdateAsync(int id, UpsertPackageRequest request, CancellationToken ct = default)
    {
        var package = await db.Packages
            .Include(p => p.Images)
            .Include(p => p.Services)
            .Include(p => p.Hotels)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Package", id);

        package.Slug = await SlugHelper.ResolveUniqueAsync(request.Slug ?? package.Slug, request.Name, "package",
            s => db.Packages.AnyAsync(p => p.Slug == s && p.Id != id, ct));

        db.PackageImages.RemoveRange(package.Images);
        db.PackageServices.RemoveRange(package.Services);
        db.PackageHotels.RemoveRange(package.Hotels);
        package.Images.Clear();
        package.Services.Clear();
        package.Hotels.Clear();

        await ApplyAsync(package, request, ct);
        await db.SaveChangesAsync(ct);
        return await GetAsync(id.ToString(System.Globalization.CultureInfo.InvariantCulture), adminView: true, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var package = await db.Packages.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Package", id);
        db.Packages.Remove(package);
        await db.SaveChangesAsync(ct);
    }

    private async Task ApplyAsync(Package p, UpsertPackageRequest r, CancellationToken ct)
    {
        var hotelIds = r.Hotels.Select(h => h.HotelId).ToList();
        var hotels = await db.Hotels.AsNoTracking()
            .Where(h => hotelIds.Contains(h.Id))
            .Select(h => new { h.Id, h.City })
            .ToDictionaryAsync(h => h.Id, h => h.City, ct);

        var missing = hotelIds.Where(id => !hotels.ContainsKey(id)).ToList();
        if (missing.Count > 0)
            throw new RequestValidationException("Hotels", $"Unknown hotel id(s): {string.Join(", ", missing)}.");

        p.Name = r.Name.Trim();
        p.ShortDescription = r.ShortDescription.Trim();
        p.Description = r.Description.Trim();
        p.PackageType = r.PackageType;
        p.DurationInDays = r.DurationInDays;
        p.MakkahDays = r.MakkahDays;
        p.MadinahDays = r.MadinahDays;
        p.Price = r.Price;
        p.Currency = r.Currency.ToUpperInvariant();
        p.ImageUrl = r.ImageUrl.Trim();
        p.IsFeatured = r.IsFeatured;
        p.IsActive = r.IsActive;
        p.IsDemoContent = r.IsDemoContent;
        p.Hotels = r.Hotels.Select(h => new PackageHotel { HotelId = h.HotelId, City = hotels[h.HotelId], Nights = h.Nights }).ToList();
        p.Services = r.Services
            .Select((s, i) => new PackageServiceEntity { ServiceName = s.ServiceName.Trim(), Description = s.Description?.Trim(), SortOrder = i })
            .ToList();
        p.Images = r.Images
            .Select((img, i) => new PackageImage { Url = img.Url.Trim(), Caption = img.Caption?.Trim(), SortOrder = i })
            .ToList();
    }

    private static PackageDetailDto ToDetail(Package p, bool adminView) => new(
        p.Id, p.Slug, p.Name, p.PackageType, p.DurationInDays, p.MakkahDays, p.MadinahDays,
        p.Price, p.Currency, p.ShortDescription, p.Description, p.ImageUrl,
        p.IsFeatured, p.IsActive, p.IsDemoContent,
        p.Images.OrderBy(i => i.SortOrder).Select(i => new PackageImageDto(i.Id, i.Url, i.Caption, i.SortOrder)).ToList(),
        p.Hotels.OrderBy(h => h.City == City.Makkah ? 0 : 1).Select(h => new PackageHotelDto(
            h.HotelId, h.Hotel!.Slug, h.Hotel.Name, h.City, h.Hotel.StarRating, h.Hotel.ImageUrl,
            adminView || h.Hotel.IsDistanceVerified ? h.Hotel.DistanceFromHolyMosqueMeters : null,
            h.Nights)).ToList(),
        p.Services.OrderBy(s => s.SortOrder).Select(s => new PackageServiceDto(s.Id, s.ServiceName, s.Description, s.SortOrder)).ToList(),
        p.CreatedAt, p.UpdatedAt);
}
