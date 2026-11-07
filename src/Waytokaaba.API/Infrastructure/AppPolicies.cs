namespace Waytokaaba.API.Infrastructure;

public static class AppPolicies
{
    /// <summary>Authorization policy for all administrative endpoints.</summary>
    public const string Admin = "Admin";

    /// <summary>CORS policy for the Next.js frontend.</summary>
    public const string Frontend = "Frontend";

    /// <summary>Rate-limit policy for public inquiry submissions.</summary>
    public const string InquiryLimit = "inquiries";

    /// <summary>Rate-limit policy for admin login attempts.</summary>
    public const string LoginLimit = "login";
}
