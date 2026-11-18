namespace Waytokaaba.Core.Entities;

/// <summary>A purchasable option for a visa, e.g. "30-Day Tourist Visa".</summary>
public class VisaTier : BaseEntity
{
    public int VisaId { get; set; }
    public Visa? Visa { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int SortOrder { get; set; }
}
