using FluentValidation;
using Waytokaaba.Application.Common;

namespace Waytokaaba.Application.Hotels;

public sealed class HotelQueryValidator : AbstractValidator<HotelQuery>
{
    public HotelQueryValidator()
    {
        RuleFor(x => x.City).IsInEnum();
        RuleFor(x => x.Q).MaximumLength(100);
        RuleFor(x => x.MinPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(0)
            .GreaterThanOrEqualTo(x => x.MinPrice).When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue)
            .WithMessage("MaxPrice must be greater than or equal to MinPrice.");
        RuleForEach(x => x.Stars).InclusiveBetween(1, 5);
        RuleFor(x => x.Guests).InclusiveBetween(1, 20);
        RuleFor(x => x.Sort).Must(s => s is null || HotelSort.All.Contains(s))
            .WithMessage($"Sort must be one of: {string.Join(", ", HotelSort.All)}.");
    }
}

public sealed class UpsertHotelRequestValidator : AbstractValidator<UpsertHotelRequest>
{
    public UpsertHotelRequestValidator()
    {
        RuleFor(x => x.Slug).MaximumLength(160).Matches(ValidationRules.SlugPattern)
            .When(x => !string.IsNullOrEmpty(x.Slug));
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ShortDescription).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.City).IsInEnum();
        RuleFor(x => x.Address).NotEmpty().MaximumLength(300);
        RuleFor(x => x.StarRating).InclusiveBetween(1, 5);
        RuleFor(x => x.DistanceFromHolyMosqueMeters).InclusiveBetween(0, 50_000);
        RuleFor(x => x.IsDistanceVerified)
            .Must((x, verified) => !verified || x.DistanceFromHolyMosqueMeters.HasValue)
            .WithMessage("A verified distance requires DistanceFromHolyMosqueMeters.");
        RuleFor(x => x.ImageUrl).NotEmpty().MaximumLength(1000).ValidUrl();
        RuleFor(x => x.PricePerNight).GreaterThan(0).LessThan(100_000_000);
        RuleFor(x => x.Currency).Currency();
        RuleFor(x => x.Amenities).Must(a => a.Count <= 40).WithMessage("At most 40 amenities.");
        RuleForEach(x => x.Amenities).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Images).Must(i => i.Count <= 20).WithMessage("At most 20 gallery images.");
        RuleForEach(x => x.Images).ChildRules(img =>
        {
            img.RuleFor(i => i.Url).NotEmpty().MaximumLength(1000).ValidUrl();
            img.RuleFor(i => i.Caption).MaximumLength(200);
        });
    }
}

public sealed class UpsertRoomRequestValidator : AbstractValidator<UpsertRoomRequest>
{
    public UpsertRoomRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Capacity).InclusiveBetween(1, 20);
        RuleFor(x => x.BedConfiguration).MaximumLength(120);
        RuleFor(x => x.PricePerNight).GreaterThan(0).LessThan(100_000_000);
        RuleFor(x => x.ImageUrl).MaximumLength(1000).ValidUrl();
    }
}
