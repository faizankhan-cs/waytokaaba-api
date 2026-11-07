using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Waytokaaba.Application.Common;

public static partial class SlugHelper
{
    public static string Slugify(string input)
    {
        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        var ascii = sb.ToString().ToLowerInvariant();
        ascii = NonAlphanumeric().Replace(ascii, "-").Trim('-');
        return MultiDash().Replace(ascii, "-");
    }

    /// <summary>
    /// An explicitly requested slug must be free (otherwise 409). A slug derived from the name
    /// gets a numeric suffix on collision.
    /// </summary>
    public static async Task<string> ResolveUniqueAsync(
        string? requested, string name, string resourceName, Func<string, Task<bool>> isTaken)
    {
        var explicitSlug = !string.IsNullOrWhiteSpace(requested);
        var slug = Slugify(explicitSlug ? requested! : name);
        if (slug.Length == 0)
            throw new RequestValidationException("Slug", "Could not derive a URL slug from the name.");

        if (!await isTaken(slug))
            return slug;
        if (explicitSlug)
            throw new ConflictException($"Slug '{slug}' is already used by another {resourceName}.");

        for (var i = 2; ; i++)
        {
            var candidate = $"{slug}-{i}";
            if (!await isTaken(candidate))
                return candidate;
        }
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();

    [GeneratedRegex("-{2,}")]
    private static partial Regex MultiDash();
}
