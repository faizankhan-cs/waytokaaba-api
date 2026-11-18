using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waytokaaba.API.Infrastructure;
using Waytokaaba.Application.Trips;

namespace Waytokaaba.API.Controllers;

[ApiController]
[Route("api/trip-settings")]
[Produces("application/json")]
public sealed class TripSettingsController(ITripSettingsService settings) : ControllerBase
{
    /// <summary>Prices used by the Round Trip planner (combined visa + transport price per guest).</summary>
    [HttpGet]
    public Task<TripSettingsDto> Get(CancellationToken ct) => settings.GetAsync(ct);

    [HttpPut]
    [Authorize(Policy = AppPolicies.Admin)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public Task<TripSettingsDto> Update(UpdateTripSettingsRequest request, CancellationToken ct) => settings.UpdateAsync(request, ct);
}
