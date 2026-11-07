using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Waytokaaba.Application.Hotels;
using Waytokaaba.Application.Packages;

namespace Waytokaaba.Tests;

[Collection(ApiCollection.Name)]
public sealed class AdminApiTests(ApiFactory factory)
{
    private static object NewHotel(string name) => new
    {
        name,
        shortDescription = "Integration test hotel",
        description = "Created by an integration test.",
        city = "Madinah",
        address = "Test Street",
        starRating = 4,
        imageUrl = "https://images.unsplash.com/photo-1621293954908-907159247fc8",
        pricePerNight = 25000,
        currency = "PKR",
        amenities = new[] { "Free Wi-Fi" },
        isActive = true,
        images = Array.Empty<object>(),
    };

    [Theory]
    [InlineData("POST", "/api/hotels")]
    [InlineData("PUT", "/api/hotels/1")]
    [InlineData("DELETE", "/api/hotels/1")]
    [InlineData("POST", "/api/packages")]
    [InlineData("DELETE", "/api/packages/1")]
    [InlineData("POST", "/api/hotels/1/rooms")]
    [InlineData("GET", "/api/inquiries")]
    [InlineData("PATCH", "/api/inquiries/1/status")]
    [InlineData("GET", "/api/admin/hotels")]
    [InlineData("GET", "/api/admin/packages")]
    public async Task Admin_endpoints_require_authentication(string method, string url)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), url) { Content = JsonContent.Create(new { }) };

        var response = await factory.CreateClient().SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_with_wrong_password_returns_401()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/login",
            new { email = ApiFactory.AdminEmail, password = "wrong-password" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Admin_can_create_update_add_room_and_delete_a_hotel()
    {
        var admin = await factory.CreateAdminClientAsync();
        var name = $"Test Hotel {Guid.NewGuid():N}"[..24];

        var created = await admin.PostAsJsonAsync("/api/hotels", NewHotel(name));
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var hotel = (await created.Content.ReadFromJsonAsync<HotelDetailDto>(ApiFactory.Json))!;
        hotel.Slug.Should().NotBeNullOrEmpty();

        var room = await admin.PostAsJsonAsync($"/api/hotels/{hotel.Id}/rooms",
            new { name = "Quint Room", capacity = 5, pricePerNight = 40000 });
        room.StatusCode.Should().Be(HttpStatusCode.Created);

        var updated = await admin.PutAsJsonAsync($"/api/hotels/{hotel.Id}", NewHotel(name + " Updated"));
        updated.StatusCode.Should().Be(HttpStatusCode.OK);
        (await updated.Content.ReadFromJsonAsync<HotelDetailDto>(ApiFactory.Json))!.Rooms.Should().ContainSingle();

        (await admin.DeleteAsync($"/api/hotels/{hotel.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await admin.GetAsync($"/api/hotels/{hotel.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Hotel_used_by_a_package_cannot_be_deleted()
    {
        var admin = await factory.CreateAdminClientAsync();
        var package = await admin.GetFromJsonAsync<PackageDetailDto>("/api/packages/economy-15-day-umrah", ApiFactory.Json);

        var response = await admin.DeleteAsync($"/api/hotels/{package!.Hotels[0].HotelId}");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Invalid_hotel_payload_returns_400()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsJsonAsync("/api/hotels", new { name = "", starRating = 9, pricePerNight = -1 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Admin_can_create_a_package_with_hotels_and_services()
    {
        var admin = await factory.CreateAdminClientAsync();
        var makkah = await admin.GetFromJsonAsync<HotelDetailDto>("/api/hotels/zamzam-pearl-hotel", ApiFactory.Json);

        var response = await admin.PostAsJsonAsync("/api/packages", new
        {
            name = $"Test Package {Guid.NewGuid():N}"[..26],
            shortDescription = "Test",
            description = "Test package",
            packageType = "Standard",
            durationInDays = 10,
            makkahDays = 6,
            madinahDays = 3,
            price = 300000,
            currency = "PKR",
            imageUrl = "https://images.unsplash.com/photo-1580418827493-f2b22c0a76cb",
            hotels = new[] { new { hotelId = makkah!.Id, nights = 6 } },
            services = new[] { new { serviceName = "Visa", description = (string?)null } },
            images = Array.Empty<object>(),
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var package = (await response.Content.ReadFromJsonAsync<PackageDetailDto>(ApiFactory.Json))!;
        package.Hotels.Should().ContainSingle(h => h.HotelId == makkah.Id);
        package.Services.Should().ContainSingle();

        (await admin.DeleteAsync($"/api/packages/{package.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
