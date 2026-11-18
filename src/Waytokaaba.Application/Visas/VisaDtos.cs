using Waytokaaba.Application.Common;
using Waytokaaba.Core.Enums;

namespace Waytokaaba.Application.Visas;

public sealed record VisaTierDto(int Id, string Name, string? Description, decimal Price, int SortOrder);

public sealed record VisaSummaryDto(
    int Id,
    string Slug,
    string Country,
    string CountryCode,
    string ImageUrl,
    VisaFormat VisaFormat,
    string Summary,
    string ProcessingTime,
    decimal? StartingPrice,
    string Currency,
    bool IsFeatured,
    bool IsActive,
    bool IsDemoContent);

public sealed record VisaDetailDto(
    int Id,
    string Slug,
    string Country,
    string CountryCode,
    string ImageUrl,
    VisaFormat VisaFormat,
    string Summary,
    string ProcessingTime,
    string Validity,
    string AllowedStay,
    VisaEntryType EntryType,
    bool IncludesEmbassyFee,
    bool IncludesInsurance,
    bool IncludesServiceCharge,
    IReadOnlyList<string> RequiredDocuments,
    string? Disclaimer,
    decimal? StartingPrice,
    string Currency,
    int SortOrder,
    bool IsFeatured,
    bool IsActive,
    bool IsDemoContent,
    IReadOnlyList<VisaTierDto> Tiers,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed class VisaQuery : PagedQuery
{
    public VisaQuery() => PageSize = MaxPageSize;

    /// <summary>Free-text search on country name.</summary>
    public string? Q { get; set; }
    public VisaFormat[]? Format { get; set; }
    public bool? Featured { get; set; }
}

public sealed record VisaTierRequest(string Name, string? Description, decimal Price);

public sealed class UpsertVisaRequest
{
    public string? Slug { get; set; }
    public string Country { get; set; } = "";
    public string CountryCode { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public VisaFormat VisaFormat { get; set; }
    public string Summary { get; set; } = "";
    public string ProcessingTime { get; set; } = "";
    public string Validity { get; set; } = "";
    public string AllowedStay { get; set; } = "";
    public VisaEntryType EntryType { get; set; }
    public bool IncludesEmbassyFee { get; set; }
    public bool IncludesInsurance { get; set; }
    public bool IncludesServiceCharge { get; set; }
    public List<string> RequiredDocuments { get; set; } = [];
    public string? Disclaimer { get; set; }
    public int SortOrder { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDemoContent { get; set; }
    public List<VisaTierRequest> Tiers { get; set; } = [];
}
