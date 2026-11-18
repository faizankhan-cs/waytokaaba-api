using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waytokaaba.API.Infrastructure;
using Waytokaaba.Application.Transport;

namespace Waytokaaba.API.Controllers;

[ApiController]
[Route("api/transport")]
[Produces("application/json")]
public sealed class TransportController(ITransportService transport) : ControllerBase
{
    /// <summary>Private transport catalog: vehicles, routes and the price (PKR, per vehicle, one way) of each vehicle on each route.</summary>
    [HttpGet]
    public Task<TransportCatalogDto> Catalog(CancellationToken ct) => transport.GetCatalogAsync(ct: ct);

    // ---------------- Admin: vehicles ----------------

    [HttpPost("vehicles")]
    [Authorize(Policy = AppPolicies.Admin)]
    [ProducesResponseType<TransportVehicleDto>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<ActionResult<TransportVehicleDto>> CreateVehicle(UpsertTransportVehicleRequest request, CancellationToken ct)
    {
        var vehicle = await transport.CreateVehicleAsync(request, ct);
        return Created($"/api/admin/transport/vehicles/{vehicle.Id}", vehicle);
    }

    [HttpPut("vehicles/{id:int}")]
    [Authorize(Policy = AppPolicies.Admin)]
    public Task<TransportVehicleDto> UpdateVehicle(int id, UpsertTransportVehicleRequest request, CancellationToken ct) =>
        transport.UpdateVehicleAsync(id, request, ct);

    [HttpDelete("vehicles/{id:int}")]
    [Authorize(Policy = AppPolicies.Admin)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteVehicle(int id, CancellationToken ct)
    {
        await transport.DeleteVehicleAsync(id, ct);
        return NoContent();
    }

    // ---------------- Admin: routes ----------------

    [HttpPost("routes")]
    [Authorize(Policy = AppPolicies.Admin)]
    [ProducesResponseType<TransportRouteDto>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<ActionResult<TransportRouteDto>> CreateRoute(UpsertTransportRouteRequest request, CancellationToken ct)
    {
        var route = await transport.CreateRouteAsync(request, ct);
        return Created($"/api/admin/transport/routes/{route.Id}", route);
    }

    [HttpPut("routes/{id:int}")]
    [Authorize(Policy = AppPolicies.Admin)]
    public Task<TransportRouteDto> UpdateRoute(int id, UpsertTransportRouteRequest request, CancellationToken ct) =>
        transport.UpdateRouteAsync(id, request, ct);

    [HttpDelete("routes/{id:int}")]
    [Authorize(Policy = AppPolicies.Admin)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteRoute(int id, CancellationToken ct)
    {
        await transport.DeleteRouteAsync(id, ct);
        return NoContent();
    }
}
