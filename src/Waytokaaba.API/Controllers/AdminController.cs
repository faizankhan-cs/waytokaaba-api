using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Waytokaaba.API.Infrastructure;
using Waytokaaba.Application.Auth;
using Waytokaaba.Application.Common;
using Waytokaaba.Application.Hotels;
using Waytokaaba.Application.Packages;

namespace Waytokaaba.API.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    /// <summary>Admin login. Returns a JWT bearer token.</summary>
    [HttpPost("login")]
    [EnableRateLimiting(AppPolicies.LoginLimit)]
    [ProducesResponseType<LoginResponse>(200)]
    [ProducesResponseType<ProblemDetails>(401)]
    public Task<LoginResponse> Login(LoginRequest request, CancellationToken ct) => auth.LoginAsync(request, ct);
}

/// <summary>Admin read endpoints that include inactive records and unverified data.</summary>
[ApiController]
[Route("api/admin")]
[Produces("application/json")]
[Authorize(Policy = AppPolicies.Admin)]
public sealed class AdminController(IHotelService hotels, IPackageService packages) : ControllerBase
{
    [HttpGet("hotels")]
    public Task<PagedResult<HotelSummaryDto>> Hotels([FromQuery] HotelQuery query, CancellationToken ct) =>
        hotels.SearchAsync(query, adminView: true, ct);

    [HttpGet("hotels/{id:int}")]
    public Task<HotelDetailDto> Hotel(int id, CancellationToken ct) =>
        hotels.GetAsync(id.ToString(System.Globalization.CultureInfo.InvariantCulture), adminView: true, ct);

    [HttpGet("packages")]
    public Task<PagedResult<PackageSummaryDto>> Packages([FromQuery] PackageQuery query, CancellationToken ct) =>
        packages.SearchAsync(query, adminView: true, ct);

    [HttpGet("packages/{id:int}")]
    public Task<PackageDetailDto> Package(int id, CancellationToken ct) =>
        packages.GetAsync(id.ToString(System.Globalization.CultureInfo.InvariantCulture), adminView: true, ct);
}
