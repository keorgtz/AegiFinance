namespace AegiFinance.Application.Dtos;

public class ExchangeRateDto
{
    public Guid Id { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal RateToMXN { get; set; }
    public decimal RateFromMXN { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string Source { get; set; } = string.Empty;
}
