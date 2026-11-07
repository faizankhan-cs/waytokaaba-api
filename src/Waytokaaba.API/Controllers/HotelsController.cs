using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Waytokaaba.API.Infrastructure;
using Waytokaaba.Application.Common;
using Waytokaaba.Application.Hotels;
using Waytokaaba.Core.Enums;

namespace Waytokaaba.API.Controllers;

[ApiController]
[Route("api/hotels")]
[Produces("application/json")]
public sealed class HotelsController(IHotelService hotels) : ControllerBase
{
    /// <summary>List hotels with optional filtering, sorting and pagination.</summary>
    [HttpGet]
    public Task<PagedResult<HotelSummaryDto>> List([FromQuery] HotelQuery query, CancellationToken ct) =>
        hotels.SearchAsync(query, ct: ct);

    /// <summary>Search hotels (same parameters as the list endpoint).</summary>
    /// <remarks>Example: /api/hotels/search?city=Makkah&amp;guests=3&amp;stars=4&amp;stars=5&amp;maxPrice=50000&amp;sort=price_asc</remarks>
    [HttpGet("search")]
    public Task<PagedResult<HotelSummaryDto>> Search([FromQuery] HotelQuery query, CancellationToken ct) =>
        hotels.SearchAsync(query, ct: ct);

    /// <summary>Hotels in one city (Makkah or Madinah).</summary>
    [HttpGet("city/{city}")]
    [ProducesResponseType<PagedResult<HotelSummaryDto>>(200)]
    [ProducesResponseType<ProblemDetails>(404)]
    public async Task<ActionResult<PagedResult<HotelSummaryDto>>> ByCity(string city, [FromQuery] HotelQuery query, CancellationToken ct)
    {
        if (!Enum.TryParse<City>(city, ignoreCase: true, out var parsed) || !Enum.IsDefined(parsed))
            throw new NotFoundException("City", city);
        query.City = parsed;
        return await hotels.SearchAsync(query, ct: ct);
    }

    /// <summary>Featured hotels for the homepage.</summary>
    [HttpGet("featured")]
    public async Task<IReadOnlyList<HotelSummaryDto>> Featured([FromQuery] City? city, [FromQuery] int limit = 6, CancellationToken ct = default)
    {
        var result = await hotels.SearchAsync(
            new HotelQuery { City = city, Featured = true, PageSize = Math.Clamp(limit, 1, 12) }, ct: ct);
        return result.Items;
    }

    /// <summary>Hotel details by numeric id or slug.</summary>
    [HttpGet("{idOrSlug}")]
    [ProducesResponseType<HotelDetailDto>(200)]
    [ProducesResponseType<ProblemDetails>(404)]
    public Task<HotelDetailDto> Get(string idOrSlug, CancellationToken ct) => hotels.GetAsync(idOrSlug, ct: ct);

    /// <summary>Room types for a hotel.</summary>
    [HttpGet("{hotelId:int}/rooms")]
    public Task<IReadOnlyList<RoomDto>> Rooms(int hotelId, CancellationToken ct) => hotels.GetRoomsAsync(hotelId, ct);

    // ---------------- Admin ----------------

    [HttpPost]
    [Authorize(Policy = AppPolicies.Admin)]
    [ProducesResponseType<HotelDetailDto>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<ActionResult<HotelDetailDto>> Create(UpsertHotelRequest request, CancellationToken ct)
    {
        var hotel = await hotels.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { idOrSlug = hotel.Id }, hotel);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = AppPolicies.Admin)]
    public Task<HotelDetailDto> Update(int id, UpsertHotelRequest request, CancellationToken ct) =>
        hotels.UpdateAsync(id, request, ct);

    [HttpDelete("{id:int}")]
    [Authorize(Policy = AppPolicies.Admin)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await hotels.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{hotelId:int}/rooms")]
    [Authorize(Policy = AppPolicies.Admin)]
    [ProducesResponseType<RoomDto>(201)]
    public async Task<ActionResult<RoomDto>> AddRoom(int hotelId, UpsertRoomRequest request, CancellationToken ct)
    {
        var room = await hotels.AddRoomAsync(hotelId, request, ct);
        return CreatedAtAction(nameof(Rooms), new { hotelId }, room);
    }

    [HttpPut("{hotelId:int}/rooms/{roomId:int}")]
    [Authorize(Policy = AppPolicies.Admin)]
    public Task<RoomDto> UpdateRoom(int hotelId, int roomId, UpsertRoomRequest request, CancellationToken ct) =>
        hotels.UpdateRoomAsync(hotelId, roomId, request, ct);

    [HttpDelete("{hotelId:int}/rooms/{roomId:int}")]
    [Authorize(Policy = AppPolicies.Admin)]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteRoom(int hotelId, int roomId, CancellationToken ct)
    {
        await hotels.DeleteRoomAsync(hotelId, roomId, ct);
        return NoContent();
    }
}
