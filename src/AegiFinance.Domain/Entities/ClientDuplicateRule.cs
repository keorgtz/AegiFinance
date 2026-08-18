namespace AegiFinance.Domain.Entities;

public class ClientDuplicateRule : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public bool MatchTaxId { get; set; } = true;
    public bool MatchName { get; set; } = true;
    public bool MatchBillingEmail { get; set; } = true;
    public bool BlockOnMatch { get; set; } = true;
}
