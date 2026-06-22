using AegiFinance.Domain.Enums;

namespace AegiFinance.Domain.Entities;

public class ExchangeRate : BaseEntity
{
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal RateToMXN { get; set; }
    public decimal RateFromMXN { get; set; }
    public DateTime EffectiveDate { get; set; }
    public ExchangeRateSource Source { get; set; }
}
