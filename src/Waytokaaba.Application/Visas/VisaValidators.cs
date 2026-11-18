using FluentValidation;
using Waytokaaba.Application.Common;

namespace Waytokaaba.Application.Visas;

public sealed class VisaQueryValidator : AbstractValidator<VisaQuery>
{
    public VisaQueryValidator()
    {
        RuleFor(x => x.Q).MaximumLength(100);
        RuleForEach(x => x.Format).IsInEnum();
    }
}

public sealed class UpsertVisaRequestValidator : AbstractValidator<UpsertVisaRequest>
{
    public UpsertVisaRequestValidator()
    {
        RuleFor(x => x.Slug).MaximumLength(160).Matches(ValidationRules.SlugPattern)
            .When(x => !string.IsNullOrEmpty(x.Slug));
        RuleFor(x => x.Country).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CountryCode).NotEmpty().Matches("^[A-Za-z]{2}$")
            .WithMessage("CountryCode must be a 2-letter ISO code, e.g. AE.");
        RuleFor(x => x.ImageUrl).NotEmpty().MaximumLength(1000).ValidUrl();
        RuleFor(x => x.VisaFormat).IsInEnum();
        RuleFor(x => x.EntryType).IsInEnum();
        RuleFor(x => x.Summary).NotEmpty().MaximumLength(500);
        RuleFor(x => x.ProcessingTime).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Validity).NotEmpty().MaximumLength(80);
        RuleFor(x => x.AllowedStay).NotEmpty().MaximumLength(80);
        RuleFor(x => x.Disclaimer).MaximumLength(1000);
        RuleFor(x => x.SortOrder).InclusiveBetween(0, 10_000);

        RuleFor(x => x.RequiredDocuments).Must(d => d.Count <= 30).WithMessage("At most 30 documents.");
        RuleForEach(x => x.RequiredDocuments).NotEmpty().MaximumLength(200);

        RuleFor(x => x.Tiers).NotEmpty().WithMessage("Add at least one visa tier with a price.")
            .Must(t => t.Count <= 10).WithMessage("At most 10 tiers per visa.");
        RuleForEach(x => x.Tiers).ChildRules(t =>
        {
            t.RuleFor(i => i.Name).NotEmpty().MaximumLength(150);
            t.RuleFor(i => i.Description).MaximumLength(500);
            t.RuleFor(i => i.Price).GreaterThan(0).LessThan(100_000_000);
        });
    }
}
