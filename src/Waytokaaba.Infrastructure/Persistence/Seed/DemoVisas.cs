using Waytokaaba.Core.Entities;
using Waytokaaba.Core.Enums;

namespace Waytokaaba.Infrastructure.Persistence.Seed;

/// <summary>
/// Demonstration visa listings. Prices, timelines and document lists are INDICATIVE placeholders, not real quotes,
/// and every record is flagged <c>IsDemoContent</c>. The agency replaces them in the admin area.
/// Landmark photos are from Wikimedia Commons (free licences, attribution on the file pages) or Unsplash.
/// </summary>
internal static class DemoVisas
{
    private const string Wikimedia = "https://thumb.wikimedia.org/wikipedia/commons/thumb";

    private static readonly string[] TouristDocuments =
    [
        "Scanned passport valid for 6+ months",
        "CNIC copy (front and back)",
        "Passport-size photo on a white background",
        "Bank statement of the last 3 months (if required)",
    ];

    public static List<Visa> Build() =>
    [
        Visa("uae-visa", "United Arab Emirates", "AE", VisaFormat.EVisa,
            $"{Wikimedia}/9/90/Burj_Khalifa_%28worlds_tallest_building%29_and_the_Dubai_skyline_%2825781049892%29.jpg/1280px-Burj_Khalifa_%28worlds_tallest_building%29_and_the_Dubai_skyline_%2825781049892%29.jpg",
            "Dubai and Abu Dhabi tourist e-visa delivered by email, with no embassy visit.",
            "3–5 Working Days", "60 days to enter", "30 or 60 days", VisaEntryType.SingleEntry,
            embassy: true, insurance: false, service: true, featured: true, order: 1,
            [("30-Day Tourist Visa", "Single entry, 30 days stay.", 28_000m), ("60-Day Tourist Visa", "Single entry, 60 days stay.", 42_000m)],
            TouristDocuments),

        Visa("turkey-visa", "Turkey", "TR", VisaFormat.EVisa,
            $"{Wikimedia}/4/4a/Hagia_Sophia_%28228968325%29.jpeg/1280px-Hagia_Sophia_%28228968325%29.jpeg",
            "Istanbul, Cappadocia and Antalya tourist e-visa for eligible passport holders.",
            "5–7 Working Days", "180 days to enter", "Up to 30 days", VisaEntryType.SingleEntry,
            embassy: true, insurance: false, service: true, featured: true, order: 2,
            [("30-Day Tourist E-Visa", "Single entry, up to 30 days stay.", 35_000m)],
            [.. TouristDocuments, "Confirmed return air ticket", "Hotel booking for the stay"]),

        Visa("saudi-umrah-visa", "Saudi Arabia (Umrah)", "SA", VisaFormat.UmrahVisa,
            "https://images.unsplash.com/photo-1580418827493-f2b22c0a76cb?auto=format&fit=crop&w=1600&q=80",
            "Umrah-only visa for Makkah and Madinah, processed with your package or on its own.",
            "5–10 Working Days", "90 days to enter", "Up to 30 days", VisaEntryType.SingleEntry,
            embassy: true, insurance: true, service: true, featured: true, order: 3,
            [("Umrah Visa Only", "Visa processing without accommodation or flights.", 45_000m)],
            ["Scanned passport valid for 6+ months", "CNIC copy (front and back)", "Passport-size photo on a white background", "Meningitis vaccination certificate"]),

        Visa("malaysia-visa", "Malaysia", "MY", VisaFormat.EVisa,
            $"{Wikimedia}/d/d2/Petronas_Panorama_II_%284to3%29.jpg/1280px-Petronas_Panorama_II_%284to3%29.jpg",
            "Kuala Lumpur, Langkawi and Penang tourist e-visa.",
            "7–10 Working Days", "90 days to enter", "Up to 30 days", VisaEntryType.SingleEntry,
            embassy: true, insurance: false, service: true, featured: true, order: 4,
            [("30-Day Tourist Visa", "Single entry, up to 30 days stay.", 32_000m)],
            [.. TouristDocuments, "Confirmed return air ticket"]),

        Visa("thailand-visa", "Thailand", "TH", VisaFormat.StickerVisa,
            $"{Wikimedia}/2/2a/%E0%B9%80%E0%B8%88%E0%B8%94%E0%B8%B5%E0%B8%A2%E0%B9%8C%E0%B8%9B%E0%B8%A3%E0%B8%B0%E0%B8%98%E0%B8%B2%E0%B8%99%E0%B8%97%E0%B8%A3%E0%B8%87%E0%B8%9B%E0%B8%A3%E0%B8%B2%E0%B8%87%E0%B8%84%E0%B9%8C%E0%B8%A7%E0%B8%B1%E0%B8%94%E0%B8%AD%E0%B8%A3%E0%B8%B8%E0%B8%932.jpg/1280px-%E0%B9%80%E0%B8%88%E0%B8%94%E0%B8%B5%E0%B8%A2%E0%B9%8C%E0%B8%9B%E0%B8%A3%E0%B8%B0%E0%B8%98%E0%B8%B2%E0%B8%99%E0%B8%97%E0%B8%A3%E0%B8%87%E0%B8%9B%E0%B8%A3%E0%B8%B2%E0%B8%87%E0%B8%84%E0%B9%8C%E0%B8%A7%E0%B8%B1%E0%B8%94%E0%B8%AD%E0%B8%A3%E0%B8%B8%E0%B8%932.jpg",
            "Bangkok, Phuket and Pattaya tourist visa. Passport submitted to the embassy.",
            "10–14 Working Days", "90 days to enter", "Up to 60 days", VisaEntryType.SingleEntry,
            embassy: true, insurance: false, service: true, featured: false, order: 5,
            [("Tourist Visa (Single Entry)", "Single entry, up to 60 days stay.", 38_000m)],
            [.. TouristDocuments, "Confirmed return air ticket", "Hotel booking for the stay"]),

        Visa("uk-visa", "United Kingdom", "GB", VisaFormat.StickerVisa,
            $"{Wikimedia}/0/05/Elizabeth_Tower_and_the_north_front_of_the_Palace_of_Westminster%2C_London.jpg/1280px-Elizabeth_Tower_and_the_north_front_of_the_Palace_of_Westminster%2C_London.jpg",
            "Standard visitor visa for tourism, family visits and business trips. Biometrics at a visa centre.",
            "15–20 Working Days", "6 months", "Up to 6 months", VisaEntryType.MultipleEntry,
            embassy: false, insurance: false, service: true, featured: false, order: 6,
            [("6-Month Standard Visitor", "Multiple entry, up to 6 months per visit. Embassy fee paid separately.", 60_000m)],
            [.. TouristDocuments, "Employment or business proof", "Travel history and itinerary", "Sponsor letter (if applicable)"]),
    ];

    private static Visa Visa(
        string slug, string country, string code, VisaFormat format, string image, string summary,
        string processing, string validity, string stay, VisaEntryType entry,
        bool embassy, bool insurance, bool service, bool featured, int order,
        (string Name, string? Description, decimal Price)[] tiers, string[] documents) => new()
    {
        Slug = slug,
        Country = country,
        CountryCode = code,
        ImageUrl = image,
        VisaFormat = format,
        Summary = summary,
        ProcessingTime = processing,
        Validity = validity,
        AllowedStay = stay,
        EntryType = entry,
        IncludesEmbassyFee = embassy,
        IncludesInsurance = insurance,
        IncludesServiceCharge = service,
        RequiredDocuments = [.. documents],
        SortOrder = order,
        IsFeatured = featured,
        IsActive = true,
        IsDemoContent = true,
        Tiers = [.. tiers.Select((t, i) => new VisaTier { Name = t.Name, Description = t.Description, Price = t.Price, SortOrder = i })],
    };
}
