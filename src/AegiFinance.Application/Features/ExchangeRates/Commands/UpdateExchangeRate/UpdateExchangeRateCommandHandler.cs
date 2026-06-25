using AegiFinance.Application.Common.Interfaces;
using AegiFinance.Application.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AegiFinance.Application.Features.ExchangeRates.Commands.UpdateExchangeRate;

public class UpdateExchangeRateCommandHandler : IRequestHandler<UpdateExchangeRateCommand, ExchangeRateDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateExchangeRateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExchangeRateDto> Handle(UpdateExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var rate = await _context.ExchangeRates
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (rate is null)
        {
            throw new InvalidOperationException("El tipo de cambio no existe.");
        }

        rate.RateToMXN = request.RateToMXN;
        rate.RateFromMXN = request.RateFromMXN;
        rate.EffectiveDate = request.EffectiveDate;

        await _context.SaveChangesAsync(cancellationToken);

        return new ExchangeRateDto
        {
            Id = rate.Id,
            CurrencyCode = rate.CurrencyCode,
            RateToMXN = rate.RateToMXN,
            RateFromMXN = rate.RateFromMXN,
            EffectiveDate = rate.EffectiveDate,
            Source = rate.Source.ToString()
        };
    }
}
