using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class User : AuditableEntity
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserType UserType { get; set; }
    public Guid? ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public List<Role> Roles { get; set; } = new List<Role>();
    public List<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
