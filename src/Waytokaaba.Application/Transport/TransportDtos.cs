namespace Waytokaaba.Application.Transport;

/// <param name="StartingPrice">Lowest listed price of this vehicle across the routes, or null when it has none.</param>
public sealed record TransportVehicleDto(
    int Id,
    string Name,
    string ImageUrl,
    int Seats,
    int Bags,
    string? Description,
    int SortOrder,
    bool IsActive,
    bool IsDemoContent,
    decimal? StartingPrice);

public sealed record TransportRateDto(int VehicleId, decimal Price);

public sealed record TransportRouteDto(
    int Id,
    string From,
    string? To,
    string? Note,
    int SortOrder,
    bool IsActive,
    bool IsDemoContent,
    IReadOnlyList<TransportRateDto> Rates);

/// <summary>Everything the transport page needs in one response: vehicles, routes and the price grid.</summary>
public sealed record TransportCatalogDto(
    IReadOnlyList<TransportVehicleDto> Vehicles,
    IReadOnlyList<TransportRouteDto> Routes,
    string Currency);

public sealed class UpsertTransportVehicleRequest
{
    public string Name { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public int Seats { get; set; }
    public int Bags { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDemoContent { get; set; }
}

public sealed record TransportRatePriceRequest(int VehicleId, decimal Price);

public sealed class UpsertTransportRouteRequest
{
    public string From { get; set; } = "";
    public string? To { get; set; }
    public string? Note { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDemoContent { get; set; }
    public List<TransportRatePriceRequest> Prices { get; set; } = [];
}
