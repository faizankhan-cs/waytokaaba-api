using Waytokaaba.Application.Common;
using Waytokaaba.Core.Enums;

namespace Waytokaaba.Application.Hotels;

public sealed record HotelSummaryDto(
    int Id,
    string Slug,
    string Name,
    City City,
    string Address,
    int StarRating,
    int? DistanceFromHolyMosqueMeters,
    string ShortDescription,
    string ImageUrl,
    decimal PricePerNight,
    string Currency,
    int MaxRoomCapacity,
    bool IsFeatured,
    bool IsActive,
    bool IsDemoContent);

public sealed record HotelImageDto(int Id, string Url, string? Caption, int SortOrder);

public sealed record RoomDto(
    int Id,
    int HotelId,
    string Name,
    string? Description,
    int Capacity,
    string? BedConfiguration,
    decimal PricePerNight,
    string? ImageUrl);

public sealed record HotelDetailDto(
    int Id,
    string Slug,
    string Name,
    City City,
    string Address,
    int StarRating,
    int? DistanceFromHolyMosqueMeters,
    bool IsDistanceVerified,
    string ShortDescription,
    string Description,
    string ImageUrl,
    decimal PricePerNight,
    string Currency,
    IReadOnlyList<string> Amenities,
    bool IsFeatured,
    bool IsActive,
    bool IsDemoContent,
    IReadOnlyList<HotelImageDto> Images,
    IReadOnlyList<RoomDto> Rooms,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public static class HotelSort
{
    public const string Recommended = "recommended";
    public const string PriceAsc = "price_asc";
    public const string PriceDesc = "price_desc";
    public const string StarsDesc = "stars_desc";
    public const string DistanceAsc = "distance_asc";
    public static readonly string[] All = [Recommended, PriceAsc, PriceDesc, StarsDesc, DistanceAsc];
}

/// <summary>Hotel search/filter parameters (bound from the query string).</summary>
public sealed class HotelQuery : PagedQuery
{
    public City? City { get; set; }

    /// <summary>Free-text search on hotel name or address.</summary>
    public string? Q { get; set; }

    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    /// <summary>Star ratings to include, e.g. stars=4&amp;stars=5.</summary>
    public int[]? Stars { get; set; }

    /// <summary>Only hotels with at least one room that fits this many guests.</summary>
    public int? Guests { get; set; }

    public bool? Featured { get; set; }

    /// <summary>recommended | price_asc | price_desc | stars_desc | distance_asc</summary>
    public string? Sort { get; set; }
}

public sealed record HotelImageRequest(string Url, string? Caption);

public sealed class UpsertHotelRequest
{
    /// <summary>Optional; generated from Name when omitted.</summary>
    public string? Slug { get; set; }
    public string Name { get; set; } = "";
    public string ShortDescription { get; set; } = "";
    public string Description { get; set; } = "";
    public City City { get; set; }
    public string Address { get; set; } = "";
    public int StarRating { get; set; }
    public int? DistanceFromHolyMosqueMeters { get; set; }
    public bool IsDistanceVerified { get; set; }
    public string ImageUrl { get; set; } = "";
    public decimal PricePerNight { get; set; }
    public string Currency { get; set; } = "PKR";
    public List<string> Amenities { get; set; } = [];
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDemoContent { get; set; }
    public List<HotelImageRequest> Images { get; set; } = [];
}

public sealed class UpsertRoomRequest
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public string? BedConfiguration { get; set; }
    public decimal PricePerNight { get; set; }
    public string? ImageUrl { get; set; }
}
