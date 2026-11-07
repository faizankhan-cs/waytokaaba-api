using Waytokaaba.Application.Common;

namespace Waytokaaba.Application.Hotels;

public interface IHotelService
{
    /// <param name="adminView">Admin view: include inactive records and unverified distances.</param>
    Task<PagedResult<HotelSummaryDto>> SearchAsync(HotelQuery query, bool adminView = false, CancellationToken ct = default);
    Task<HotelDetailDto> GetAsync(string idOrSlug, bool adminView = false, CancellationToken ct = default);
    Task<HotelDetailDto> CreateAsync(UpsertHotelRequest request, CancellationToken ct = default);
    Task<HotelDetailDto> UpdateAsync(int id, UpsertHotelRequest request, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<RoomDto>> GetRoomsAsync(int hotelId, CancellationToken ct = default);
    Task<RoomDto> AddRoomAsync(int hotelId, UpsertRoomRequest request, CancellationToken ct = default);
    Task<RoomDto> UpdateRoomAsync(int hotelId, int roomId, UpsertRoomRequest request, CancellationToken ct = default);
    Task DeleteRoomAsync(int hotelId, int roomId, CancellationToken ct = default);
}
