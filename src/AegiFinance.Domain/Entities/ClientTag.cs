namespace AegiFinance.Domain.Entities;

public class ClientTag : AuditableEntity
{
    public string Name { get; set; } = null!;
    public string Color { get; set; } = null!; // hex color
    public ICollection<Client> Clients { get; set; } = new List<Client>();
}
