namespace AegiFinance.Application.Dtos;

public class SubscriptionPriceHistoryDto
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}
