namespace Waytokaaba.Core.Entities;

/// <summary>The price (PKR, per vehicle, one way) of one vehicle on one route.</summary>
public class TransportRate : BaseEntity
{
    public int RouteId { get; set; }
    public TransportRoute? Route { get; set; }
    public int VehicleId { get; set; }
    public TransportVehicle? Vehicle { get; set; }
    public decimal Price { get; set; }
}
