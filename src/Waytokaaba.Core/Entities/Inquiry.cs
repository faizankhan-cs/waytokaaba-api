using Waytokaaba.Core.Enums;

namespace Waytokaaba.Core.Entities;

public class Inquiry : AuditableEntity
{
    public required string CustomerName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public InquiryType InquiryType { get; set; }
    public int? PackageId { get; set; }
    public Package? Package { get; set; }
    public int? HotelId { get; set; }
    public Hotel? Hotel { get; set; }
    public DateOnly? CheckIn { get; set; }
    public DateOnly? CheckOut { get; set; }
    public int? Guests { get; set; }
    public required string Message { get; set; }
    public InquiryStatus Status { get; set; } = InquiryStatus.New;
}
