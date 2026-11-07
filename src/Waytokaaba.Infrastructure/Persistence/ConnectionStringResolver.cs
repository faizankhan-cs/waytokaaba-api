using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Waytokaaba.Infrastructure.Persistence;

/// <summary>
/// Resolves the PostgreSQL connection string. Accepts either a standard Npgsql connection string
/// (ConnectionStrings:DefaultConnection) or a URL (DATABASE_URL, as given by Neon/Render/Railway:
/// postgresql://user:pass@host/db?sslmode=require).
/// </summary>
public static class ConnectionStringResolver
{
    public static string? Resolve(IConfiguration configuration)
    {
        var value = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(value))
            value = configuration["DATABASE_URL"];
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
            ? FromUrl(value)
            : value;
    }

    private static string FromUrl(string url)
    {
        var uri = new Uri(url);
        var userInfo = uri.UserInfo.Split(':', 2);
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : null,
            SslMode = SslMode.Require,
        };

        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        if (query["sslmode"] is { } sslMode && Enum.TryParse<SslMode>(sslMode.Replace("-", "", StringComparison.Ordinal), true, out var mode))
            builder.SslMode = mode;

        return builder.ConnectionString;
    }
}
