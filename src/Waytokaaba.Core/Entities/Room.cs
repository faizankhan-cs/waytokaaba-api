namespace Waytokaaba.Core.Entities;

public class Room : AuditableEntity
{
    public int HotelId { get; set; }
    public Hotel? Hotel { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    /// <summary>Maximum number of guests.</summary>
    public int Capacity { get; set; }

    public string? BedConfiguration { get; set; }
    public decimal PricePerNight { get; set; }
    public string? ImageUrl { get; set; }
}
