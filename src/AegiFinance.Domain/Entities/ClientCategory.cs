namespace AegiFinance.Domain.Entities;

public class ClientCategory : AuditableEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ICollection<Client> Clients { get; set; } = new List<Client>();
}
