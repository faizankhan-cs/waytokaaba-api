namespace Waytokaaba.Core.Entities;

public class HotelImage : BaseEntity
{
    public int HotelId { get; set; }
    public Hotel? Hotel { get; set; }
    public required string Url { get; set; }
    public string? Caption { get; set; }
    public int SortOrder { get; set; }
}
