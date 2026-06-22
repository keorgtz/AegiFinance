namespace AegiFinance.Domain.Entities;

public class ClientUser : BaseEntity
{
    public Guid ClientId { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
