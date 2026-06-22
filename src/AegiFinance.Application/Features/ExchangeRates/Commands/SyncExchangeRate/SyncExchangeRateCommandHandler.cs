using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using AegiFinance.Domain.Entities;
using AegiFinance.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ExchangeRates.Commands.SyncExchangeRate;

public class SyncExchangeRateCommandHandler : IRequestHandler<SyncExchangeRateCommand, ExchangeRateDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IExchangeRateProvider _exchangeRateProvider;

    public SyncExchangeRateCommandHandler(IApplicationDbContext context, IExchangeRateProvider exchangeRateProvider)
    {
        _context = context;
        _exchangeRateProvider = exchangeRateProvider;
    }

    public async Task<ExchangeRateDto?> Handle(SyncExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var result = await _exchangeRateProvider.GetRateAsync(request.CurrencyCode, request.Date, cancellationToken);

        if (result is null)
        {
            return null;
        }

        var effectiveDate = result.EffectiveDate.Date;
        var existing = await _context.ExchangeRates
            .FirstOrDefaultAsync(r =>
                r.CurrencyCode == request.CurrencyCode &&
                r.EffectiveDate.Date == effectiveDate &&
                r.Source == ExchangeRateSource.Auto,
                cancellationToken);

        if (existing is not null)
        {
            existing.RateToMXN = result.Rate;
            existing.RateFromMXN = 1 / result.Rate;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var rate = new ExchangeRate
            {
                Id = Guid.NewGuid(),
                CurrencyCode = request.CurrencyCode,
                RateToMXN = result.Rate,
                RateFromMXN = 1 / result.Rate,
                EffectiveDate = result.EffectiveDate,
                Source = ExchangeRateSource.Auto,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ExchangeRates.Add(rate);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new ExchangeRateDto
        {
            Id = existing?.Id ?? Guid.Empty,
            CurrencyCode = request.CurrencyCode,
            RateToMXN = result.Rate,
            RateFromMXN = 1 / result.Rate,
            EffectiveDate = result.EffectiveDate,
            Source = ExchangeRateSource.Auto.ToString()
        };
    }
}
