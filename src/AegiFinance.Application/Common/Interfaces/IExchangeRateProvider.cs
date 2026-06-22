using AegiFinance.Application.Common.Models;

namespace AegiFinance.Application.Common.Interfaces;

public interface IExchangeRateProvider
{
    string Name { get; }
    Task<ExchangeRateResult?> GetRateAsync(string currencyCode, DateTime? date = null, CancellationToken cancellationToken = default);
}
