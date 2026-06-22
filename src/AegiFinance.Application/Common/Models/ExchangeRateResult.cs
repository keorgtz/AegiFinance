namespace AegiFinance.Application.Common.Models;

public class ExchangeRateResult
{
    public decimal Rate { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime EffectiveDate { get; set; }
}
