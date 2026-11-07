using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Waytokaaba.Application.Common;
using Waytokaaba.Application.Hotels;
using Waytokaaba.Core.Enums;

namespace Waytokaaba.Tests;

[Collection(ApiCollection.Name)]
public sealed class HotelsApiTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private Task<PagedResult<HotelSummaryDto>?> Search(string query) =>
        _client.GetFromJsonAsync<PagedResult<HotelSummaryDto>>($"/api/hotels/search?{query}", ApiFactory.Json);

    [Fact]
    public async Task Search_by_city_returns_only_that_city()
    {
        var result = await Search("city=Madinah&pageSize=50");

        result!.Items.Should().NotBeEmpty()
            .And.OnlyContain(h => h.City == City.Madinah);
    }

    [Fact]
    public async Task Search_by_guests_returns_hotels_with_a_room_that_fits()
    {
        var result = await Search("city=Makkah&guests=5&pageSize=50");

        result!.Items.Should().NotBeEmpty()
            .And.OnlyContain(h => h.MaxRoomCapacity >= 5);
    }

    [Fact]
    public async Task Search_filters_by_price_range_and_stars()
    {
        var result = await Search("minPrice=20000&maxPrice=50000&stars=4&pageSize=50");

        result!.Items.Should().NotBeEmpty()
            .And.OnlyContain(h => h.PricePerNight >= 20000 && h.PricePerNight <= 50000 && h.StarRating == 4);
    }

    [Fact]
    public async Task Search_by_name_is_case_insensitive()
    {
        var result = await Search("q=zAmZaM");

        result!.Items.Should().ContainSingle(h => h.Slug == "zamzam-pearl-hotel");
    }

    [Theory]
    [InlineData("price_asc")]
    [InlineData("price_desc")]
    public async Task Search_sorts_by_price(string sort)
    {
        var result = await Search($"sort={sort}&pageSize=50");
        var prices = result!.Items.Select(h => h.PricePerNight).ToList();

        if (sort == "price_asc") prices.Should().BeInAscendingOrder();
        else prices.Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task Search_paginates()
    {
        var result = await Search("pageSize=5&page=2");

        result!.Page.Should().Be(2);
        result.Items.Should().HaveCount(5);
        result.TotalCount.Should().BeGreaterThanOrEqualTo(12);
    }

    [Fact]
    public async Task Search_with_invalid_parameters_returns_400_with_errors()
    {
        var response = await _client.GetAsync("/api/hotels/search?minPrice=500&maxPrice=100&sort=nope");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("maxPrice").And.Contain("sort");
    }

    [Fact]
    public async Task City_route_works_and_rejects_unknown_cities()
    {
        var ok = await _client.GetFromJsonAsync<PagedResult<HotelSummaryDto>>("/api/hotels/city/makkah", ApiFactory.Json);
        ok!.Items.Should().OnlyContain(h => h.City == City.Makkah);

        (await _client.GetAsync("/api/hotels/city/jeddah")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Detail_by_slug_includes_rooms_and_gallery_and_hides_unverified_distance()
    {
        var hotel = await _client.GetFromJsonAsync<HotelDetailDto>("/api/hotels/al-noor-haram-view-hotel", ApiFactory.Json);

        hotel!.Rooms.Should().NotBeEmpty();
        hotel.Images.Should().NotBeEmpty();
        hotel.IsDistanceVerified.Should().BeFalse();
        hotel.DistanceFromHolyMosqueMeters.Should().BeNull("unverified distances must not be shown publicly");
    }

    [Fact]
    public async Task Unknown_hotel_returns_404_problem_details()
    {
        var response = await _client.GetAsync("/api/hotels/does-not-exist");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task Rooms_endpoint_returns_rooms_for_hotel()
    {
        var hotel = await _client.GetFromJsonAsync<HotelDetailDto>("/api/hotels/quba-light-hotel", ApiFactory.Json);
        var rooms = await _client.GetFromJsonAsync<List<RoomDto>>($"/api/hotels/{hotel!.Id}/rooms", ApiFactory.Json);

        rooms.Should().NotBeEmpty().And.OnlyContain(r => r.HotelId == hotel.Id);
    }
}
