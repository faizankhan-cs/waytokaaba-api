namespace Waytokaaba.Core.Entities;

/// <summary>A service included in a package (visa, transport, ziyarat, etc.).</summary>
public class PackageService : BaseEntity
{
    public int PackageId { get; set; }
    public Package? Package { get; set; }
    public required string ServiceName { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}
