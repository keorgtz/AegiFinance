namespace AegiFinance.Domain.Entities;

public class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public List<Role> Roles { get; set; } = new List<Role>();
    public List<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
