namespace AegiFinance.Application.Dtos;

public class SubscriptionChangeLogDto
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public string ChangeType { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}
