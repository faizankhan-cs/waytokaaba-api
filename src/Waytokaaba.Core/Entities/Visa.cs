using Waytokaaba.Core.Enums;

namespace Waytokaaba.Core.Entities;

/// <summary>A destination country we process visas for. Pricing lives on its <see cref="VisaTier"/> rows.</summary>
public class Visa : AuditableEntity
{
    public required string Slug { get; set; }
    public required string Country { get; set; }

    /// <summary>ISO 3166-1 alpha-2 code (e.g. "AE"), used to show the flag.</summary>
    public required string CountryCode { get; set; }

    public required string ImageUrl { get; set; }
    public VisaFormat VisaFormat { get; set; }
    public required string Summary { get; set; }
    public required string ProcessingTime { get; set; }
    public required string Validity { get; set; }
    public required string AllowedStay { get; set; }
    public VisaEntryType EntryType { get; set; }

    public bool IncludesEmbassyFee { get; set; }
    public bool IncludesInsurance { get; set; }
    public bool IncludesServiceCharge { get; set; }

    public List<string> RequiredDocuments { get; set; } = [];

    /// <summary>Overrides the default disclaimer shown on the detail page when set.</summary>
    public string? Disclaimer { get; set; }

    public int SortOrder { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDemoContent { get; set; }

    public List<VisaTier> Tiers { get; set; } = [];
}
