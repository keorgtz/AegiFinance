using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ExchangeRates.Queries.GetExchangeRates;

public class GetExchangeRatesQueryHandler : IRequestHandler<GetExchangeRatesQuery, List<ExchangeRateDto>>
{
    private readonly IApplicationDbContext _context;

    public GetExchangeRatesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ExchangeRateDto>> Handle(GetExchangeRatesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ExchangeRates.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.CurrencyCode))
        {
            query = query.Where(r => r.CurrencyCode == request.CurrencyCode);
        }

        if (request.FromDate.HasValue)
        {
            query = query.Where(r => r.EffectiveDate >= request.FromDate.Value.Date);
        }

        if (request.ToDate.HasValue)
        {
            query = query.Where(r => r.EffectiveDate <= request.ToDate.Value.Date);
        }

        return await query
            .OrderByDescending(r => r.EffectiveDate)
            .Select(r => new ExchangeRateDto
            {
                Id = r.Id,
                CurrencyCode = r.CurrencyCode,
                RateToMXN = r.RateToMXN,
                RateFromMXN = r.RateFromMXN,
                EffectiveDate = r.EffectiveDate,
                Source = r.Source.ToString()
            })
            .ToListAsync(cancellationToken);
    }
}
