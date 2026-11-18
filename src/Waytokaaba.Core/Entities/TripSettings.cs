namespace Waytokaaba.Core.Entities;

/// <summary>
/// Site-wide prices used by the Round Trip planner. There is exactly one row (Id = 1), edited from the admin panel.
/// </summary>
public class TripSettings : BaseEntity
{
    public const int SingletonId = 1;

    /// <summary>Combined Umrah visa + transport price (PKR) charged per guest, infants included.</summary>
    public decimal VisaTransportPricePerPerson { get; set; }
}
