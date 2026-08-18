namespace AegiFinance.Domain.Entities;

public sealed class UserSession : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string RefreshTokenHash { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime LastSeenAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public Guid? RevokedBy { get; set; }
    public string? RevokedReason { get; set; }

    public bool IsActiveAt(DateTime utcNow) => RevokedAt is null && ExpiresAt > utcNow && !IsDeleted;
}
