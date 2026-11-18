using Waytokaaba.Core.Entities;

namespace Waytokaaba.Infrastructure.Persistence.Seed;

/// <summary>
/// Demonstration private-transport vehicles, routes and prices. Prices are a supplier's 1448H rate sheet in SAR,
/// converted to PKR at about 73.76 per SAR and rounded to the nearest 100. They are INDICATIVE placeholders and
/// every record is flagged <c>IsDemoContent</c>; the agency sets its own prices in the admin area.
/// Photos are free Unsplash images of similar vehicle types, to be replaced with the agency's own photos.
/// </summary>
internal static class DemoTransport
{
    private const decimal PkrPerSar = 73.76m;

    private static decimal Pkr(int sar) => Math.Round(sar * PkrPerSar / 100m, MidpointRounding.AwayFromZero) * 100m;

    private static string Photo(string id) => $"https://images.unsplash.com/photo-{id}?auto=format&fit=crop";

    public static (List<TransportVehicle> Vehicles, List<TransportRoute> Routes) Build()
    {
        List<TransportVehicle> vehicles =
        [
            Vehicle("Sedan", Photo("1780263274133-74580def6296"), seats: 4, bags: 2, order: 1,
                "Comfortable sedan for small families and couples."),
            Vehicle("H1-Staria", Photo("1776774853502-11e7a46252d8"), seats: 7, bags: 5, order: 2,
                "Spacious family van with room for luggage."),
            Vehicle("Hi-Ace", Photo("1776521906834-45551a2004ac"), seats: 10, bags: 8, order: 3,
                "Ideal for larger families and small groups."),
            Vehicle("GMC", Photo("1767749995474-cfb164d4d9ef"), seats: 7, bags: 5, order: 4,
                "Premium SUV for a luxurious, private ride."),
            Vehicle("Coaster", Photo("1534011056808-50c1c6082fe7"), seats: 25, bags: 20, order: 5,
                "Mini coach for groups travelling together."),
        ];

        // SAR prices per vehicle, in the order above: Sedan, H1-Staria, Hi-Ace, GMC, Coaster.
        (string From, string? To, string? Note, int[] Sar)[] rows =
        [
            ("Jeddah", "Makkah", null, [260, 290, 340, 400, 500]),
            ("Makkah", "Madinah", null, [390, 440, 550, 800, 850]),
            ("Madinah", "Makkah", null, [390, 440, 550, 800, 850]),
            ("Makkah", "Jeddah", null, [220, 250, 320, 350, 500]),
            ("Jeddah", "Madinah", null, [400, 500, 600, 850, 900]),
            ("Madinah", "Jeddah", null, [400, 500, 600, 800, 900]),
            ("Madinah Ziyarat", null, "Visit the holy sites around Madinah.", [200, 250, 300, 400, 350]),
            ("Makkah Ziyarat", null, "Visit the holy sites around Makkah.", [200, 250, 320, 400, 350]),
            ("Madinah Airport", "Madinah Hotel", null, [190, 200, 300, 330, 330]),
            ("Madinah Hotel", "Madinah Airport", null, [160, 170, 220, 300, 330]),
            ("Train Station", "Hotel", "Haramain train station pick-up.", [150, 160, 230, 300, 290]),
            ("Hotel", "Train Station", "Drop-off at the Haramain train station.", [150, 160, 230, 300, 290]),
        ];

        var routes = rows.Select((r, i) => new TransportRoute
        {
            From = r.From,
            To = r.To,
            Note = r.Note,
            SortOrder = i + 1,
            IsDemoContent = true,
            Rates = r.Sar.Select((sar, v) => new TransportRate { Vehicle = vehicles[v], Price = Pkr(sar) }).ToList(),
        }).ToList();

        return (vehicles, routes);
    }

    private static TransportVehicle Vehicle(string name, string image, int seats, int bags, int order, string description) => new()
    {
        Name = name,
        ImageUrl = image,
        Seats = seats,
        Bags = bags,
        Description = description,
        SortOrder = order,
        IsDemoContent = true,
    };
}
