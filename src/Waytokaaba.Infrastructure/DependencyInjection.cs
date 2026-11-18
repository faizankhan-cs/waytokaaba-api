using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Waytokaaba.Application.Auth;
using Waytokaaba.Application.Common;
using Waytokaaba.Infrastructure.Auth;
using Waytokaaba.Infrastructure.Persistence;
using Waytokaaba.Infrastructure.Persistence.Seed;
using Waytokaaba.Infrastructure.Storage;

namespace Waytokaaba.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = ConnectionStringResolver.Resolve(configuration)
            ?? throw new InvalidOperationException(
                "Database connection is not configured. Set ConnectionStrings__DefaultConnection (or DATABASE_URL).");

        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(maxRetryCount: 5);
                npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                npgsql.MigrationsHistoryTable("__ef_migrations_history");
            })
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => o.SigningKey.Length >= 32, "Jwt:SigningKey must be at least 32 characters.")
            .ValidateOnStart();

        services.AddOptions<SeedOptions>().Bind(configuration.GetSection(SeedOptions.SectionName));

        services.AddOptions<UploadOptions>()
            .Bind(configuration.GetSection(UploadOptions.SectionName))
            .Validate(o => o.MaxBytes > 0 && o.MaxDimension >= 320, "Uploads:MaxBytes and Uploads:MaxDimension must be positive.")
            .ValidateOnStart();
        // The host makes Uploads:Directory absolute (against its content root) with PostConfigure; see Program.cs.
        services.AddSingleton<IImageStorage>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<UploadOptions>>();
            return new LocalImageStorage(options, options.Value.ResolveDirectory(AppContext.BaseDirectory));
        });

        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<Application.Auth.IPasswordHasher, IdentityPasswordHasher>();
        services.AddScoped<DatabaseInitializer>();
        return services;
    }
}
