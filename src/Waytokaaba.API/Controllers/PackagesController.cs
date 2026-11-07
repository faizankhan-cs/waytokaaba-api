using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waytokaaba.API.Infrastructure;
using Waytokaaba.Application.Common;
using Waytokaaba.Application.Packages;

namespace Waytokaaba.API.Controllers;

[ApiController]
[Route("api/packages")]
[Produces("application/json")]
public sealed class PackagesController(IPackageService packages) : ControllerBase
{
    /// <summary>List Umrah packages with optional filtering, sorting and pagination.</summary>
    [HttpGet]
    public Task<PagedResult<PackageSummaryDto>> List([FromQuery] PackageQuery query, CancellationToken ct) =>
        packages.SearchAsync(query, ct: ct);

    /// <summary>Search packages (same parameters as the list endpoint).</summary>
    /// <remarks>Example: /api/packages/search?q=ramadan&amp;type=Premium&amp;maxDays=14&amp;sort=price_asc</remarks>
    [HttpGet("search")]
    public Task<PagedResult<PackageSummaryDto>> Search([FromQuery] PackageQuery query, CancellationToken ct) =>
        packages.SearchAsync(query, ct: ct);

    /// <summary>Featured packages for the homepage.</summary>
    [HttpGet("featured")]
    public async Task<IReadOnlyList<PackageSummaryDto>> Featured([FromQuery] int limit = 6, CancellationToken ct = default)
    {
        var result = await packages.SearchAsync(new PackageQuery { Featured = true, PageSize = Math.Clamp(limit, 1, 12) }, ct: ct);
        return result.Items;
    }

    /// <summary>Package details by numeric id or slug.</summary>
    [HttpGet("{idOrSlug}")]
    [ProducesResponseType<PackageDetailDto>(200)]
    [ProducesResponseType<ProblemDetails>(404)]
    public Task<PackageDetailDto> Get(string idOrSlug, CancellationToken ct) => packages.GetAsync(idOrSlug, ct: ct);

    // ---------------- Admin ----------------

    [HttpPost]
    [Authorize(Policy = AppPolicies.Admin)]
    [ProducesResponseType<PackageDetailDto>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<ActionResult<PackageDetailDto>> Create(UpsertPackageRequest request, CancellationToken ct)
    {
        var package = await packages.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { idOrSlug = package.Id }, package);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = AppPolicies.Admin)]
    public Task<PackageDetailDto> Update(int id, UpsertPackageRequest request, CancellationToken ct) =>
        packages.UpdateAsync(id, request, ct);

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AppPolicies.Admin)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await packages.DeleteAsync(id, ct);
        return NoContent();
    }
}
