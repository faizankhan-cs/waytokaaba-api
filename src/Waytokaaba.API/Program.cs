using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Waytokaaba.API.Infrastructure;
using Waytokaaba.Application;
using Waytokaaba.Infrastructure;
using Waytokaaba.Infrastructure.Auth;
using Waytokaaba.Infrastructure.Persistence;
using Waytokaaba.Infrastructure.Persistence.Seed;
using Waytokaaba.Infrastructure.Storage;

var builder = WebApplication.CreateBuilder(args);

// Hosting platforms such as Render inject the port to listen on via $PORT.
if (Environment.GetEnvironmentVariable("PORT") is { Length: > 0 } port)
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ---------------- Services ----------------

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.PostConfigure<UploadOptions>(o => o.Directory = o.ResolveDirectory(builder.Environment.ContentRootPath));

builder.Services
    .AddControllers(o => o.Filters.Add<ValidationFilter>())
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
    });

builder.Services.AddProblemDetails(o => o.CustomizeProblemDetails = ctx =>
{
    ctx.ProblemDetails.Instance = ctx.HttpContext.Request.Path;
    ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Authentication: JWT bearer tokens issued by /api/auth/login.
var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey.PadRight(32))),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
        };
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AppPolicies.Admin, p => p.RequireAuthenticatedUser().RequireRole(JwtOptions.AdminRole));

// CORS: only the configured frontend origins may call the API from a browser.
var allowedOrigins = (builder.Configuration["Cors:AllowedOrigins"] ?? "")
    .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Select(o => o.TrimEnd('/'))
    .ToArray();
builder.Services.AddCors(o => o.AddPolicy(AppPolicies.Frontend, p => p
    .WithOrigins(allowedOrigins)
    .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE")
    .WithHeaders("Content-Type", "Authorization")
    .SetPreflightMaxAge(TimeSpan.FromHours(1))));

// Rate limiting (per client IP) for abuse-prone public endpoints.
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy(AppPolicies.InquiryLimit, ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = builder.Configuration.GetValue("RateLimits:InquiriesPerMinute", 5), Window = TimeSpan.FromMinutes(1) }));
    o.AddPolicy(AppPolicies.LoginLimit, ctx => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = builder.Configuration.GetValue("RateLimits:LoginsPer5Minutes", 10), Window = TimeSpan.FromMinutes(5) }));
});

// Behind a TLS-terminating proxy (Render, etc.), trust X-Forwarded-* so client IPs and scheme are correct.
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownIPNetworks.Clear();
    o.KnownProxies.Clear();
});

builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Way To Kaaba API",
        Version = "v1",
        Contact = new OpenApiContact { Name = "Way To Kaaba Travel & Tours", Email = "waytokaaba786@gmail.com" },
        Description = "Umrah packages, Makkah/Madinah hotel discovery and customer inquiries. Admin endpoints require a bearer token from POST /api/auth/login.",
    });
    o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the accessToken returned by /api/auth/login.",
    });
    o.AddSecurityRequirement(doc => new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference("Bearer", doc)] = [] });
    o.SupportNonNullableReferenceTypes();
    o.CustomSchemaIds(t => t.FullName?.Replace("+", ".", StringComparison.Ordinal));
});

var app = builder.Build();

// ---------------- Pipeline ----------------

app.UseForwardedHeaders();

// Admin-uploaded photos. File names are unique, so they can be cached for a year.
var uploads = app.Services.GetRequiredService<IOptions<UploadOptions>>().Value;
var uploadsDirectory = uploads.Directory;
Directory.CreateDirectory(uploadsDirectory);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsDirectory),
    RequestPath = UploadOptions.RequestPath,
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        ctx.Context.Response.Headers.XContentTypeOptions = "nosniff";
    },
});

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Configuration.GetValue("Swagger:Enabled", true))
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.SwaggerEndpoint("/swagger/v1/swagger.json", "Way To Kaaba API v1");
        o.DocumentTitle = "Way To Kaaba API";
    });
}

app.UseCors(AppPolicies.Frontend);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Ok(new
{
    name = "Waytokaaba API",
    version = Assembly.GetExecutingAssembly().GetName().Version?.ToString(),
    docs = "/swagger",
    health = "/health",
})).ExcludeFromDescription();

// Apply migrations / seed data according to configuration.
await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
}

await app.RunAsync();

/// <summary>Exposed for WebApplicationFactory in integration tests.</summary>
public partial class Program;
