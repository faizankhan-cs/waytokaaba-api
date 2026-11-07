using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Waytokaaba.Application.Common;
using Waytokaaba.Application.Hotels;
using Waytokaaba.Application.Inquiries;
using Waytokaaba.Application.Packages;
using Waytokaaba.Core.Enums;

namespace Waytokaaba.Tests;

[Collection(ApiCollection.Name)]
public sealed class PackagesApiTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Filters_by_type_and_duration_and_sorts_by_price()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<PackageSummaryDto>>(
            "/api/packages?type=Premium&type=Luxury&maxDays=12&sort=price_asc", ApiFactory.Json);

        result!.Items.Should().NotBeEmpty()
            .And.OnlyContain(p => (p.PackageType == PackageType.Premium || p.PackageType == PackageType.Luxury) && p.DurationInDays <= 12);
        result.Items.Select(p => p.Price).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Search_by_name()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<PackageSummaryDto>>("/api/packages/search?q=ramadan", ApiFactory.Json);

        result!.Items.Should().ContainSingle().Which.Slug.Should().Be("ramadan-last-ten-nights-umrah");
    }

    [Fact]
    public async Task Detail_includes_hotels_services_and_images()
    {
        var package = await _client.GetFromJsonAsync<PackageDetailDto>("/api/packages/luxury-12-day-haram-view-umrah", ApiFactory.Json);

        package!.Hotels.Should().HaveCount(2);
        package.Hotels[0].City.Should().Be(City.Makkah, "Makkah stays are listed first");
        package.Services.Should().NotBeEmpty();
        package.Images.Should().NotBeEmpty();
        (package.MakkahDays + package.MadinahDays).Should().BeLessThanOrEqualTo(package.DurationInDays);
    }

    [Fact]
    public async Task Featured_returns_only_featured_packages()
    {
        var featured = await _client.GetFromJsonAsync<List<PackageSummaryDto>>("/api/packages/featured", ApiFactory.Json);

        featured.Should().NotBeEmpty().And.OnlyContain(p => p.IsFeatured);
    }
}

[Collection(ApiCollection.Name)]
public sealed class InquiriesApiTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Valid_hotel_inquiry_is_saved_and_visible_to_admin()
    {
        var hotel = await _client.GetFromJsonAsync<HotelDetailDto>("/api/hotels/zamzam-pearl-hotel", ApiFactory.Json);
        var email = $"guest-{Guid.NewGuid():N}@example.com";

        var response = await _client.PostAsJsonAsync("/api/inquiries", new
        {
            customerName = "Test Guest",
            email,
            phone = "+92 300 1234567",
            inquiryType = "Hotel",
            hotelId = hotel!.Id,
            checkIn = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            checkOut = DateTime.UtcNow.AddDays(35).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            guests = 3,
            message = "Please share availability for a triple room.",
        });
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var admin = await factory.CreateAdminClientAsync();
        var list = await admin.GetFromJsonAsync<PagedResult<InquiryDto>>("/api/inquiries?pageSize=50", ApiFactory.Json);
        list!.Items.Should().ContainSingle(i => i.Email == email)
            .Which.Should().Match<InquiryDto>(i => i.HotelName == hotel.Name && i.Status == InquiryStatus.New);
    }

    [Fact]
    public async Task Invalid_inquiry_returns_400_with_field_errors()
    {
        var response = await _client.PostAsJsonAsync("/api/inquiries", new
        {
            customerName = "",
            email = "not-an-email",
            phone = "12",
            inquiryType = "Package",
            message = "short",
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("customerName").And.Contain("email").And.Contain("phone").And.Contain("packageId");
    }

    [Fact]
    public async Task Inquiry_for_unknown_package_returns_400()
    {
        var response = await _client.PostAsJsonAsync("/api/inquiries", new
        {
            customerName = "Test", email = "t@example.com", phone = "+923001234567",
            inquiryType = "Package", packageId = 99999, message = "Interested in this package.",
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
