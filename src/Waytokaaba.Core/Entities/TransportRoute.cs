namespace Waytokaaba.Core.Entities;

/// <summary>
/// A private transport route, e.g. "Jeddah Airport" → "Makkah Hotel". <see cref="To"/> is empty for a
/// round tour such as "Madinah Ziyarat". Prices per vehicle live on <see cref="TransportRate"/> rows.
/// </summary>
public class TransportRoute : AuditableEntity
{
    public required string From { get; set; }
    public string? To { get; set; }
    public string? Note { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDemoContent { get; set; }

    public List<TransportRate> Rates { get; set; } = [];
}
