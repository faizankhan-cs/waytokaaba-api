using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Waytokaaba.Application.Common;
using Waytokaaba.Core.Entities;

namespace Waytokaaba.Application.Auth;

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt, string Email);

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(200);
    }
}

/// <summary>Hashes and verifies admin passwords. Implemented in Infrastructure.</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string hash, string password);
}

/// <summary>Issues signed access tokens. Implemented in Infrastructure.</summary>
public interface ITokenService
{
    (string Token, DateTimeOffset ExpiresAt) CreateAdminToken(AdminUser user);
}

public sealed class UnauthorizedException() : Exception("Invalid email or password.");

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

public sealed class AuthService(IAppDbContext db, IPasswordHasher hasher, ITokenService tokens, TimeProvider clock) : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.AdminUsers.FirstOrDefaultAsync(u => u.Email == email, ct);

        if (user is null || !hasher.Verify(user.PasswordHash, request.Password))
            throw new UnauthorizedException();

        user.LastLoginAt = clock.GetUtcNow();
        await db.SaveChangesAsync(ct);

        var (token, expiresAt) = tokens.CreateAdminToken(user);
        return new LoginResponse(token, expiresAt, user.Email);
    }
}
