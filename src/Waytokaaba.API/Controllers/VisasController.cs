using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waytokaaba.API.Infrastructure;
using Waytokaaba.Application.Common;
using Waytokaaba.Application.Visas;

namespace Waytokaaba.API.Controllers;

[ApiController]
[Route("api/visas")]
[Produces("application/json")]
public sealed class VisasController(IVisaService visas) : ControllerBase
{
    /// <summary>List the countries we process visas for, with the lowest tier price.</summary>
    [HttpGet]
    public Task<PagedResult<VisaSummaryDto>> List([FromQuery] VisaQuery query, CancellationToken ct) =>
        visas.SearchAsync(query, ct: ct);

    /// <summary>Featured visas for the homepage slider.</summary>
    [HttpGet("featured")]
    public async Task<IReadOnlyList<VisaSummaryDto>> Featured([FromQuery] int limit = 8, CancellationToken ct = default)
    {
        var result = await visas.SearchAsync(new VisaQuery { Featured = true, PageSize = Math.Clamp(limit, 1, 12) }, ct: ct);
        return result.Items;
    }

    /// <summary>Visa details (tiers, documents, inclusions) by numeric id or slug.</summary>
    [HttpGet("{idOrSlug}")]
    [ProducesResponseType<VisaDetailDto>(200)]
    [ProducesResponseType<ProblemDetails>(404)]
    public Task<VisaDetailDto> Get(string idOrSlug, CancellationToken ct) => visas.GetAsync(idOrSlug, ct: ct);

    // ---------------- Admin ----------------

    [HttpPost]
    [Authorize(Policy = AppPolicies.Admin)]
    [ProducesResponseType<VisaDetailDto>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<ActionResult<VisaDetailDto>> Create(UpsertVisaRequest request, CancellationToken ct)
    {
        var visa = await visas.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { idOrSlug = visa.Id }, visa);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = AppPolicies.Admin)]
    public Task<VisaDetailDto> Update(int id, UpsertVisaRequest request, CancellationToken ct) =>
        visas.UpdateAsync(id, request, ct);

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AppPolicies.Admin)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await visas.DeleteAsync(id, ct);
        return NoContent();
    }
}
