namespace AegiFinance.Domain.Entities;

public class ClientUser : BaseEntity
{
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
