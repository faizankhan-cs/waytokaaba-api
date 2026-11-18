namespace Waytokaaba.Core.Entities;

/// <summary>A vehicle type offered for private transport, e.g. "Sedan" or "Coaster".</summary>
public class TransportVehicle : AuditableEntity
{
    public required string Name { get; set; }
    public required string ImageUrl { get; set; }
    public int Seats { get; set; }
    public int Bags { get; set; }
    public string? Description { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDemoContent { get; set; }

    public List<TransportRate> Rates { get; set; } = [];
}
