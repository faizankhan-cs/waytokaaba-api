using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Waytokaaba.Application.Common;
using Waytokaaba.Application.Visas;
using Waytokaaba.Core.Enums;

namespace Waytokaaba.Tests;

[Collection(ApiCollection.Name)]
public sealed class VisasApiTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    private static UpsertVisaRequest NewVisa(string country = "Test Land", bool active = true) => new()
    {
        Country = country,
        CountryCode = "TL",
        ImageUrl = "https://example.com/landmark.jpg",
        VisaFormat = VisaFormat.EVisa,
        Summary = "A test visa.",
        ProcessingTime = "3–5 Working Days",
        Validity = "60 days to enter",
        AllowedStay = "30 days",
        EntryType = VisaEntryType.SingleEntry,
        IncludesServiceCharge = true,
        RequiredDocuments = ["Passport", "Photo"],
        IsActive = active,
        Tiers = [new VisaTierRequest("30-Day Visa", null, 30_000m), new VisaTierRequest("60-Day Visa", "Longer stay", 45_000m)],
    };

    [Fact]
    public async Task List_returns_active_demo_visas_with_a_starting_price()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<VisaSummaryDto>>("/api/visas", ApiFactory.Json);

        result!.Items.Should().NotBeEmpty()
            .And.OnlyContain(v => v.IsActive && v.StartingPrice > 0 && v.Currency == "PKR");
        result.Items.Should().Contain(v => v.Slug == "uae-visa" && v.StartingPrice == 28_000m);
    }

    [Fact]
    public async Task Featured_returns_only_featured_visas()
    {
        var items = await _client.GetFromJsonAsync<List<VisaSummaryDto>>("/api/visas/featured?limit=3", ApiFactory.Json);

        items.Should().NotBeEmpty().And.HaveCountLessThanOrEqualTo(3).And.OnlyContain(v => v.IsFeatured);
    }

    [Fact]
    public async Task Get_by_slug_returns_tiers_and_documents()
    {
        var visa = await _client.GetFromJsonAsync<VisaDetailDto>("/api/visas/uae-visa", ApiFactory.Json);

        visa!.Tiers.Should().HaveCountGreaterThan(1).And.BeInAscendingOrder(t => t.SortOrder);
        visa.RequiredDocuments.Should().NotBeEmpty();
        visa.StartingPrice.Should().Be(visa.Tiers.Min(t => t.Price));
    }

    [Fact]
    public async Task Get_unknown_visa_returns_404()
    {
        var response = await _client.GetAsync("/api/visas/no-such-visa");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Search_by_country_is_case_insensitive()
    {
        var result = await _client.GetFromJsonAsync<PagedResult<VisaSummaryDto>>("/api/visas?q=tURk", ApiFactory.Json);

        result!.Items.Should().ContainSingle(v => v.Slug == "turkey-visa");
    }

    [Fact]
    public async Task Anonymous_cannot_create_a_visa()
    {
        var response = await _client.PostAsJsonAsync("/api/visas", NewVisa(), ApiFactory.Json);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Admin_can_create_update_and_delete_a_visa()
    {
        var admin = await factory.CreateAdminClientAsync();

        var created = await admin.PostAsJsonAsync("/api/visas", NewVisa("Crud Land"), ApiFactory.Json);
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var visa = (await created.Content.ReadFromJsonAsync<VisaDetailDto>(ApiFactory.Json))!;
        visa.Slug.Should().Be("crud-land-visa");
        visa.Tiers.Should().HaveCount(2);
        visa.StartingPrice.Should().Be(30_000m);

        var update = NewVisa("Crud Land");
        update.Tiers = [new VisaTierRequest("Only Tier", null, 20_000m)];
        update.RequiredDocuments = ["Passport"];
        var updated = await admin.PutAsJsonAsync($"/api/visas/{visa.Id}", update, ApiFactory.Json);
        updated.StatusCode.Should().Be(HttpStatusCode.OK);
        var after = (await updated.Content.ReadFromJsonAsync<VisaDetailDto>(ApiFactory.Json))!;
        after.Tiers.Should().ContainSingle(t => t.Name == "Only Tier");
        after.RequiredDocuments.Should().ContainSingle();

        (await admin.DeleteAsync($"/api/visas/{visa.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await _client.GetAsync($"/api/visas/{visa.Slug}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Inactive_visa_is_hidden_publicly_but_visible_to_admin()
    {
        var admin = await factory.CreateAdminClientAsync();
        var created = await admin.PostAsJsonAsync("/api/visas", NewVisa("Hidden Land", active: false), ApiFactory.Json);
        var visa = (await created.Content.ReadFromJsonAsync<VisaDetailDto>(ApiFactory.Json))!;

        (await _client.GetAsync($"/api/visas/{visa.Slug}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await admin.GetAsync($"/api/admin/visas/{visa.Id}")).StatusCode.Should().Be(HttpStatusCode.OK);

        await admin.DeleteAsync($"/api/visas/{visa.Id}");
    }

    [Fact]
    public async Task Creating_a_visa_without_tiers_is_rejected()
    {
        var admin = await factory.CreateAdminClientAsync();
        var request = NewVisa();
        request.Tiers = [];

        var response = await admin.PostAsJsonAsync("/api/visas", request, ApiFactory.Json);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
