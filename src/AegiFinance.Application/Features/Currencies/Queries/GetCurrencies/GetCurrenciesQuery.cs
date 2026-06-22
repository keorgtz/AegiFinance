using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.Currencies.Queries.GetCurrencies;

public class GetCurrenciesQuery : IRequest<List<CurrencyConfigDto>>
{
}

public class GetCurrenciesQueryHandler : IRequestHandler<GetCurrenciesQuery, List<CurrencyConfigDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCurrenciesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CurrencyConfigDto>> Handle(GetCurrenciesQuery request, CancellationToken cancellationToken)
    {
        return await _context.CurrencyConfigs
            .AsNoTracking()
            .OrderBy(c => c.Code)
            .Select(c => new CurrencyConfigDto
            {
                Id = c.Id,
                Code = c.Code,
                Name = c.Name,
                Symbol = c.Symbol,
                IsActive = c.IsActive,
                IsDefault = c.IsDefault
            })
            .ToListAsync(cancellationToken);
    }
}
