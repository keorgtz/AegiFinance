namespace AegiFinance.Domain.Entities;

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public PermissionDefinition Permission { get; set; } = null!;
    public bool IsGranted { get; set; } = true;
}
