using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Waytokaaba.Application.Transport;

namespace Waytokaaba.Tests;

[Collection(ApiCollection.Name)]
public sealed class TransportApiTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private static UpsertTransportVehicleRequest NewVehicle(string name, bool active = true) => new()
    {
        Name = name,
        ImageUrl = "https://example.com/van.jpg",
        Seats = 7,
        Bags = 4,
        Description = "A test vehicle.",
        SortOrder = 99,
        IsActive = active,
    };

    private static UpsertTransportRouteRequest NewRoute(string from, params (int VehicleId, decimal Price)[] prices) => new()
    {
        From = from,
        To = "Test Destination",
        SortOrder = 99,
        Prices = prices.Select(p => new TransportRatePriceRequest(p.VehicleId, p.Price)).ToList(),
    };

    private async Task<TransportCatalogDto> PublicCatalogAsync() =>
        (await _client.GetFromJsonAsync<TransportCatalogDto>("/api/transport", ApiFactory.Json))!;

    [Fact]
    public async Task Catalog_lists_seeded_vehicles_and_routes_with_prices()
    {
        var catalog = await PublicCatalogAsync();

        catalog.Currency.Should().Be("PKR");
        catalog.Vehicles.Select(v => v.Name).Should().Contain(["Sedan", "H1-Staria", "Hi-Ace", "GMC", "Coaster"]);
        catalog.Vehicles.Should().OnlyContain(v => v.IsActive);
        catalog.Routes.Should().HaveCountGreaterThanOrEqualTo(12).And.OnlyContain(r => r.Rates.Count > 0);

        // 260 SAR at ~73.76 PKR/SAR, rounded to the nearest 100.
        var sedan = catalog.Vehicles.Single(v => v.Name == "Sedan");
        var jeddahMakkah = catalog.Routes.Single(r => r.From == "Jeddah" && r.To == "Makkah");
        jeddahMakkah.Rates.Single(r => r.VehicleId == sedan.Id).Price.Should().Be(19_200m);
        sedan.StartingPrice.Should().Be(catalog.Routes.SelectMany(r => r.Rates).Where(r => r.VehicleId == sedan.Id).Min(r => r.Price));
    }

    [Fact]
    public async Task Anonymous_cannot_change_transport()
    {
        var vehicle = await _client.PostAsJsonAsync("/api/transport/vehicles", NewVehicle("Nope"), ApiFactory.Json);
        var route = await _client.DeleteAsync("/api/transport/routes/1");

        vehicle.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        route.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Admin_can_create_update_and_delete_a_vehicle()
    {
        var admin = await factory.CreateAdminClientAsync();

        var created = await admin.PostAsJsonAsync("/api/transport/vehicles", NewVehicle("Test Van"), ApiFactory.Json);
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var vehicle = (await created.Content.ReadFromJsonAsync<TransportVehicleDto>(ApiFactory.Json))!;
        vehicle.Name.Should().Be("Test Van");
        vehicle.StartingPrice.Should().BeNull();

        var update = NewVehicle("Test Van XL");
        update.Seats = 12;
        var updated = await admin.PutAsJsonAsync($"/api/transport/vehicles/{vehicle.Id}", update, ApiFactory.Json);
        updated.StatusCode.Should().Be(HttpStatusCode.OK);
        (await updated.Content.ReadFromJsonAsync<TransportVehicleDto>(ApiFactory.Json))!.Seats.Should().Be(12);

        (await admin.DeleteAsync($"/api/transport/vehicles/{vehicle.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await admin.GetAsync($"/api/admin/transport/vehicles/{vehicle.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Admin_can_create_update_and_delete_a_route_with_prices()
    {
        var admin = await factory.CreateAdminClientAsync();
        var sedan = (await PublicCatalogAsync()).Vehicles.Single(v => v.Name == "Sedan");

        var created = await admin.PostAsJsonAsync("/api/transport/routes", NewRoute("Test Origin", (sedan.Id, 10_000m)), ApiFactory.Json);
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var route = (await created.Content.ReadFromJsonAsync<TransportRouteDto>(ApiFactory.Json))!;
        route.Rates.Should().ContainSingle(r => r.VehicleId == sedan.Id && r.Price == 10_000m);

        (await PublicCatalogAsync()).Routes.Should().Contain(r => r.Id == route.Id);

        var updated = await admin.PutAsJsonAsync($"/api/transport/routes/{route.Id}", NewRoute("Test Origin", (sedan.Id, 12_500m)), ApiFactory.Json);
        updated.StatusCode.Should().Be(HttpStatusCode.OK);
        (await updated.Content.ReadFromJsonAsync<TransportRouteDto>(ApiFactory.Json))!
            .Rates.Should().ContainSingle(r => r.Price == 12_500m);

        (await admin.DeleteAsync($"/api/transport/routes/{route.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await PublicCatalogAsync()).Routes.Should().NotContain(r => r.Id == route.Id);
    }

    [Fact]
    public async Task Inactive_vehicle_and_its_prices_are_hidden_publicly_but_shown_to_admin()
    {
        var admin = await factory.CreateAdminClientAsync();
        var hidden = (await (await admin.PostAsJsonAsync("/api/transport/vehicles", NewVehicle("Hidden Van", active: false), ApiFactory.Json))
            .Content.ReadFromJsonAsync<TransportVehicleDto>(ApiFactory.Json))!;
        var route = (await (await admin.PostAsJsonAsync("/api/transport/routes", NewRoute("Hidden Only Route", (hidden.Id, 5_000m)), ApiFactory.Json))
            .Content.ReadFromJsonAsync<TransportRouteDto>(ApiFactory.Json))!;

        var publicCatalog = await PublicCatalogAsync();
        publicCatalog.Vehicles.Should().NotContain(v => v.Id == hidden.Id);
        publicCatalog.Routes.Should().NotContain(r => r.Id == route.Id, "its only price belongs to a hidden vehicle");

        var adminCatalog = await admin.GetFromJsonAsync<TransportCatalogDto>("/api/admin/transport", ApiFactory.Json);
        adminCatalog!.Vehicles.Should().Contain(v => v.Id == hidden.Id);
        adminCatalog.Routes.Should().Contain(r => r.Id == route.Id && r.Rates.Any(x => x.VehicleId == hidden.Id));

        await admin.DeleteAsync($"/api/transport/routes/{route.Id}");
        await admin.DeleteAsync($"/api/transport/vehicles/{hidden.Id}");
    }

    [Fact]
    public async Task Route_with_unknown_vehicle_returns_400()
    {
        var admin = await factory.CreateAdminClientAsync();

        var response = await admin.PostAsJsonAsync("/api/transport/routes", NewRoute("Bad Route", (999_999, 1_000m)), ApiFactory.Json);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Route_with_the_same_vehicle_twice_or_no_prices_returns_400()
    {
        var admin = await factory.CreateAdminClientAsync();
        var sedan = (await PublicCatalogAsync()).Vehicles.Single(v => v.Name == "Sedan");

        var duplicate = await admin.PostAsJsonAsync("/api/transport/routes", NewRoute("Dup Route", (sedan.Id, 1_000m), (sedan.Id, 2_000m)), ApiFactory.Json);
        var empty = await admin.PostAsJsonAsync("/api/transport/routes", NewRoute("Empty Route"), ApiFactory.Json);

        duplicate.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        empty.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Deleting_a_vehicle_removes_its_prices_from_routes()
    {
        var admin = await factory.CreateAdminClientAsync();
        var sedan = (await PublicCatalogAsync()).Vehicles.Single(v => v.Name == "Sedan");
        var temp = (await (await admin.PostAsJsonAsync("/api/transport/vehicles", NewVehicle("Temp Van"), ApiFactory.Json))
            .Content.ReadFromJsonAsync<TransportVehicleDto>(ApiFactory.Json))!;
        var route = (await (await admin.PostAsJsonAsync("/api/transport/routes", NewRoute("Cascade Route", (sedan.Id, 3_000m), (temp.Id, 4_000m)), ApiFactory.Json))
            .Content.ReadFromJsonAsync<TransportRouteDto>(ApiFactory.Json))!;

        await admin.DeleteAsync($"/api/transport/vehicles/{temp.Id}");

        var after = await admin.GetFromJsonAsync<TransportRouteDto>($"/api/admin/transport/routes/{route.Id}", ApiFactory.Json);
        after!.Rates.Should().ContainSingle(r => r.VehicleId == sedan.Id);

        await admin.DeleteAsync($"/api/transport/routes/{route.Id}");
    }
}
