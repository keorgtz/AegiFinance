namespace AegiFinance.Domain.Entities;

public class BillingGenerationLog : AuditableEntity
{
    public Guid? BillingCycleId { get; set; }
    public BillingCycle? BillingCycle { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string Status { get; set; } = null!; // Success, Partial, Failed
    public int ItemsGenerated { get; set; }
    public string? Errors { get; set; }
    public Guid? TriggeredBy { get; set; }
}
