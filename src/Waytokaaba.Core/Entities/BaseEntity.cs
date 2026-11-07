namespace Waytokaaba.Core.Entities;

/// <summary>Base type for all persisted entities.</summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
}

/// <summary>Entity with creation/update audit timestamps (UTC), set automatically by the DbContext.</summary>
public abstract class AuditableEntity : BaseEntity
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
