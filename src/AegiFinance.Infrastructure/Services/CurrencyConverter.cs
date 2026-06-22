using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Common.Models;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using AegiFinance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AegiFinance.Infrastructure.Services;

public class CurrencyConverter : ICurrencyConverter
{
    private readonly ApplicationDbContext _context;
    private readonly IExchangeRateProvider _exchangeRateProvider;
    private readonly ILogger<CurrencyConverter> _logger;

    public CurrencyConverter(
        ApplicationDbContext context,
        IExchangeRateProvider exchangeRateProvider,
        ILogger<CurrencyConverter> logger)
    {
        _context = context;
        _exchangeRateProvider = exchangeRateProvider;
        _logger = logger;
    }

    public async Task<decimal> ConvertAsync(decimal amountInMXN, string targetCurrencyCode, DateTime? date = null, CancellationToken cancellationToken = default)
    {
        if (string.Equals(targetCurrencyCode, "MXN", StringComparison.OrdinalIgnoreCase))
        {
            return amountInMXN;
        }

        var rateResult = await GetRateAsync(targetCurrencyCode, date, cancellationToken);
        return amountInMXN * rateResult.Rate;
    }

    public async Task<ExchangeRateResult> GetRateAsync(string targetCurrencyCode, DateTime? date = null, CancellationToken cancellationToken = default)
    {
        if (string.Equals(targetCurrencyCode, "MXN", StringComparison.OrdinalIgnoreCase))
        {
            return new ExchangeRateResult
            {
                Rate = 1,
                Source = ExchangeRateSource.Manual.ToString(),
                EffectiveDate = date?.Date ?? DateTime.UtcNow.Date
            };
        }

        var effectiveDate = date?.Date ?? DateTime.UtcNow.Date;

        var manualRate = await _context.ExchangeRates
            .AsNoTracking()
            .Where(r => r.CurrencyCode == targetCurrencyCode && r.Source == ExchangeRateSource.Manual && r.EffectiveDate.Date <= effectiveDate)
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (manualRate is not null)
        {
            return new ExchangeRateResult
            {
                Rate = manualRate.RateFromMXN,
                Source = manualRate.Source.ToString(),
                EffectiveDate = manualRate.EffectiveDate
            };
        }

        var autoRate = await _context.ExchangeRates
            .AsNoTracking()
            .Where(r => r.CurrencyCode == targetCurrencyCode && r.Source == ExchangeRateSource.Auto && r.EffectiveDate.Date <= effectiveDate)
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (autoRate is not null)
        {
            return new ExchangeRateResult
            {
                Rate = autoRate.RateFromMXN,
                Source = autoRate.Source.ToString(),
                EffectiveDate = autoRate.EffectiveDate
            };
        }

        var providerResult = await _exchangeRateProvider.GetRateAsync(targetCurrencyCode, effectiveDate, cancellationToken);

        if (providerResult is not null)
        {
            var newRate = new ExchangeRate
            {
                Id = Guid.NewGuid(),
                CurrencyCode = targetCurrencyCode,
                RateToMXN = 1 / providerResult.Rate,
                RateFromMXN = providerResult.Rate,
                EffectiveDate = providerResult.EffectiveDate,
                Source = ExchangeRateSource.Auto,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ExchangeRates.Add(newRate);
            await _context.SaveChangesAsync(cancellationToken);

            return providerResult;
        }

        _logger.LogWarning("No se encontró tasa de cambio para {CurrencyCode} en la fecha {EffectiveDate}", targetCurrencyCode, effectiveDate);
        throw new InvalidOperationException($"No se encontró tasa de cambio para {targetCurrencyCode}.");
    }
}
