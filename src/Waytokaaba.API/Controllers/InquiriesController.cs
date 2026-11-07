using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Waytokaaba.API.Infrastructure;
using Waytokaaba.Application.Common;
using Waytokaaba.Application.Inquiries;

namespace Waytokaaba.API.Controllers;

[ApiController]
[Route("api/inquiries")]
[Produces("application/json")]
public sealed class InquiriesController(IInquiryService inquiries) : ControllerBase
{
    /// <summary>Submit an inquiry for a package, hotel, or a general question. This is not a booking.</summary>
    [HttpPost]
    [EnableRateLimiting(AppPolicies.InquiryLimit)]
    [ProducesResponseType<InquiryCreatedDto>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    [ProducesResponseType(429)]
    public async Task<IActionResult> Create(CreateInquiryRequest request, CancellationToken ct)
    {
        var created = await inquiries.CreateAsync(request, ct);
        // Spam submissions get the same success status so bots learn nothing.
        return created is null ? StatusCode(201) : StatusCode(201, created);
    }

    /// <summary>Admin: list inquiries, newest first.</summary>
    [HttpGet]
    [Authorize(Policy = AppPolicies.Admin)]
    public Task<PagedResult<InquiryDto>> List([FromQuery] InquiryQuery query, CancellationToken ct) =>
        inquiries.ListAsync(query, ct);

    /// <summary>Admin: update an inquiry's status.</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Policy = AppPolicies.Admin)]
    public Task<InquiryDto> UpdateStatus(int id, UpdateInquiryStatusRequest request, CancellationToken ct) =>
        inquiries.UpdateStatusAsync(id, request.Status, ct);
}
