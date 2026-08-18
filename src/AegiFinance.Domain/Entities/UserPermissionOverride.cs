namespace AegiFinance.Domain.Entities;

public class UserPermissionOverride : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public PermissionDefinition Permission { get; set; } = null!;
    public bool IsGranted { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
