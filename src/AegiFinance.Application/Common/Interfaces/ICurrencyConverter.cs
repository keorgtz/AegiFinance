using AegiFinance.Application.Common.Models;

namespace AegiFinance.Application.Common.Interfaces;

public interface ICurrencyConverter
{
    Task<decimal> ConvertAsync(decimal amountInMXN, string targetCurrencyCode, DateTime? date = null, CancellationToken cancellationToken = default);
    Task<ExchangeRateResult> GetRateAsync(string targetCurrencyCode, DateTime? date = null, CancellationToken cancellationToken = default);
}
