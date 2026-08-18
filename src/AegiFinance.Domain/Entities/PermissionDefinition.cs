using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class PermissionDefinition : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public PermissionKind Kind { get; set; } = PermissionKind.Business;
    public bool IsSystemGenerated { get; set; } = true;
    public bool IsActive { get; set; } = true;

    public List<RolePermission> RolePermissions { get; set; } = new();
    public List<UserPermissionOverride> UserPermissionOverrides { get; set; } = new();
}
