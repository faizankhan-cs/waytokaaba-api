using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Waytokaaba.Application.Auth;
using Waytokaaba.Core.Entities;

namespace Waytokaaba.Infrastructure.Persistence.Seed;

public sealed class SeedOptions
{
    public const string SectionName = "Database";

    /// <summary>Apply pending EF Core migrations when the API starts.</summary>
    public bool ApplyMigrationsOnStartup { get; set; }

    /// <summary>Insert demonstration hotels/packages when the database has none.</summary>
    public bool SeedDemoData { get; set; }

    /// <summary>Initial admin account (created only if it doesn't exist). Supply via environment variables.</summary>
    public string? AdminEmail { get; set; }
    public string? AdminPassword { get; set; }
}

public sealed partial class DatabaseInitializer(
    AppDbContext db,
    IPasswordHasher hasher,
    IOptions<SeedOptions> options,
    ILogger<DatabaseInitializer> logger)
{
    private readonly SeedOptions _options = options.Value;

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (_options.ApplyMigrationsOnStartup)
        {
            LogMigrating(logger);
            await db.Database.MigrateAsync(ct);
        }

        await EnsureAdminAsync(ct);

        if (_options.SeedDemoData && !await db.Hotels.AnyAsync(ct))
        {
            LogSeeding(logger);
            var (hotels, packages) = DemoData.Build();
            db.Hotels.AddRange(hotels);
            db.Packages.AddRange(packages);
            await db.SaveChangesAsync(ct);
            LogSeeded(logger, hotels.Count, packages.Count);
        }
    }

    private async Task EnsureAdminAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_options.AdminEmail) || string.IsNullOrWhiteSpace(_options.AdminPassword))
        {
            if (!await db.AdminUsers.AnyAsync(ct))
                LogNoAdmin(logger);
            return;
        }

        if (_options.AdminPassword.Length < 10)
            throw new InvalidOperationException("Database:AdminPassword must be at least 10 characters.");

        var email = _options.AdminEmail.Trim().ToLowerInvariant();
        if (await db.AdminUsers.AnyAsync(u => u.Email == email, ct))
            return;

        db.AdminUsers.Add(new AdminUser { Email = email, PasswordHash = hasher.Hash(_options.AdminPassword) });
        await db.SaveChangesAsync(ct);
        LogAdminCreated(logger, email);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Applying database migrations...")]
    private static partial void LogMigrating(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeding demonstration data...")]
    private static partial void LogSeeding(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Seeded {Hotels} hotels and {Packages} packages.")]
    private static partial void LogSeeded(ILogger logger, int hotels, int packages);

    [LoggerMessage(Level = LogLevel.Warning, Message = "No admin account exists. Set Database__AdminEmail and Database__AdminPassword to create one.")]
    private static partial void LogNoAdmin(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Created admin account {Email}.")]
    private static partial void LogAdminCreated(ILogger logger, string email);
}
