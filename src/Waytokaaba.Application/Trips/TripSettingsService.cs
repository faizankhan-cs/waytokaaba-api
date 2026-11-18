using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Waytokaaba.Application.Common;
using Entity = Waytokaaba.Core.Entities.TripSettings;

namespace Waytokaaba.Application.Trips;

/// <summary>Prices used by the Round Trip planner.</summary>
/// <param name="VisaTransportPricePerPerson">Combined visa + transport price per guest (infants included).</param>
public sealed record TripSettingsDto(decimal VisaTransportPricePerPerson, string Currency);

public sealed record UpdateTripSettingsRequest(decimal VisaTransportPricePerPerson);

public sealed class UpdateTripSettingsRequestValidator : AbstractValidator<UpdateTripSettingsRequest>
{
    public UpdateTripSettingsRequestValidator()
    {
        RuleFor(x => x.VisaTransportPricePerPerson).GreaterThan(0).LessThanOrEqualTo(10_000_000);
    }
}

public interface ITripSettingsService
{
    Task<TripSettingsDto> GetAsync(CancellationToken ct = default);
    Task<TripSettingsDto> UpdateAsync(UpdateTripSettingsRequest request, CancellationToken ct = default);
}

public sealed class TripSettingsService(IAppDbContext db) : ITripSettingsService
{
    private const string Currency = "PKR";

    public async Task<TripSettingsDto> GetAsync(CancellationToken ct = default)
    {
        var settings = await db.TripSettings.AsNoTracking().SingleOrDefaultAsync(s => s.Id == Entity.SingletonId, ct)
            ?? throw new NotFoundException("Trip settings", Entity.SingletonId);
        return ToDto(settings);
    }

    public async Task<TripSettingsDto> UpdateAsync(UpdateTripSettingsRequest request, CancellationToken ct = default)
    {
        var settings = await db.TripSettings.SingleOrDefaultAsync(s => s.Id == Entity.SingletonId, ct);
        if (settings is null)
        {
            settings = new Entity { Id = Entity.SingletonId };
            db.TripSettings.Add(settings);
        }

        settings.VisaTransportPricePerPerson = request.VisaTransportPricePerPerson;
        await db.SaveChangesAsync(ct);
        return ToDto(settings);
    }

    private static TripSettingsDto ToDto(Entity s) => new(s.VisaTransportPricePerPerson, Currency);
}
