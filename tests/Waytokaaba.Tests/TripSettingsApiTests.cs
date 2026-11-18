using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Waytokaaba.Application.Trips;

namespace Waytokaaba.Tests;

[Collection(ApiCollection.Name)]
public sealed class TripSettingsApiTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Settings_are_public_and_seeded()
    {
        var settings = await _client.GetFromJsonAsync<TripSettingsDto>("/api/trip-settings", ApiFactory.Json);

        settings!.Currency.Should().Be("PKR");
        settings.VisaTransportPricePerPerson.Should().BePositive();
    }

    [Fact]
    public async Task Admin_can_update_the_visa_and_transport_price()
    {
        var admin = await factory.CreateAdminClientAsync();
        var original = (await _client.GetFromJsonAsync<TripSettingsDto>("/api/trip-settings", ApiFactory.Json))!;

        try
        {
            var response = await admin.PutAsJsonAsync("/api/trip-settings", new UpdateTripSettingsRequest(72_500m), ApiFactory.Json);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var after = await _client.GetFromJsonAsync<TripSettingsDto>("/api/trip-settings", ApiFactory.Json);
            after!.VisaTransportPricePerPerson.Should().Be(72_500m);
        }
        finally
        {
            await admin.PutAsJsonAsync("/api/trip-settings", new UpdateTripSettingsRequest(original.VisaTransportPricePerPerson), ApiFactory.Json);
        }
    }

    [Fact]
    public async Task Anonymous_cannot_change_settings()
    {
        var response = await _client.PutAsJsonAsync("/api/trip-settings", new UpdateTripSettingsRequest(1m), ApiFactory.Json);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Price_must_be_positive()
    {
        var admin = await factory.CreateAdminClientAsync();
        var response = await admin.PutAsJsonAsync("/api/trip-settings", new UpdateTripSettingsRequest(0m), ApiFactory.Json);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
