namespace AegiFinance.Domain.Entities;

public class ServiceCategory : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ICollection<Service> Services { get; set; } = new List<Service>();
}
