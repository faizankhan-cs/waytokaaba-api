using FluentValidation;
using Waytokaaba.Application.Common;

namespace Waytokaaba.Application.Packages;

public sealed class PackageQueryValidator : AbstractValidator<PackageQuery>
{
    public PackageQueryValidator()
    {
        RuleFor(x => x.Q).MaximumLength(100);
        RuleFor(x => x.MinPrice).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(0)
            .GreaterThanOrEqualTo(x => x.MinPrice).When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue)
            .WithMessage("MaxPrice must be greater than or equal to MinPrice.");
        RuleFor(x => x.MinDays).InclusiveBetween(1, 90);
        RuleFor(x => x.MaxDays).InclusiveBetween(1, 90)
            .GreaterThanOrEqualTo(x => x.MinDays).When(x => x.MinDays.HasValue && x.MaxDays.HasValue)
            .WithMessage("MaxDays must be greater than or equal to MinDays.");
        RuleForEach(x => x.Type).IsInEnum();
        RuleFor(x => x.Sort).Must(s => s is null || PackageSort.All.Contains(s))
            .WithMessage($"Sort must be one of: {string.Join(", ", PackageSort.All)}.");
    }
}

public sealed class UpsertPackageRequestValidator : AbstractValidator<UpsertPackageRequest>
{
    public UpsertPackageRequestValidator()
    {
        RuleFor(x => x.Slug).MaximumLength(160).Matches(ValidationRules.SlugPattern)
            .When(x => !string.IsNullOrEmpty(x.Slug));
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ShortDescription).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(5000);
        RuleFor(x => x.PackageType).IsInEnum();
        RuleFor(x => x.DurationInDays).InclusiveBetween(1, 90);
        RuleFor(x => x.MakkahDays).InclusiveBetween(0, 90);
        RuleFor(x => x.MadinahDays).InclusiveBetween(0, 90);
        RuleFor(x => x).Must(x => x.MakkahDays + x.MadinahDays <= x.DurationInDays)
            .WithName("DurationInDays")
            .WithMessage("Makkah days plus Madinah days cannot exceed the total duration.");
        RuleFor(x => x.Price).GreaterThan(0).LessThan(100_000_000);
        RuleFor(x => x.Currency).Currency();
        RuleFor(x => x.ImageUrl).NotEmpty().MaximumLength(1000).ValidUrl();

        RuleFor(x => x.Hotels).Must(h => h.Count <= 10).WithMessage("At most 10 hotels per package.")
            .Must(h => h.Select(x => x.HotelId).Distinct().Count() == h.Count).WithMessage("A hotel can only be listed once.");
        RuleForEach(x => x.Hotels).ChildRules(h =>
        {
            h.RuleFor(i => i.HotelId).GreaterThan(0);
            h.RuleFor(i => i.Nights).InclusiveBetween(1, 90);
        });

        RuleFor(x => x.Services).Must(s => s.Count <= 30).WithMessage("At most 30 services.");
        RuleForEach(x => x.Services).ChildRules(s =>
        {
            s.RuleFor(i => i.ServiceName).NotEmpty().MaximumLength(120);
            s.RuleFor(i => i.Description).MaximumLength(500);
        });

        RuleFor(x => x.Images).Must(i => i.Count <= 20).WithMessage("At most 20 gallery images.");
        RuleForEach(x => x.Images).ChildRules(img =>
        {
            img.RuleFor(i => i.Url).NotEmpty().MaximumLength(1000).ValidUrl();
            img.RuleFor(i => i.Caption).MaximumLength(200);
        });
    }
}
