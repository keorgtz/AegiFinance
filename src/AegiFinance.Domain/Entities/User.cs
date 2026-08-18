using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class User : AuditableEntity
{
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserType UserType { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public DateTime? PasswordChangedAt { get; set; }
    public bool MustChangePassword { get; set; }

    public List<Role> Roles { get; set; } = new List<Role>();
    public List<UserPermissionOverride> PermissionOverrides { get; set; } = new();
    public List<UiControlPolicy> UiControlPolicies { get; set; } = new();
    public List<UserSession> Sessions { get; set; } = new();
}
