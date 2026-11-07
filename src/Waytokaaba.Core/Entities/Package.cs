using Waytokaaba.Core.Enums;

namespace Waytokaaba.Core.Entities;

public class Package : AuditableEntity
{
    public required string Slug { get; set; }
    public required string Name { get; set; }
    public required string ShortDescription { get; set; }
    public required string Description { get; set; }
    public PackageType PackageType { get; set; }
    public int DurationInDays { get; set; }
    public int MakkahDays { get; set; }
    public int MadinahDays { get; set; }

    /// <summary>Indicative price per person.</summary>
    public decimal Price { get; set; }

    public string Currency { get; set; } = "PKR";
    public required string ImageUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDemoContent { get; set; }

    public List<PackageImage> Images { get; set; } = [];
    public List<PackageHotel> Hotels { get; set; } = [];
    public List<PackageService> Services { get; set; } = [];
}
