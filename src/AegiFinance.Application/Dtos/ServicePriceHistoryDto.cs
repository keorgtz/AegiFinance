namespace AegiFinance.Application.Dtos;

public class ServicePriceHistoryDto
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}
