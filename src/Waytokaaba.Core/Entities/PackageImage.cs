namespace Waytokaaba.Core.Entities;

public class PackageImage : BaseEntity
{
    public int PackageId { get; set; }
    public Package? Package { get; set; }
    public required string Url { get; set; }
    public string? Caption { get; set; }
    public int SortOrder { get; set; }
}
