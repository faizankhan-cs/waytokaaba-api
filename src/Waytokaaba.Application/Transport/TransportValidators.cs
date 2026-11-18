using FluentValidation;
using Waytokaaba.Application.Common;

namespace Waytokaaba.Application.Transport;

public sealed class UpsertTransportVehicleRequestValidator : AbstractValidator<UpsertTransportVehicleRequest>
{
    public UpsertTransportVehicleRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ImageUrl).NotEmpty().MaximumLength(1000).ValidUrl();
        RuleFor(x => x.Seats).InclusiveBetween(1, 60);
        RuleFor(x => x.Bags).InclusiveBetween(0, 60);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.SortOrder).InclusiveBetween(0, 10_000);
    }
}

public sealed class UpsertTransportRouteRequestValidator : AbstractValidator<UpsertTransportRouteRequest>
{
    public UpsertTransportRouteRequestValidator()
    {
        RuleFor(x => x.From).NotEmpty().MaximumLength(100);
        RuleFor(x => x.To).MaximumLength(100);
        RuleFor(x => x.Note).MaximumLength(300);
        RuleFor(x => x.SortOrder).InclusiveBetween(0, 10_000);

        RuleFor(x => x.Prices).NotEmpty().WithMessage("Add a price for at least one vehicle.")
            .Must(p => p.Count <= 30).WithMessage("At most 30 vehicle prices per route.")
            .Must(p => p.Select(i => i.VehicleId).Distinct().Count() == p.Count)
            .WithMessage("Each vehicle can only have one price per route.");
        RuleForEach(x => x.Prices).ChildRules(p =>
        {
            p.RuleFor(i => i.VehicleId).GreaterThan(0);
            p.RuleFor(i => i.Price).GreaterThan(0).LessThan(100_000_000);
        });
    }
}
