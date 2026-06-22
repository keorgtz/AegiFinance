namespace AegiFinance.Domain.Entities;

public class ClientNote : AuditableEntity
{
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public string Content { get; set; } = null!;
    public bool IsPinned { get; set; }
}
