using Waytokaaba.Core.Enums;

namespace Waytokaaba.Core.Entities;

public class Hotel : AuditableEntity
{
    public required string Slug { get; set; }
    public required string Name { get; set; }
    public required string ShortDescription { get; set; }
    public required string Description { get; set; }
    public City City { get; set; }
    public required string Address { get; set; }

    /// <summary>1–5 stars.</summary>
    public int StarRating { get; set; }

    /// <summary>Walking distance to Masjid al-Haram (Makkah) or Masjid an-Nabawi (Madinah), in meters.</summary>
    public int? DistanceFromHolyMosqueMeters { get; set; }

    /// <summary>Only verified distances are shown publicly.</summary>
    public bool IsDistanceVerified { get; set; }

    public required string ImageUrl { get; set; }

    /// <summary>Indicative "from" price per night.</summary>
    public decimal PricePerNight { get; set; }

    public string Currency { get; set; } = "PKR";
    public List<string> Amenities { get; set; } = [];
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>True for sample/demonstration records that are not verified commercial inventory.</summary>
    public bool IsDemoContent { get; set; }

    public List<HotelImage> Images { get; set; } = [];
    public List<Room> Rooms { get; set; } = [];
    public List<PackageHotel> PackageHotels { get; set; } = [];
}
