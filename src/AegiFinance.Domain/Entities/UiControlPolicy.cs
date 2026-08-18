using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class UiControlPolicy : AuditableEntity
{
    public Guid UiControlDefinitionId { get; set; }
    public UiControlDefinition UiControlDefinition { get; set; } = null!;
    public Guid? RoleId { get; set; }
    public Role? Role { get; set; }
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public Guid? ClientId { get; set; }
    public Guid? SubscriptionId { get; set; }
    public UiAccessMode AccessMode { get; set; } = UiAccessMode.Enabled;
    public DateTime? ExpiresAt { get; set; }
}
