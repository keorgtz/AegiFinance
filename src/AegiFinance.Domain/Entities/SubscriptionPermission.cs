namespace AegiFinance.Domain.Entities;

public class SubscriptionPermission : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid SubscriptionId { get; set; }
}
