namespace AegiFinance.Application.Dtos;

public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public bool MustChangePassword { get; set; }
    public int ActiveSessionCount { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public List<string> Permissions { get; set; } = new List<string>();
    public Dictionary<string, string> UiPolicies { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class UserSessionDto
{
    public Guid Id { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public DateTime LastSeenAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public bool IsActive { get; set; }
}

public sealed class UserPermissionOverrideDto
{
    public Guid Id { get; set; }
    public Guid PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public bool IsGranted { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
