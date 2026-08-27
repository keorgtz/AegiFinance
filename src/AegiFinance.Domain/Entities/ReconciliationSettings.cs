namespace AegiFinance.Domain.Entities;

public sealed class ReconciliationSettings : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    public int DateToleranceDays { get; set; } = 5;
    public decimal AmountTolerance { get; set; } = 0.01m;
    public decimal SuggestionThreshold { get; set; } = 60m;
    public decimal AutoConfirmThreshold { get; set; } = 100m;
    public bool AllowAutoConfirmExact { get; set; }
    public int AmountWeight { get; set; } = 45;
    public int DateWeight { get; set; } = 20;
    public int ReferenceWeight { get; set; } = 20;
    public int ClientWeight { get; set; } = 10;
    public int PatternWeight { get; set; } = 5;
}
