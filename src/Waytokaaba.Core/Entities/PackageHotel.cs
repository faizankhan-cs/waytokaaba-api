using Waytokaaba.Core.Enums;

namespace Waytokaaba.Core.Entities;

/// <summary>A hotel stay included in a package.</summary>
public class PackageHotel : BaseEntity
{
    public int PackageId { get; set; }
    public Package? Package { get; set; }
    public int HotelId { get; set; }
    public Hotel? Hotel { get; set; }
    public City City { get; set; }
    public int Nights { get; set; }
}
