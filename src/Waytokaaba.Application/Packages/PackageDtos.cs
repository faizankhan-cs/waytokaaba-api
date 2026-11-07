using Waytokaaba.Application.Common;
using Waytokaaba.Core.Enums;

namespace Waytokaaba.Application.Packages;

public sealed record PackageHotelSummaryDto(int HotelId, string HotelSlug, string HotelName, City City, int StarRating, int Nights);

public sealed record PackageSummaryDto(
    int Id,
    string Slug,
    string Name,
    PackageType PackageType,
    int DurationInDays,
    int MakkahDays,
    int MadinahDays,
    decimal Price,
    string Currency,
    string ShortDescription,
    string ImageUrl,
    IReadOnlyList<PackageHotelSummaryDto> Hotels,
    bool IsFeatured,
    bool IsActive,
    bool IsDemoContent);

public sealed record PackageImageDto(int Id, string Url, string? Caption, int SortOrder);

public sealed record PackageServiceDto(int Id, string ServiceName, string? Description, int SortOrder);

public sealed record PackageHotelDto(
    int HotelId,
    string HotelSlug,
    string HotelName,
    City City,
    int StarRating,
    string ImageUrl,
    int? DistanceFromHolyMosqueMeters,
    int Nights);

public sealed record PackageDetailDto(
    int Id,
    string Slug,
    string Name,
    PackageType PackageType,
    int DurationInDays,
    int MakkahDays,
    int MadinahDays,
    decimal Price,
    string Currency,
    string ShortDescription,
    string Description,
    string ImageUrl,
    bool IsFeatured,
    bool IsActive,
    bool IsDemoContent,
    IReadOnlyList<PackageImageDto> Images,
    IReadOnlyList<PackageHotelDto> Hotels,
    IReadOnlyList<PackageServiceDto> Services,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public static class PackageSort
{
    public const string Recommended = "recommended";
    public const string PriceAsc = "price_asc";
    public const string PriceDesc = "price_desc";
    public const string DurationAsc = "duration_asc";
    public const string DurationDesc = "duration_desc";
    public static readonly string[] All = [Recommended, PriceAsc, PriceDesc, DurationAsc, DurationDesc];
}

public sealed class PackageQuery : PagedQuery
{
    /// <summary>Free-text search on package name.</summary>
    public string? Q { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int? MinDays { get; set; }
    public int? MaxDays { get; set; }
    public PackageType[]? Type { get; set; }
    public bool? Featured { get; set; }

    /// <summary>recommended | price_asc | price_desc | duration_asc | duration_desc</summary>
    public string? Sort { get; set; }
}

public sealed record PackageHotelRequest(int HotelId, int Nights);
public sealed record PackageServiceRequest(string ServiceName, string? Description);
public sealed record PackageImageRequest(string Url, string? Caption);

public sealed class UpsertPackageRequest
{
    public string? Slug { get; set; }
    public string Name { get; set; } = "";
    public string ShortDescription { get; set; } = "";
    public string Description { get; set; } = "";
    public PackageType PackageType { get; set; }
    public int DurationInDays { get; set; }
    public int MakkahDays { get; set; }
    public int MadinahDays { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "PKR";
    public string ImageUrl { get; set; } = "";
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDemoContent { get; set; }
    public List<PackageHotelRequest> Hotels { get; set; } = [];
    public List<PackageServiceRequest> Services { get; set; } = [];
    public List<PackageImageRequest> Images { get; set; } = [];
}
