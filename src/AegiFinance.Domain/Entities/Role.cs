using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public UserType? UserType { get; set; }

    public List<User> Users { get; set; } = new List<User>();
    public List<RolePermission> RolePermissions { get; set; } = new();
    public List<UiControlPolicy> UiControlPolicies { get; set; } = new();
}
