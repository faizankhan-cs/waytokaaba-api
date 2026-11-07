using Waytokaaba.Core.Entities;
using Waytokaaba.Core.Enums;
using PackageServiceEntity = Waytokaaba.Core.Entities.PackageService;

namespace Waytokaaba.Infrastructure.Persistence.Seed;

/// <summary>
/// Demonstration content for the MVP. Hotel names are FICTIONAL, prices are INDICATIVE (PKR), and all
/// records are flagged <c>IsDemoContent</c> so the UI labels them as sample listings. Distances are stored
/// but left unverified, so they are not shown publicly until an admin verifies them.
/// Images are from Unsplash (Unsplash License: free for commercial use) and can be replaced in the admin area.
/// </summary>
internal static class DemoData
{
    private static string Img(string id, int width = 1600) =>
        $"https://images.unsplash.com/photo-{id}?auto=format&fit=crop&w={width}&q=80";

    // Verified image ids (HTTP 200, content checked).
    private static class Photos
    {
        public const string KaabaCrowdTower = "1580418827493-f2b22c0a76cb";
        public const string KaabaClockTower = "1627728734379-a5f8c099763e";
        public const string KaabaArch = "1713239060784-e6ed820a0715";
        public const string KaabaSun = "1592326871020-04f58c1a52f3";
        public const string KaabaCourtyard = "1693590614566-1d3ea9ef32f7";
        public const string KaabaTowers = "1720549973451-018d3623b55a";
        public const string KaabaPilgrims = "1564769625905-50e93615e769";
        public const string KaabaWhite = "1591004272853-1462c050dca8";

        public const string NabawiSunset = "1591604129939-f1efa4d9f7fa";
        public const string NabawiGreenDome = "1646424857576-2a66db82a65c";
        public const string NabawiMinarets = "1605976528013-638e49b6599f";
        public const string NabawiDomeSky = "1724191078796-8a997b989f43";
        public const string NabawiDomeMinaret = "1588513646744-e94675143d6f";
        public const string NabawiBlueSky = "1711261817080-8f064416206c";
        public const string NabawiUmbrellas = "1677835214504-9648944c3e50";
        public const string NabawiClearSky = "1667454872134-c25973237138";

        public const string RoomWhiteLinen = "1618773928121-c32242e63f39";
        public const string RoomWoodBed = "1611892440504-42a792e24d32";
        public const string RoomBed = "1631049307264-da0ec9d70304";
        public const string RoomGrayPillow = "1629140727571-9b5c6f6267b4";
        public const string RoomBed2 = "1631049552057-403cdb8f0658";
        public const string RoomSofa = "1578683010236-d716f9a3f461";
        public const string RoomLiving = "1583847268964-b28dc8f51f92";
        public const string RoomWhiteSheets = "1711059985570-4c32ed12a12c";
        public const string RoomCurtains = "1759328257280-28bdb3050be3";
        public const string RoomWood = "1765547090903-348b711f0eee";

        public const string LobbyWood = "1621293954908-907159247fc8";
        public const string LobbyReception = "1782890790046-2aefb0b503cb";
        public const string Chandelier = "1625244724120-1fd1d34d00f6";
        public const string LobbySleek = "1758193783649-13371d7fb8dd";
        public const string LobbyMarbleGold = "1660557989695-14fac79c086d";
        public const string LobbyMarblePlants = "1758448500688-3ababa93fd67";
        public const string LobbyArched = "1573052905904-34ad8c27f0cc";
        public const string LobbyLounge = "1587702068694-a909ef4aa346";
        public const string Hallway = "1630587148265-761cbd139043";
        public const string LobbyLighting = "1776361984994-089a9df800f6";
    }

    private sealed record HotelSeed(
        string Slug, string Name, City City, int Stars, int DistanceMeters, decimal Price,
        string Address, string Short, string Description, string MainPhoto, string[] Gallery,
        string[] Amenities, bool Featured, bool HasSuite);

    private static readonly string[] CommonAmenities = ["Free Wi-Fi", "24-hour front desk", "Air conditioning", "Elevator", "Daily housekeeping"];

    private static readonly HotelSeed[] HotelSeeds =
    [
        // ---------------- Makkah ----------------
        new("al-noor-haram-view-hotel", "Al Noor Haram View Hotel", City.Makkah, 5, 250, 85_000m,
            "Ibrahim Al Khalil Road, Ajyad, Makkah",
            "Elegant five-star rooms a short walk from Masjid al-Haram, with select rooms overlooking the Haram.",
            "A refined five-star stay designed around the rhythm of worship. Spacious rooms feature premium bedding, prayer mats and Qibla direction, and blackout curtains for rest between prayers. Guests can enjoy an international breakfast buffet, a quiet lounge, and attentive multilingual staff who can help with transport and ziyarat arrangements.",
            Photos.LobbyMarbleGold, [Photos.KaabaTowers, Photos.RoomWhiteLinen, Photos.Chandelier, Photos.RoomLiving],
            ["Haram-view rooms (select)", "Breakfast buffet", "Prayer area", "Room service", "Concierge", "Laundry service"], true, true),

        new("safa-grand-residence", "Safa Grand Residence", City.Makkah, 5, 400, 72_000m,
            "Al Safa Street, Makkah",
            "Spacious five-star suites ideal for families, close to the Haram's main gates.",
            "Safa Grand Residence offers generously sized rooms and family suites with kitchenettes, making longer stays comfortable for families and groups. A 24-hour restaurant serves Pakistani, Arabic and continental dishes, and a dedicated Umrah desk is on hand to help with schedules and transport.",
            Photos.LobbyReception, [Photos.KaabaClockTower, Photos.RoomLiving, Photos.RoomSofa, Photos.LobbyLighting],
            ["Family suites", "24-hour restaurant", "Kitchenette (suites)", "Umrah help desk", "Laundry service"], true, true),

        new("zamzam-pearl-hotel", "Zamzam Pearl Hotel", City.Makkah, 4, 650, 42_000m,
            "Ajyad Al Masafi, Makkah",
            "Comfortable four-star hotel with modern rooms and easy walking access to the Haram.",
            "Zamzam Pearl Hotel balances comfort and value with modern, well-kept rooms and a calm atmosphere. The hotel is a popular choice for small groups, offering triple and quad rooms, a daily breakfast buffet and help arranging taxis and ziyarat.",
            Photos.LobbySleek, [Photos.KaabaCourtyard, Photos.RoomBed, Photos.RoomGrayPillow],
            ["Breakfast buffet", "Prayer area", "Luggage storage", "Laundry service"], true, false),

        new("ajyad-comfort-suites", "Ajyad Comfort Suites", City.Makkah, 4, 900, 34_000m,
            "Ajyad Street, Makkah",
            "Practical suites with family-friendly layouts and a complimentary Haram shuttle at prayer times.",
            "Ajyad Comfort Suites is a practical base for pilgrims who want extra space. Rooms come in double, triple and quad configurations, and a complimentary shuttle runs to the Haram around the five daily prayers.",
            Photos.LobbyArched, [Photos.KaabaArch, Photos.RoomWoodBed, Photos.RoomBed2],
            ["Haram shuttle (prayer times)", "Breakfast available", "Family rooms", "Luggage storage"], false, false),

        new("misfalah-garden-inn", "Misfalah Garden Inn", City.Makkah, 3, 1_400, 19_500m,
            "Misfalah District, Makkah",
            "Clean, budget-friendly three-star rooms with a regular shuttle to Masjid al-Haram.",
            "Misfalah Garden Inn offers simple, clean rooms at an accessible price. It suits budget-conscious pilgrims and large groups, with quad rooms available and a regular shuttle service to the Haram.",
            Photos.Hallway, [Photos.KaabaSun, Photos.RoomWhiteSheets, Photos.RoomCurtains],
            ["Haram shuttle", "Breakfast available", "Group check-in desk"], false, false),

        new("aziziyah-family-hotel", "Aziziyah Family Hotel", City.Makkah, 3, 4_500, 12_500m,
            "Al Aziziyah, Makkah",
            "Value hotel in Aziziyah with spacious family rooms and scheduled transport to the Haram.",
            "Located in the quieter Aziziyah district, this value hotel offers larger rooms at lower rates, with scheduled bus transport to the Haram. A good fit for economy packages and longer stays.",
            Photos.LobbyLounge, [Photos.KaabaPilgrims, Photos.RoomGrayPillow, Photos.RoomWood],
            ["Scheduled Haram transport", "Family rooms", "Grocery nearby"], false, false),

        // ---------------- Madinah ----------------
        new("al-rawdah-crown-hotel", "Al Rawdah Crown Hotel", City.Madinah, 5, 150, 68_000m,
            "King Faisal Road, Central Area, Madinah",
            "Five-star comfort steps from Masjid an-Nabawi, with elegant rooms and fine dining.",
            "Al Rawdah Crown Hotel places guests within easy walking distance of Masjid an-Nabawi's northern gates. Rooms feature calm, elegant interiors, and the hotel offers an all-day restaurant, a quiet lounge, and a concierge who can arrange ziyarat to Quba, Uhud and other historic sites.",
            Photos.Chandelier, [Photos.NabawiSunset, Photos.RoomWhiteLinen, Photos.LobbyMarbleGold, Photos.RoomLiving],
            ["Breakfast buffet", "Room service", "Concierge", "Ziyarat desk", "Laundry service"], true, true),

        new("bab-al-salam-residence", "Bab Al Salam Residence", City.Madinah, 5, 300, 58_000m,
            "Bab Al Salam Street, Madinah",
            "Upscale residence near Bab al-Salam with family suites and a rooftop dining area.",
            "Bab Al Salam Residence offers upscale rooms and family suites close to the Prophet's Mosque. Guests enjoy a rooftop dining area, a prayer room and multilingual staff experienced in hosting pilgrim groups.",
            Photos.LobbyMarblePlants, [Photos.NabawiGreenDome, Photos.RoomSofa, Photos.RoomBed],
            ["Family suites", "Rooftop dining", "Prayer room", "Laundry service"], true, true),

        new("quba-light-hotel", "Quba Light Hotel", City.Madinah, 4, 500, 33_000m,
            "Abu Dhar Al Ghifari Street, Madinah",
            "Bright, modern four-star rooms within walking distance of the Haram.",
            "Quba Light Hotel offers bright, modern rooms and a friendly atmosphere. Triple and quad rooms make it popular with families and small groups, and a daily breakfast buffet is served in the lobby restaurant.",
            Photos.LobbyWood, [Photos.NabawiDomeSky, Photos.RoomWoodBed, Photos.RoomBed2],
            ["Breakfast buffet", "Family rooms", "Luggage storage"], true, false),

        new("al-ansar-plaza-hotel", "Al Ansar Plaza Hotel", City.Madinah, 4, 700, 27_500m,
            "Al Ansar Road, Madinah",
            "Well-located four-star hotel with comfortable rooms and helpful group services.",
            "Al Ansar Plaza Hotel is a dependable four-star choice with comfortable rooms, an efficient group check-in desk, and easy access to shops and restaurants around the Central Area.",
            Photos.LobbyLighting, [Photos.NabawiMinarets, Photos.RoomCurtains, Photos.RoomGrayPillow],
            ["Breakfast available", "Group check-in desk", "Laundry service"], false, false),

        new("green-dome-inn", "Green Dome Inn", City.Madinah, 3, 1_100, 16_000m,
            "Sultana Road, Madinah",
            "Simple, comfortable three-star rooms at a friendly price, a short ride from the Haram.",
            "Green Dome Inn provides simple, comfortable accommodation for pilgrims on a budget, with quad rooms for groups and regular transport to Masjid an-Nabawi.",
            Photos.LobbyArched, [Photos.NabawiUmbrellas, Photos.RoomWhiteSheets, Photos.RoomWood],
            ["Haram transport", "Breakfast available"], false, false),

        new("al-aqiq-budget-stay", "Al Aqiq Budget Stay", City.Madinah, 3, 2_500, 10_500m,
            "Al Aqiq District, Madinah",
            "Economical rooms in a quiet district with scheduled transport to Masjid an-Nabawi.",
            "Al Aqiq Budget Stay is an economical option in a quiet residential district, suited to economy packages, with scheduled transport to the Haram and spacious quad rooms.",
            Photos.Hallway, [Photos.NabawiClearSky, Photos.RoomBed, Photos.RoomSofa],
            ["Scheduled Haram transport", "Family rooms"], false, false),
    ];

    public static (List<Hotel> Hotels, List<Package> Packages) Build()
    {
        var hotels = HotelSeeds.Select(BuildHotel).ToList();
        var bySlug = hotels.ToDictionary(h => h.Slug);
        var packages = BuildPackages(bySlug);
        return (hotels, packages);
    }

    private static Hotel BuildHotel(HotelSeed s)
    {
        var hotel = new Hotel
        {
            Slug = s.Slug,
            Name = s.Name,
            City = s.City,
            StarRating = s.Stars,
            DistanceFromHolyMosqueMeters = s.DistanceMeters,
            IsDistanceVerified = false,
            PricePerNight = s.Price,
            Currency = "PKR",
            Address = s.Address,
            ShortDescription = s.Short,
            Description = s.Description,
            ImageUrl = Img(s.MainPhoto),
            Amenities = [.. CommonAmenities, .. s.Amenities],
            IsFeatured = s.Featured,
            IsActive = true,
            IsDemoContent = true,
            Images = [.. new[] { s.MainPhoto }.Concat(s.Gallery)
                .Select((p, i) => new HotelImage { Url = Img(p), SortOrder = i, Caption = i == 0 ? s.Name : null })],
        };

        hotel.Rooms =
        [
            Room("Double Room", 2, "1 king or 2 single beds", s.Price, Photos.RoomWhiteLinen,
                "Comfortable room for two guests with en-suite bathroom."),
            Room("Triple Room", 3, "3 single beds", Round(s.Price * 1.25m), Photos.RoomBed,
                "Ideal for small families or friends travelling together."),
            Room("Quad Room", 4, "4 single beds", Round(s.Price * 1.45m), Photos.RoomBed2,
                "Popular with groups and families; four single beds."),
        ];
        if (s.HasSuite)
        {
            hotel.Rooms.Add(Room("Family Suite", 6, "1 king bed, 4 single beds, sitting area", Round(s.Price * 2.4m), Photos.RoomLiving,
                "Separate sitting area and extra space for larger families."));
        }
        return hotel;
    }

    private static Room Room(string name, int capacity, string beds, decimal price, string photo, string description) => new()
    {
        Name = name,
        Capacity = capacity,
        BedConfiguration = beds,
        PricePerNight = price,
        ImageUrl = Img(photo, 1200),
        Description = description,
    };

    private static decimal Round(decimal value) => Math.Round(value / 500m, MidpointRounding.AwayFromZero) * 500m;

    private static readonly (string Name, string? Description)[] CoreServices =
    [
        ("Umrah visa processing", "Visa application handled by our team."),
        ("Airport transfers", "Transfers between Jeddah/Madinah airport and your hotel."),
        ("Makkah–Madinah transport", "Intercity transport by air-conditioned coach."),
    ];

    private static List<Package> BuildPackages(Dictionary<string, Hotel> h) =>
    [
        Package("economy-15-day-umrah", "Economy 15-Day Umrah", PackageType.Economy, 15, 10, 4, 285_000m,
            "An affordable 15-day Umrah with comfortable value hotels and scheduled transport to both Harams.",
            "Our economy package is designed for pilgrims who want a complete, well-organised Umrah at an accessible price. You will spend 10 nights in Makkah and 4 nights in Madinah in clean, value-focused hotels with scheduled transport to the Haram. Visa, transfers and intercity transport are included.",
            Photos.KaabaPilgrims, [Photos.KaabaPilgrims, Photos.NabawiClearSky],
            [(h["aziziyah-family-hotel"], 10), (h["al-aqiq-budget-stay"], 4)],
            ["Return economy airfare", "Shared quad-room accommodation"], featured: true),

        Package("standard-21-day-umrah", "Standard 21-Day Umrah", PackageType.Standard, 21, 14, 6, 345_000m,
            "Three weeks for unhurried worship, with comfortable three-star hotels and full ground arrangements.",
            "Take your time with a 21-day journey: 14 nights in Makkah and 6 nights in Madinah. This package suits pilgrims who want more time for worship and reflection, with comfortable hotels and guided ziyarat in both cities.",
            Photos.KaabaSun, [Photos.KaabaSun, Photos.NabawiUmbrellas],
            [(h["misfalah-garden-inn"], 14), (h["green-dome-inn"], 6)],
            ["Return economy airfare", "Ziyarat tours in Makkah & Madinah", "Daily breakfast"], featured: true),

        Package("comfort-14-day-umrah", "Comfort 14-Day Umrah", PackageType.Standard, 14, 9, 4, 395_000m,
            "Balanced two-week package with four-star hotels and family-friendly room options.",
            "A balanced two-week package with 9 nights in Makkah and 4 nights in Madinah in four-star hotels. Triple and quad rooms are available for families, and the Makkah hotel offers a shuttle to the Haram at prayer times.",
            Photos.KaabaArch, [Photos.KaabaArch, Photos.NabawiMinarets],
            [(h["ajyad-comfort-suites"], 9), (h["al-ansar-plaza-hotel"], 4)],
            ["Return economy airfare", "Daily breakfast", "Ziyarat tours in Makkah & Madinah"], featured: false),

        Package("premium-10-day-umrah", "Premium 10-Day Umrah", PackageType.Premium, 10, 6, 3, 545_000m,
            "A shorter premium itinerary with four-star hotels within walking distance of both Harams.",
            "For pilgrims with limited time, this premium itinerary offers 6 nights in Makkah and 3 nights in Madinah in four-star hotels within walking distance of the Harams, with breakfast daily and private airport transfers.",
            Photos.KaabaCourtyard, [Photos.KaabaCourtyard, Photos.NabawiDomeSky],
            [(h["zamzam-pearl-hotel"], 6), (h["quba-light-hotel"], 3)],
            ["Return economy airfare", "Daily breakfast", "Private airport transfers", "Ziyarat tours in Makkah & Madinah"], featured: true),

        Package("luxury-12-day-haram-view-umrah", "Luxury 12-Day Haram View Umrah", PackageType.Luxury, 12, 7, 4, 895_000m,
            "Five-star hotels steps from both Harams, Haram-view rooms in Makkah (subject to availability) and private transfers.",
            "Our most comfortable itinerary: 7 nights in Makkah and 4 nights in Madinah in five-star hotels moments from the Harams. Enjoy Haram-view rooms in Makkah where available, daily breakfast buffets, private transfers and a dedicated group coordinator.",
            Photos.KaabaTowers, [Photos.KaabaTowers, Photos.NabawiSunset, Photos.Chandelier],
            [(h["al-noor-haram-view-hotel"], 7), (h["al-rawdah-crown-hotel"], 4)],
            ["Return economy airfare", "Daily breakfast buffet", "Private airport & intercity transfers", "Dedicated group coordinator", "Ziyarat tours in Makkah & Madinah"], featured: true),

        Package("ramadan-last-ten-nights-umrah", "Ramadan Last Ten Nights Umrah", PackageType.Premium, 12, 8, 3, 725_000m,
            "Spend the blessed last ten nights of Ramadan in Makkah, with suhoor and iftar arrangements.",
            "Experience the last ten nights of Ramadan in Makkah with 8 nights at a five-star residence near the Haram, followed by 3 nights in Madinah. Suhoor and iftar arrangements are included. Dates follow the Islamic calendar and are confirmed on inquiry.",
            Photos.KaabaCrowdTower, [Photos.KaabaCrowdTower, Photos.NabawiGreenDome],
            [(h["safa-grand-residence"], 8), (h["bab-al-salam-residence"], 3)],
            ["Return economy airfare", "Suhoor & iftar meals", "Ziyarat tours in Makkah & Madinah"], featured: true),

        Package("short-7-day-umrah", "Short 7-Day Umrah", PackageType.Standard, 7, 4, 2, 265_000m,
            "A compact one-week Umrah for working professionals, with all ground arrangements handled.",
            "Ideal for those who can only take a week off: 4 nights in Makkah and 2 nights in Madinah with visa, transfers and intercity transport included.",
            Photos.KaabaWhite, [Photos.KaabaWhite, Photos.NabawiBlueSky],
            [(h["misfalah-garden-inn"], 4), (h["green-dome-inn"], 2)],
            ["Return economy airfare"], featured: false),
    ];

    private static Package Package(
        string slug, string name, PackageType type, int days, int makkah, int madinah, decimal price,
        string shortDescription, string description, string mainPhoto, string[] gallery,
        (Hotel Hotel, int Nights)[] stays, string[] extraServices, bool featured) => new()
    {
        Slug = slug,
        Name = name,
        PackageType = type,
        DurationInDays = days,
        MakkahDays = makkah,
        MadinahDays = madinah,
        Price = price,
        Currency = "PKR",
        ShortDescription = shortDescription,
        Description = description,
        ImageUrl = Img(mainPhoto),
        IsFeatured = featured,
        IsActive = true,
        IsDemoContent = true,
        Images = [.. gallery.Select((p, i) => new PackageImage { Url = Img(p), SortOrder = i })],
        Hotels = [.. stays.Select(s => new PackageHotel { Hotel = s.Hotel, City = s.Hotel.City, Nights = s.Nights })],
        Services = [.. CoreServices.Concat(extraServices.Select(e => (Name: e, Description: (string?)null)))
            .Select((s, i) => new PackageServiceEntity { ServiceName = s.Name, Description = s.Description, SortOrder = i })],
    };
}
