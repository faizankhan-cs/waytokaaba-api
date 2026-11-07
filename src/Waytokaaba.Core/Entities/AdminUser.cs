namespace Waytokaaba.Core.Entities;

public class AdminUser : AuditableEntity
{
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
}
