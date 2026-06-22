namespace AegiFinance.Domain.Entities;

public class ServiceCategory : AuditableEntity
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ICollection<Service> Services { get; set; } = new List<Service>();
}
