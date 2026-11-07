using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Waytokaaba.Application.Auth;
using Waytokaaba.Core.Entities;

namespace Waytokaaba.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public const string AdminRole = "Admin";

    public string Issuer { get; set; } = "waytokaaba-api";
    public string Audience { get; set; } = "waytokaaba-admin";

    /// <summary>HMAC signing key, at least 32 characters. Must come from environment/secret storage.</summary>
    public string SigningKey { get; set; } = "";

    public int ExpiryMinutes { get; set; } = 480;
}

public sealed class JwtTokenService(IOptions<JwtOptions> options, TimeProvider clock) : ITokenService
{
    private readonly JwtOptions _options = options.Value;

    public (string Token, DateTimeOffset ExpiresAt) CreateAdminToken(AdminUser user)
    {
        var now = clock.GetUtcNow();
        var expires = now.AddMinutes(_options.ExpiryMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, JwtOptions.AdminRole),
            ],
            notBefore: now.UtcDateTime,
            expires: expires.UtcDateTime,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}

/// <summary>PBKDF2 hashing via ASP.NET Core Identity's battle-tested hasher.</summary>
public sealed class IdentityPasswordHasher : Application.Auth.IPasswordHasher
{
    private static readonly PasswordHasher<AdminUser> Hasher = new();
    private static readonly AdminUser Dummy = new() { Email = "", PasswordHash = "" };

    public string Hash(string password) => Hasher.HashPassword(Dummy, password);

    public bool Verify(string hash, string password) =>
        Hasher.VerifyHashedPassword(Dummy, hash, password) != PasswordVerificationResult.Failed;
}
