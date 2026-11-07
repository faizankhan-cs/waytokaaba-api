using FluentValidation;

namespace Waytokaaba.Application.Common;

public static class ValidationRules
{
    public const string SlugPattern = "^[a-z0-9]+(?:-[a-z0-9]+)*$";
    public const string PhonePattern = @"^\+?[0-9\s\-()]{7,20}$";

    public static IRuleBuilderOptions<T, string?> ValidUrl<T>(this IRuleBuilder<T, string?> rule) =>
        rule.Must(u => u is null || (Uri.TryCreate(u, UriKind.Absolute, out var uri)
                                     && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)))
            .WithMessage("'{PropertyName}' must be an absolute http(s) URL.");

    public static IRuleBuilderOptions<T, string> Currency<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().Length(3).Matches("^[A-Z]{3}$").WithMessage("Currency must be a 3-letter ISO code, e.g. PKR.");
}
