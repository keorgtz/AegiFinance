namespace AegiFinance.Application.Dtos;

public class BillingGenerationLogDto
{
    public Guid Id { get; set; }
    public Guid? BillingCycleId { get; set; }
    public string? BillingCycleLabel { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemsGenerated { get; set; }
    public string? Errors { get; set; }
    public Guid? TriggeredBy { get; set; }
}
